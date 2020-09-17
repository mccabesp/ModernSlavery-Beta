using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public class SearchBusinessLogic : ISearchBusinessLogic
    {
        #region Dependencies
        public SearchOptions SearchOptions { get; }
        public readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly ISearchRepository<OrganisationSearchModel> _organisationSearchRepository;
        public IAuditLogger SearchLog { get; }
        public bool Disabled => _organisationSearchRepository.Disabled;
        #endregion

        #region Constructor
        public SearchBusinessLogic(
            SearchOptions searchOptions,
            SharedOptions sharedOptions,
            IDataRepository dataRepository,
            IReportingDeadlineHelper reportingDeadlineHelper,
            ISearchRepository<OrganisationSearchModel> organisationSearchRepository,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog
        )
        {
            SearchOptions = searchOptions;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _organisationSearchRepository = organisationSearchRepository;
            SearchLog = searchLog;
        }
        #endregion

        #region RefreshSearchDocuments
        public async Task RefreshSearchDocumentsAsync(IEnumerable<Organisation> organisations)
        {
            //Remove the test organisations
            if (!string.IsNullOrWhiteSpace(_sharedOptions.TestPrefix))
                organisations = organisations.Where(o => !o.OrganisationName.StartsWithI(_sharedOptions.TestPrefix));

            //Remove those which are not to be searched
            organisations = LookupSearchableOrganisations(organisations.ToArray()).ToList();

            //Make sure we have an index
            await _organisationSearchRepository.CreateIndexIfNotExistsAsync(_organisationSearchRepository.IndexName).ConfigureAwait(false);

            //Get the old indexes
            var oldSearchModels = await _organisationSearchRepository.ListDocumentsAsync(nameof(OrganisationSearchModel.SearchDocumentKey));

            //Create the new indexes
            var newSearchModels = CreateOrganisationSearchModels(organisations);

            if (newSearchModels.Any()) await _organisationSearchRepository.AddOrUpdateDocumentsAsync(newSearchModels).ConfigureAwait(false);

            //Remove the retired models
            var retiredModels = oldSearchModels.Except(newSearchModels);
            if (retiredModels.Any()) await _organisationSearchRepository.DeleteDocumentsAsync(retiredModels).ConfigureAwait(false);
        }

        private IEnumerable<Organisation> LookupSearchableOrganisations(params Organisation[] organisations)
        {
            return organisations.Where(
                o => o.Status == OrganisationStatuses.Active
                     && o.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope ||
                                       sc.ScopeStatus == ScopeStatuses.PresumedInScope)));
        }

        public async Task RefreshSearchDocumentsAsync()
        {
            //Get all the organisations
            var organisations = await _dataRepository.ToListAsync<Organisation>().ConfigureAwait(false);
            await RefreshSearchDocumentsAsync(organisations);
        }

        public async Task RefreshSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the organisations to include or exclude from search
            var newSearchModels = LookupSearchableOrganisations(organisation).SelectMany(o => CreateOrganisationSearchModels(o));

            //Remove those models not for the selected statementDeadlineYear
            if (statementDeadlineYear > 0) newSearchModels = newSearchModels.Where(m => m.StatementDeadlineYear == statementDeadlineYear);

             //Get the old indexes for statements
            var retiredModels = await ListSearchDocumentsAsync(organisation, statementDeadlineYear);

            //Batch update the included organisations
            if (newSearchModels.Any()) await _organisationSearchRepository.AddOrUpdateDocumentsAsync(newSearchModels);

            //Remove the retired models
            retiredModels = retiredModels.Except(newSearchModels).ToList();
            await RemoveSearchDocumentsAsync(retiredModels);
        }
        #endregion

        #region RemoveSearchDocuments 
        public async Task RemoveSearchDocumentsAsync(Organisation organisation)
        {

            //Get the old indexes for statements
            var retiredModels = await ListSearchDocumentsAsync(organisation);

            //Remove the retired models
            await RemoveSearchDocumentsAsync(retiredModels);
        }

        public async Task RemoveSearchDocumentsAsync(IEnumerable<OrganisationSearchModel> searchIndexes)
        {
            //Remove the indexes
            if (searchIndexes.Any()) await _organisationSearchRepository.DeleteDocumentsAsync(searchIndexes).ConfigureAwait(false);
        }
        #endregion

        #region ListSearchDocuments
        public async Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0, bool keyOnly = true)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the old indexes for statements
            var filter = $"{nameof(OrganisationSearchModel.ParentOrganisationId)} eq {organisation.OrganisationId}";
            if (statementDeadlineYear>0)filter += $" and {nameof(OrganisationSearchModel.StatementDeadlineYear)} eq {statementDeadlineYear}";

            return await _organisationSearchRepository.ListDocumentsAsync(selectFields: keyOnly ? nameof(OrganisationSearchModel.SearchDocumentKey) : null, filter: filter);
        }
        #endregion

        #region Create OrganisationSearchModel
        /// <summary>
        /// Returns a list of fully populated search index documents for specific organisations
        /// </summary>
        /// <param name="organisation">The organisations whos index documents we want to return</param>
        /// <returns>The list of fully populated search index documents</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModels(IEnumerable<Organisation> organisations)
        {
            var unreportedSearchModels = new Dictionary<long, OrganisationSearchModel>();
            var reportedOrganisationIds = new HashSet<long>();

            //Get statement models for all organisations
            foreach (var organisation in organisations)
                foreach (var searchModel in CreateOrganisationSearchModels(organisation))
                {
                    if (searchModel.StatementDeadlineYear == null && searchModel.ChildStatementOrganisationId == null)
                        unreportedSearchModels[searchModel.ParentOrganisationId] = searchModel;
                    else
                    {
                        reportedOrganisationIds.Add(searchModel.ParentOrganisationId);
                        if (searchModel.ChildStatementOrganisationId.HasValue)
                            reportedOrganisationIds.Add(searchModel.ChildStatementOrganisationId.Value);

                        yield return searchModel;
                    }
                }

            //Remove any reported organisations from the unreported
            reportedOrganisationIds.ForEach(id =>
            {
                if (unreportedSearchModels.ContainsKey(id)) unreportedSearchModels.Remove(id);
            });

            //Return the search document for any organisation who has never reported
            foreach (var organisationId in unreportedSearchModels.Keys)
                yield return unreportedSearchModels[organisationId];
        }

        /// <summary>
        /// Returns a list of fully populated search index documents for a specific organisation
        /// </summary>
        /// <param name="organisation">The organisation whos index documents we want to return</param>
        /// <returns>The list of fully populated search index documents</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModels(Organisation organisation)
        {
            foreach (var reportingDeadline in _reportingDeadlineHelper.GetReportingDeadlines(organisation.SectorType))
                foreach (var organisationSearchModel in CreateOrganisationSearchModels(organisation, reportingDeadline.Year))
                    yield return organisationSearchModel;
        }

        /// <summary>
        /// Returns a list of fully populated search index documents for a specific organisation for a specific reporting year
        /// </summary>
        /// <param name="organisation">The organisation whos index documents we want to return</param>
        /// <param name="reportingDeadlineYear">The reporting year</param>
        /// <returns>The list of fully populated search index documents for the specified year</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModels(Organisation organisation, int reportingDeadlineYear)
        {
            var submittedStatement = organisation.Statements.FirstOrDefault(s => s.Status == StatementStatuses.Submitted && s.SubmissionDeadline.Year == reportingDeadlineYear);

            // Get the abbreviations for the organisation name
            var abbreviations = CreateOrganisationNameAbbreviations(organisation.OrganisationName);

            var parentStatementModel = new OrganisationSearchModel
            {
                StatementId = submittedStatement?.StatementId,
                ParentOrganisationId = organisation.OrganisationId,
                StatementDeadlineYear = reportingDeadlineYear,
                OrganisationName = organisation.OrganisationName,
                CompanyNumber = organisation.CompanyNumber,
                Address = organisation.LatestAddress.GetAddressString(),
                SectorTypeIds = submittedStatement?.Sectors.Select(s => (int)s.StatementSectorTypeId).ToArray(),
                Turnover = submittedStatement == null ? (int?)null : (int)StatementModel.GetTurnover(submittedStatement),
                Modified = submittedStatement == null ? organisation.Modified : submittedStatement.Modified,
                IsParent = submittedStatement == null ? false : submittedStatement.StatementOrganisations.Any(),
                Abbreviations = CreateOrganisationNameAbbreviations(organisation.OrganisationName),
                PartialNameForCompleteTokenSearches = organisation.OrganisationName,
                PartialNameForSuffixSearches = organisation.OrganisationName
            };
            yield return parentStatementModel.SetSearchDocumentKey();

            if (submittedStatement!=null)
                foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
                {
                    var childStatementModel = new OrganisationSearchModel
                    {
                        StatementId = parentStatementModel.StatementId,
                        ParentOrganisationId = parentStatementModel.ParentOrganisationId,
                        StatementDeadlineYear = parentStatementModel.StatementDeadlineYear,
                        ParentName = parentStatementModel.OrganisationName,
                        SectorTypeIds = parentStatementModel.SectorTypeIds,
                        Turnover = parentStatementModel.Turnover,
                        Modified = parentStatementModel.Modified,
                        ChildStatementOrganisationId = childOrganisation.StatementOrganisationId,
                        ChildOrganisationId = childOrganisation.OrganisationId
                    };

                    if (childOrganisation.Organisation == null)
                    {
                        childStatementModel.OrganisationName = childOrganisation.OrganisationName;
                        childStatementModel.CompanyNumber = organisation.CompanyNumber;
                        childStatementModel.Address = organisation.LatestAddress.GetAddressString();
                    }
                    else
                    {
                        childStatementModel.OrganisationName = childOrganisation.Organisation.OrganisationName;
                        childStatementModel.CompanyNumber = childOrganisation.Organisation.CompanyNumber;
                        childStatementModel.Address = childOrganisation.Organisation.LatestAddress.GetAddressString();
                    }
                    childStatementModel.Abbreviations = CreateOrganisationNameAbbreviations(childStatementModel.OrganisationName);
                    childStatementModel.PartialNameForCompleteTokenSearches = childStatementModel.OrganisationName;
                    childStatementModel.PartialNameForSuffixSearches = childStatementModel.OrganisationName;

                    yield return childStatementModel.SetSearchDocumentKey();
                }
        }

        /// <summary>
        /// Returns a list of search index documents for a specific organisation with only SearchDocumentKey populated
        /// </summary>
        /// <param name="organisation">The organisation whos index documents we want to return</param>
        /// <returns>The list of search index documents with only SearchDocumentKey populated</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModelKeys(Organisation organisation)
        {
            foreach (var reportingDeadline in _reportingDeadlineHelper.GetReportingDeadlines(organisation.SectorType))
                foreach (var organisationSearchModel in CreateOrganisationSearchModelKeys(organisation, reportingDeadline.Year))
                    yield return organisationSearchModel;
        }

        /// <summary>
        /// Returns a list of search index document keys for a specific organisation for a specific reporting year
        /// </summary>
        /// <param name="organisation">The organisation whos index document keys we want to return</param>
        /// <param name="reportingDeadlineYear">The reporting year</param>
        /// <returns>The list of search index document keys for the specified year</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModelKeys(Organisation organisation, int reportingDeadlineYear)
        {
            var submittedStatement = organisation.Statements.FirstOrDefault(s => s.Status == StatementStatuses.Submitted && s.SubmissionDeadline.Year == reportingDeadlineYear);

            var parentStatementModel = new OrganisationSearchModel
            {
                ParentOrganisationId = organisation.OrganisationId,
                StatementDeadlineYear = reportingDeadlineYear,
            };
            yield return parentStatementModel.SetSearchDocumentKey();

            foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
            {
                var childStatementModel = new OrganisationSearchModel
                {
                    ParentOrganisationId = parentStatementModel.ParentOrganisationId,
                    StatementDeadlineYear = parentStatementModel.StatementDeadlineYear,
                    ChildStatementOrganisationId = childOrganisation.StatementOrganisationId,
                };

                yield return childStatementModel.SetSearchDocumentKey();
            }
        }

        /// <summary>
        /// Returns a list of abbreviations for a set of organisation names
        /// </summary>
        /// <param name="names">The organisation names to parse</param>
        /// <returns>The list of abbreviations</returns>
        private string[] CreateOrganisationNameAbbreviations(params string[] names)
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
            return abbreviations.ToArray();
        }
        #endregion

        #region SearchDocuments
        public async Task<PagedResult<OrganisationSearchModel>> SearchDocumentsAsync(string searchText,
            int currentPage,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string orderBy = null,
            Dictionary<string, Dictionary<object, long>> facets = null,
            string filter = null,
            string highlights = null,
            SearchModes searchMode = SearchModes.Any)
        {
            return await _organisationSearchRepository.SearchDocumentsAsync(searchText,currentPage,pageSize,searchFields,selectFields,orderBy,facets, filter,highlights,searchMode);
        }
        #endregion
    }
}