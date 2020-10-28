using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using AutoMapper;
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
        private readonly IMapper _autoMapper;
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
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog,
            IMapper autoMapper)
        {
            SearchOptions = searchOptions;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _organisationSearchRepository = organisationSearchRepository;
            SearchLog = searchLog;
            _autoMapper = autoMapper;
        }
        #endregion

        #region RefreshSearchDocuments

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

        public async Task RefreshSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the organisations to include or exclude from search
            var newSearchModels = LookupSearchableOrganisations(organisation).SelectMany(o => CreateOrganisationSearchModels(o));

            //Remove those models not for the selected statementDeadlineYear
            if (statementDeadlineYear > 0) newSearchModels = newSearchModels.Where(m => m.SubmissionDeadlineYear == statementDeadlineYear);

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
        public async Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsAsync(Organisation organisation, int submissionDeadlineYear = 0, bool keyOnly = true)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the old indexes for statements
            var filter = $"{nameof(OrganisationSearchModel.ParentOrganisationId)} eq {organisation.OrganisationId}";
            if (submissionDeadlineYear > 0) filter += $" and {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} eq {submissionDeadlineYear}";

            return await _organisationSearchRepository.ListDocumentsAsync(selectFields: keyOnly ? nameof(OrganisationSearchModel.SearchDocumentKey) : null, filter: filter);
        }

        public async Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsAsync(IEnumerable<int> submissionDeadlineYears = null)
        {
            //Get the old indexes for statements
            var filter = $"{nameof(OrganisationSearchModel.StatementId)} ne null";
            if (submissionDeadlineYears.Any())
            {
                string yearFilter = null;
                var deadlineQuery = submissionDeadlineYears.Select(x => $"{nameof(OrganisationSearchModel.SubmissionDeadlineYear)} eq {x}");
                yearFilter += string.Join(" or ", deadlineQuery);
                if (!string.IsNullOrWhiteSpace(yearFilter)) filter += $" and ({yearFilter})";
            }

            return await _organisationSearchRepository.ListDocumentsAsync(filter: filter);
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
                    if (searchModel.SubmissionDeadlineYear == null && searchModel.ChildStatementOrganisationId == null)
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
                GroupSubmission = submittedStatement == null ? false : submittedStatement.StatementOrganisations.Any(),

                StatementUrl = submittedStatement?.StatementUrl,
                StatementEmail = submittedStatement?.StatementEmail,
                StatementStartDate = submittedStatement?.StatementStartDate,
                StatementEndDate = submittedStatement?.StatementEndDate,
                ApprovingPerson = submittedStatement?.ApprovingPerson,
                ApprovedDate = submittedStatement?.ApprovedDate,

                IncludesStructure = submittedStatement?.IncludesStructure,
                StructureDetails = submittedStatement?.StructureDetails,
                IncludesPolicies = submittedStatement?.IncludesPolicies,
                PolicyDetails = submittedStatement?.PolicyDetails,
                IncludesRisks = submittedStatement?.IncludesRisks,
                RisksDetails = submittedStatement?.RisksDetails,
                IncludesDueDiligence = submittedStatement?.IncludesDueDiligence,
                DueDiligenceDetails = submittedStatement?.DueDiligenceDetails,
                IncludesTraining = submittedStatement?.IncludesTraining,
                TrainingDetails = submittedStatement?.TrainingDetails,
                IncludesGoals = submittedStatement?.IncludesGoals,
                GoalsDetails = submittedStatement?.GoalsDetails,

                Sectors = submittedStatement?.Sectors.Select(s => new OrganisationSearchModel.KeyName { Key = s.StatementSectorTypeId, Name = s.StatementSectorType.Description }).ToList(),
                OtherSectors = submittedStatement?.OtherSectors,

                Turnover = submittedStatement==null ? null : _autoMapper.Map<OrganisationSearchModel.KeyName>(submittedStatement.Turnover),
                StatementYears = submittedStatement==null ? null : _autoMapper.Map<OrganisationSearchModel.KeyName>(submittedStatement.StatementYears),

                Summary = submittedStatement == null ? null : _autoMapper.Map<OrganisationSearchModel.SummarySearchModel>(submittedStatement.Summary),

                StatementId = submittedStatement?.StatementId,
                ParentOrganisationId = organisation.OrganisationId,
                SubmissionDeadlineYear = reportingDeadlineYear,
                OrganisationName = organisation.OrganisationName,
                CompanyNumber = organisation.CompanyNumber,
                SectorType = new OrganisationSearchModel.KeyName { Key = (int)organisation.SectorType, Name = organisation.SectorType.ToString() },
                Address = AddressModel.Create(organisation.LatestAddress),

                Modified = submittedStatement == null ? organisation.Modified : submittedStatement.Modified,
                Abbreviations = CreateOrganisationNameAbbreviations(organisation.OrganisationName),
                PartialNameForCompleteTokenSearches = organisation.OrganisationName,
                PartialNameForSuffixSearches = organisation.OrganisationName
            };

            yield return parentStatementModel.SetSearchDocumentKey();

            if (submittedStatement != null)
                foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
                {
                    var childStatementModel = _autoMapper.Map<OrganisationSearchModel>(parentStatementModel);

                    childStatementModel.OrganisationName = childOrganisation.OrganisationName;
                    childStatementModel.ParentName = parentStatementModel.OrganisationName;
                    childStatementModel.ChildStatementOrganisationId = childOrganisation.StatementOrganisationId;
                    childStatementModel.ChildOrganisationId = childOrganisation.OrganisationId;

                    if (childOrganisation.Organisation != null)
                    {
                        childStatementModel.OrganisationName = childOrganisation.Organisation.OrganisationName;
                        childStatementModel.CompanyNumber = childOrganisation.Organisation.CompanyNumber;
                        childStatementModel.Address = AddressModel.Create(childOrganisation.Organisation.LatestAddress);
                        childStatementModel.SectorType = new OrganisationSearchModel.KeyName { Key = (int)organisation.SectorType, Name = organisation.SectorType.ToString() };
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
                SubmissionDeadlineYear = reportingDeadlineYear,
            };
            yield return parentStatementModel.SetSearchDocumentKey();

            foreach (var childOrganisation in submittedStatement.StatementOrganisations.Where(go => go.Included))
            {
                var childStatementModel = new OrganisationSearchModel
                {
                    ParentOrganisationId = parentStatementModel.ParentOrganisationId,
                    SubmissionDeadlineYear = parentStatementModel.SubmissionDeadlineYear,
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

        #region GetOrganisationAsync
        public async Task<OrganisationSearchModel> GetOrganisationAsync(long parentOrganisationId, int submissionDeadlineYear)
        {
            //Ensure we have an organisation
            if (parentOrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(parentOrganisationId));

            //Ensure we have an year
            if (submissionDeadlineYear == 0) throw new ArgumentOutOfRangeException(nameof(submissionDeadlineYear));

            //Create the key for parent organisation
            var key = $"{parentOrganisationId}-{submissionDeadlineYear}";

            return await _organisationSearchRepository.GetDocumentAsync(key);
        }
        #endregion

        #region SearchOrganisationsAsync
        public async Task<PagedSearchResult<OrganisationSearchModel>> SearchOrganisationsAsync(
            string keywords,
            IEnumerable<byte> turnovers,
            IEnumerable<short> sectors = null,
            IEnumerable<int> deadlineYears = null,
            bool submittedOnly = true,
            bool returnFacets = false,
            bool returnAllFields = false,
            int currentPage = 1,
            int pageSize = 20)
        {

            #region Clean up the search keywords
            keywords = keywords?.Trim();
            keywords = RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords(keywords);

            string RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords(string keywords)
            {
                if (string.IsNullOrEmpty(keywords)) return keywords;

                const string patternToReplace = "(?i)(limited|ltd|llp|uk | uk|\\(uk\\)|-uk|plc)[\\.]*";

                string resultingString;

                resultingString = Regex.Replace(keywords, patternToReplace, string.Empty);
                resultingString = resultingString.Trim();

                var willThisReplacementClearTheString = resultingString == string.Empty;

                return willThisReplacementClearTheString
                    ? keywords // don't replace - user wants to search 'limited' or 'uk'...
                    : resultingString;
            }

            #endregion

            turnovers = turnovers ?? Enumerable.Empty<byte>();
            sectors = sectors ?? Enumerable.Empty<short>();
            deadlineYears = deadlineYears ?? Enumerable.Empty<int>();

            //Specify the fields to return
            string selectFields = returnAllFields ? null : string.Join(',',
                nameof(OrganisationSearchModel.ParentOrganisationId),
                nameof(OrganisationSearchModel.SubmissionDeadlineYear),
                nameof(OrganisationSearchModel.OrganisationName),
                nameof(OrganisationSearchModel.CompanyNumber),
                nameof(OrganisationSearchModel.Address),
                nameof(OrganisationSearchModel.ParentName),
                nameof(OrganisationSearchModel.GroupSubmission));

            #region Build the sort criteria
            var hasFilter = sectors.Any() || turnovers.Any() || deadlineYears.Any();
            var orderBy = (string.IsNullOrWhiteSpace(keywords) && !hasFilter)
                ? $"{nameof(OrganisationSearchModel.Modified)} desc, {nameof(OrganisationSearchModel.OrganisationName)}, {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} desc"
                : $"{nameof(OrganisationSearchModel.OrganisationName)}, {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} desc";
            #endregion

            #region Specify the facet filters
            var facetFields = !returnFacets
                ? null
                : string.Join(',',
                    $"{nameof(OrganisationSearchModel.Turnover)}/{nameof(OrganisationSearchModel.KeyName.Key)}",
                    $"{nameof(OrganisationSearchModel.SectorType)}/{nameof(OrganisationSearchModel.KeyName.Key)}",
                    nameof(OrganisationSearchModel.SubmissionDeadlineYear));

            #endregion

            #region Build the filter constraints
            var queryFilter = new List<string>();

            //Add the turnover filter
            if (turnovers != null && turnovers.Any())
            {
                var turnoverQuery = turnovers.Select(x => $"{nameof(OrganisationSearchModel.Turnover)}/{nameof(OrganisationSearchModel.KeyName.Key)} eq {x}");
                queryFilter.Add($"({string.Join(" or ", turnoverQuery)})");
            }

            //Add the sector filter
            if (sectors != null && sectors.Any())
            {
                var sectorQuery = sectors.Select(x => $"{nameof(OrganisationSearchModel.Sectors)}/{nameof(OrganisationSearchModel.KeyName.Key)} eq {x}");
                queryFilter.Add($"{nameof(OrganisationSearchModel.Sectors)}/any(Sectors: {string.Join(" or ", sectorQuery)})");
            }

            //Add the years filter
            if (deadlineYears != null && deadlineYears.Any())
            {
                var deadlineQuery = deadlineYears.Select(x => $"{nameof(OrganisationSearchModel.SubmissionDeadlineYear)} eq {x}");
                queryFilter.Add($"({string.Join(" or ", deadlineQuery)})");
            }

            //Only show submitted organisations
            if (submittedOnly) queryFilter.Add($"{nameof(OrganisationSearchModel.StatementId)} ne null");

            string filter = string.Join(" and ", queryFilter);
            #endregion

            //Execute the search
            return await _organisationSearchRepository.SearchDocumentsAsync(keywords, currentPage, pageSize, selectFields: selectFields, facetFields: facetFields, orderBy: orderBy, filter: filter);
        }
        #endregion

        #region ListGroupOrganisationsAsync
        public async Task<IEnumerable<OrganisationSearchModel>> ListGroupOrganisationsAsync(long parentOrganisationId, int submissionDeadlineYear)
        {
            //Ensure we have an organisation
            if (parentOrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(parentOrganisationId));

            //Ensure we have an year
            if (submissionDeadlineYear == 0) throw new ArgumentOutOfRangeException(nameof(submissionDeadlineYear));

            //Create the filter for all parent organisations
            var filter = $"{nameof(OrganisationSearchModel.ParentName)} ne null and {nameof(OrganisationSearchModel.ParentOrganisationId)} eq {parentOrganisationId} and {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} eq {submissionDeadlineYear}";

            return await _organisationSearchRepository.ListDocumentsAsync(filter: filter);
        }
        #endregion

    }
}