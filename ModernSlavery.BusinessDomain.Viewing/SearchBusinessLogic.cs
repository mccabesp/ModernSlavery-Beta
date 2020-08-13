using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public class SearchBusinessLogic : ISearchBusinessLogic
    {
        public SearchOptions SearchOptions { get; }
        public readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        public ISearchRepository<OrganisationSearchModel> OrganisationSearchRepository { get; set; }
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        public IAuditLogger SearchLog { get; }

        public SearchBusinessLogic(
            SearchOptions searchOptions,
            SharedOptions sharedOptions,
            IDataRepository dataRepository,
            ISearchRepository<OrganisationSearchModel> organisationSearchRepository,
            IOrganisationBusinessLogic organisationBusinessLogic,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog
        )
        {
            SearchOptions = searchOptions;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
            OrganisationSearchRepository = organisationSearchRepository;
            _organisationBusinessLogic = organisationBusinessLogic;
            SearchLog = searchLog;
        }

        public async Task UpdateOrganisationSearchIndexAsync(IEnumerable<Organisation> organisations)
        {
            //Remove the test organisations
            if (!string.IsNullOrWhiteSpace(_sharedOptions.TestPrefix))
                organisations=organisations.Where(o => !o.OrganisationName.StartsWithI(_sharedOptions.TestPrefix));

            //Remove those which are not to be searched
            organisations = LookupSearchableOrganisations(organisations.ToArray()).ToList();

            //When debugging just update 100 
            if (Debugger.IsAttached) organisations = organisations.Take(100).ToList();

            //Make sure we have an index
            await OrganisationSearchRepository.CreateIndexIfNotExistsAsync(OrganisationSearchRepository.IndexName).ConfigureAwait(false);

            //Get the old indexes
            var oldSearchModels = await OrganisationSearchRepository.ListAsync();

            //Create the new indexes
            var newSearchModels = GetOrganisationSearchModels(organisations);

            if (newSearchModels.Any()) await OrganisationSearchRepository.RefreshIndexDataAsync(newSearchModels).ConfigureAwait(false);

            //Remove the retired models
            var retiredModels = oldSearchModels.Except(newSearchModels);
            if (retiredModels.Any()) await OrganisationSearchRepository.RemoveFromIndexAsync(retiredModels).ConfigureAwait(false);
        }

        //Returns a list of organisaations to include in search indexes
        public IEnumerable<Organisation> LookupSearchableOrganisations(params Organisation[] organisations)
        {
            return organisations.Where(
                o => o.Status == OrganisationStatuses.Active
                     && o.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope ||
                                       sc.ScopeStatus == ScopeStatuses.PresumedInScope)));
        }

        public async Task UpdateOrganisationSearchIndexAsync()
        {
            //Get all the organisations
            var organisations = await _dataRepository.ToListAsync<Organisation>().ConfigureAwait(false);
            await UpdateOrganisationSearchIndexAsync(organisations);
        }

        //Add or remove an organisation from the search indexes based on status and scope
        public async Task UpdateOrganisationSearchIndexAsync(Organisation organisation)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the organisations to include or exclude from search
            var newSearchModels = LookupSearchableOrganisations(organisation).SelectMany(o => GetOrganisationSearchModelKeys(o));

            //Get the old indexes for statements
            var retiredModels = await GetOrganisationSearchIndexesAsync(organisation);

            //Batch update the included organisations
            if (newSearchModels.Any())await OrganisationSearchRepository.AddOrUpdateIndexDataAsync(newSearchModels);

            //Remove the retired models
            retiredModels = retiredModels.Except(newSearchModels).ToList();
            await RemoveSearchIndexesAsync(retiredModels);
        }

        //Remove an organisation from the search indexes
        public async Task RemoveOrganisationSearchIndexesAsync(Organisation organisation)
        {

            //Get the old indexes for statements
            var retiredModels = await GetOrganisationSearchIndexesAsync(organisation);

            //Remove the retired models
            await RemoveSearchIndexesAsync(retiredModels);
        }

        public async Task RemoveSearchIndexesAsync(IEnumerable<OrganisationSearchModel> searchIndexes)
        {
            //Remove the indexes
            if (searchIndexes.Any()) await OrganisationSearchRepository.RemoveFromIndexAsync(searchIndexes).ConfigureAwait(false);
        }

        //Remove an organisation from the search indexes
        public async Task<IEnumerable<OrganisationSearchModel>> GetOrganisationSearchIndexesAsync(Organisation organisation)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the old indexes for statements
            var indexModels = new HashSet<OrganisationSearchModel>();
            var filter = new Dictionary<string, List<string>>();
            filter[nameof(OrganisationSearchModel.StatementId)] = organisation.Statements.Select(s => s.StatementId.ToString()).ToList();
            indexModels.AddRange(await OrganisationSearchRepository.ListKeysAsync(filter));

            //Get the old indexed for orgs with no statements
            filter.Clear();
            filter[nameof(OrganisationSearchModel.OrganisationId)] = new[] { organisation.OrganisationId.ToString() }.ToList();
            filter[nameof(OrganisationSearchModel.StatementDeadlineYear)] = new[] { "'null'" }.ToList();
            indexModels.AddRange(await OrganisationSearchRepository.ListKeysAsync(filter));

            return indexModels.AsEnumerable();
        }

        #region Create SearchModels
        private IEnumerable<OrganisationSearchModel> GetOrganisationSearchModels(IEnumerable<Organisation> organisations)
        {
            var unreportedSearchModels = new Dictionary<string, OrganisationSearchModel>();
            var reportedOrganisationIds = new HashSet<string>();

            //Get statement models for all organisations
            foreach (var organisation in organisations)
                foreach (var searchModel in GetOrganisationSearchModel(organisation))
                {
                    if (searchModel.StatementDeadlineYear == null && searchModel.ChildStatementOrganisationId == null)
                        unreportedSearchModels[searchModel.OrganisationId.ToString()] = searchModel;
                    else
                    {
                        reportedOrganisationIds.Add(searchModel.OrganisationId.ToString());
                        if (searchModel.ChildStatementOrganisationId!=null && !string.IsNullOrWhiteSpace(searchModel.ChildStatementOrganisationId))
                            reportedOrganisationIds.Add(searchModel.ChildStatementOrganisationId);                         

                        yield return searchModel;
                    }
                }

            //Remove any reported organisations from the unreported
            reportedOrganisationIds.ForEach(id => {
                if (unreportedSearchModels.ContainsKey(id)) unreportedSearchModels.Remove(id);
            });

            //Return the search document for any organisation who has never reported
            foreach (var organisationId in unreportedSearchModels.Keys)
                yield return unreportedSearchModels[organisationId];
        }

        private IEnumerable<OrganisationSearchModel> GetOrganisationSearchModel(Organisation organisation)
        {
            var submittedStatements = organisation.Statements.Where(s => s.Status == StatementStatuses.Submitted);

            // Get the abbreviations for the organisation name
            var abbreviations = GetOrganisationNameAbbreviations(organisation);

            // extract the prev org name (if exists)
            var prevOrganisationName = organisation.GetPreviousName();

            var searchModel = new OrganisationSearchModel
            {
                OrganisationId = organisation.OrganisationId,
                Name = organisation.OrganisationName,
                PreviousName = prevOrganisationName,
                PartialNameForSuffixSearches = organisation.OrganisationName,
                PartialNameForCompleteTokenSearches = organisation.OrganisationName,
                Abbreviations = abbreviations.ToArray(),
                Address = organisation.LatestAddress?.GetAddressString(),
            };

            //Return the search document for any organisation who has never reported
            if (!submittedStatements.Any())
                yield return searchModel;

            foreach (var submittedStatement in submittedStatements)
            {
                //Return the search document for each submitted statement for organisation
                var parentStatementModel = searchModel.GetClone();
                parentStatementModel.StatementDeadlineYear = submittedStatement.SubmissionDeadline.Year.ToString();
                parentStatementModel.Turnover = (byte)StatementModel.GetTurnover(submittedStatement);
                parentStatementModel.StatementId = submittedStatement.StatementId;
                parentStatementModel.SectorTypeIds = submittedStatement.Sectors.Select(s => s.StatementSectorTypeId).ToArray();
                parentStatementModel.IsParent = submittedStatement.StatementOrganisations.Any();

                //Mark this organisation as reported
                yield return parentStatementModel.SetSearchDocumentKey();

                foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
                {
                    //Return the search document for each child organisation in a group submitted statement
                    var childStatementModel = parentStatementModel.GetClone();
                    childStatementModel.ChildStatementOrganisationId = childOrganisation.StatementOrganisationId.ToString();
                    childStatementModel.IsParent = false;

                    if (childOrganisation.Organisation == null)
                    {
                        childStatementModel.OrganisationId = 0;
                        childStatementModel.Name = childOrganisation.OrganisationName;
                        childStatementModel.PartialNameForSuffixSearches = childOrganisation.OrganisationName;
                        childStatementModel.PartialNameForCompleteTokenSearches = childOrganisation.OrganisationName;
                    }
                    else
                    {
                        childStatementModel.OrganisationId = childOrganisation.OrganisationId.Value;
                        childStatementModel.Name = childOrganisation.Organisation.OrganisationName;
                        childStatementModel.PartialNameForSuffixSearches = childStatementModel.Name;
                        childStatementModel.PartialNameForCompleteTokenSearches = childStatementModel.Name;
                        childStatementModel.Address = childOrganisation.Organisation.LatestAddress?.GetAddressString();
                        abbreviations = GetOrganisationNameAbbreviations(childOrganisation.Organisation);
                        abbreviations.AddRange(childStatementModel.Abbreviations);
                        childStatementModel.Abbreviations = abbreviations.ToArray();
                    }

                    yield return childStatementModel.SetSearchDocumentKey();
                }
            }
        }

        private IEnumerable<OrganisationSearchModel> GetOrganisationSearchModelKeys(Organisation organisation)
        {
            var submittedStatements = organisation.Statements.Where(s => s.Status == StatementStatuses.Submitted);

            var unreportedSearchModels = new Dictionary<long, OrganisationSearchModel>();
            var reportedOrganisationIds = new HashSet<long>();

            var searchModel = new OrganisationSearchModel
            {
                OrganisationId = organisation.OrganisationId,
            };
            unreportedSearchModels[searchModel.OrganisationId] = searchModel;

            foreach (var submittedStatement in submittedStatements)
            {
                //Return the search document for each submitted statement for organisation
                var parentStatementModel = searchModel.GetClone();
                parentStatementModel.StatementDeadlineYear = submittedStatement.SubmissionDeadline.Year.ToString();

                //Mark this organisation as reported
                reportedOrganisationIds.Add(parentStatementModel.OrganisationId);
                yield return parentStatementModel.SetSearchDocumentKey();

                foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
                {
                    //Return the search document for each child organisation in a group submitted statement
                    var childStatementModel = parentStatementModel.GetClone();
                    childStatementModel.ChildStatementOrganisationId = childOrganisation.StatementOrganisationId.ToString();

                    if (childOrganisation.Organisation == null)
                    {
                        childStatementModel.OrganisationId = 0;
                    }
                    else
                    {
                        childStatementModel.OrganisationId = childOrganisation.OrganisationId.Value;

                        //Mark this organisation as reported
                        reportedOrganisationIds.Add(childStatementModel.OrganisationId);
                    }

                    yield return childStatementModel.SetSearchDocumentKey();
                }
            }

            //Remove any reported organisations from the unreported
            reportedOrganisationIds.ForEach(id => {
                if (unreportedSearchModels.ContainsKey(id)) unreportedSearchModels.Remove(id);
            });

            //Return the search document for any organisation who has never reported
            foreach (var organisationId in unreportedSearchModels.Keys)
                yield return unreportedSearchModels[organisationId].SetSearchDocumentKey();
        }

        private SortedSet<string> GetOrganisationNameAbbreviations(Organisation organisation)
        {
            // Get the last two names for the org. Most recent name first
            var names = organisation.OrganisationNames.OrderByDescending(n => n.Created).Select(n => n.Name).Take(2).ToArray();
            return GetOrganisationNameAbbreviations(names);
        }

        private SortedSet<string> GetOrganisationNameAbbreviations(params string[] names)
        {
            // Get the last two names for the org. Most recent name first
            var abbreviations = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            names.ForEach(n => abbreviations.Add(n.ToAbbr()));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".")));
            var excludes = new[]
                {"Ltd", "Limited", "PLC", "Corporation", "Incorporated", "LLP", "The", "And", "&", "For", "Of", "To"};
            names.ForEach(n => abbreviations.Add(n.ToAbbr(excludeWords: excludes)));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".", excludeWords: excludes)));

            abbreviations.RemoveWhere(a => string.IsNullOrWhiteSpace(a));

            //Remove the current name because it is specifically included in search document
            abbreviations.Remove(names[0]);

            //Remove the previos name because it is specifically included in search document
            if (names.Length > 1)
            {
                var prevOrganisationName = names[1];
                if (!string.IsNullOrWhiteSpace(prevOrganisationName))
                    abbreviations.Remove(prevOrganisationName);
            }
            return abbreviations;
        }
        #endregion
    }
}