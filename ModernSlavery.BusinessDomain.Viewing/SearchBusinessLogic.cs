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
        public readonly TestOptions _testOptions;
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
            TestOptions testOptions,
            IDataRepository dataRepository,
            IReportingDeadlineHelper reportingDeadlineHelper,
            ISearchRepository<OrganisationSearchModel> organisationSearchRepository,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog,
            IMapper autoMapper)
        {
            SearchOptions = searchOptions;
            _sharedOptions = sharedOptions;
            _testOptions = testOptions;
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _organisationSearchRepository = organisationSearchRepository;
            SearchLog = searchLog;
            _autoMapper = autoMapper;
        }
        #endregion

        #region RefreshSearchDocuments

        public async Task RefreshSearchDocumentsAsync()
        {
            //Get all the organisations
            var organisations = _dataRepository.GetAll<Organisation>().Where(o=>o.Status==OrganisationStatuses.Active && o.Statements.Any(s=>s.Status== StatementStatuses.Submitted)).ToList();
            await RefreshSearchDocumentsAsync(organisations).ConfigureAwait(false);
        }

        public async Task RefreshSearchDocumentsAsync(IEnumerable<Organisation> organisations)
        {
            //Create the new indexes
            var updatedSearchModels = CreateOrganisationSearchModels(organisations: organisations.ToArray());

            //Get the old indexes
            var existingSearchModels = await _organisationSearchRepository.ListDocumentsAsync(nameof(OrganisationSearchModel.SearchDocumentKey)).ConfigureAwait(false);

            //Batch update the included statements
            if (updatedSearchModels.Any()) await _organisationSearchRepository.AddOrUpdateDocumentsAsync(updatedSearchModels).ConfigureAwait(false);

            //Remove the retired models
            var retiredModels = existingSearchModels.Except(updatedSearchModels);
            await RemoveSearchDocumentsAsync(retiredModels).ConfigureAwait(false);
        }

        public async Task RefreshSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0)
        {
            //Ensure we have an organisation
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the organisations to include or exclude from search
            var updatedSearchModels = CreateOrganisationSearchModels(statementDeadlineYear,organisation);

            //Get the old indexes for statements
            var existingSearchModels = await ListSearchDocumentsAsync(organisation, statementDeadlineYear).ConfigureAwait(false);

            //Batch update the included statements
            if (updatedSearchModels.Any()) await _organisationSearchRepository.AddOrUpdateDocumentsAsync(updatedSearchModels).ConfigureAwait(false);

            //Remove the retired models
            var retiredModels = existingSearchModels.Except(updatedSearchModels);
            await RemoveSearchDocumentsAsync(retiredModels).ConfigureAwait(false);
        }
        #endregion

        #region RemoveSearchDocuments 
        public async Task RemoveSearchDocumentsAsync(Organisation organisation)
        {

            //Get the old indexes for statements
            var retiredModels = await ListSearchDocumentsAsync(organisation).ConfigureAwait(false);

            //Remove the retired models
            await RemoveSearchDocumentsAsync(retiredModels).ConfigureAwait(false);
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

            return await _organisationSearchRepository.ListDocumentsAsync(selectFields: keyOnly ? nameof(OrganisationSearchModel.SearchDocumentKey) : null, filter: filter).ConfigureAwait(false);
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

            return await _organisationSearchRepository.ListDocumentsAsync(filter: filter).ConfigureAwait(false);
        }

        public async Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsByTimestampAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            //Get the old indexes for statements
            string filter = null;
            if (startDate != null || endDate != null)
            {
                if (startDate != null) filter = $"{nameof(OrganisationSearchModel.Timestamp)} ge {startDate.Value.ToUniversalTime():O}";
                if (endDate != null) filter = string.Join(" and ", filter, $"{nameof(OrganisationSearchModel.Timestamp)} lt {endDate.Value.ToUniversalTime():O}");
            }
            return await _organisationSearchRepository.ListDocumentsAsync(filter: filter).ConfigureAwait(false);
        }

        #endregion

        #region Create OrganisationSearchModel
        /// <summary>
        /// Returns a list of fully populated search index documents for specific organisations
        /// </summary>
        /// <param name="organisation">The organisations whos index documents we want to return</param>
        /// <returns>The list of fully populated search index documents</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModels(int submissionDeadlineYear = 0,params Organisation[] organisations)
        {
            //Show include organisations who have submitted a statement
            return organisations.Where(o => o.Status == OrganisationStatuses.Active).SelectMany(o => o.Statements)
                .Where(s => s.Status == StatementStatuses.Submitted && (submissionDeadlineYear == 0 || s.SubmissionDeadline.Year== submissionDeadlineYear))
                .SelectMany(s => CreateOrganisationSearchModels(s));
        }

        /// <summary>
        /// Returns a list of search index documents for the parent organisation and group organisations of a submitted statement
        /// </summary>
        /// <param name="submittedStatement">The submitted statement for a parent organisations</param>
        /// <returns>The list of search index documents for the parent organisation and group organisations</returns>
        private IEnumerable<OrganisationSearchModel> CreateOrganisationSearchModels(Statement submittedStatement)
        {

            if (submittedStatement==null) throw new ArgumentNullException(nameof(submittedStatement));
            if (submittedStatement.Status != StatementStatuses.Submitted) throw new ArgumentException($"Cannot create search model for statement with status={submittedStatement.Status}",nameof(submittedStatement));
            if (submittedStatement.Organisation.Status != OrganisationStatuses.Active) throw new ArgumentException($"Cannot create search model for statement with organisation status={submittedStatement.Organisation.Status}",nameof(submittedStatement));

            var parentStatementModel = CreateParentOrganisationSearchModel(submittedStatement);
            yield return parentStatementModel;

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
                    childStatementModel.SectorType = new OrganisationSearchModel.KeyName { Key = (int)submittedStatement.Organisation.SectorType, Name = submittedStatement.Organisation.SectorType.ToString() };
                }
                childStatementModel.Abbreviations = CreateOrganisationNameAbbreviations(childStatementModel.OrganisationName);
                childStatementModel.PartialNameForCompleteTokenSearches = childStatementModel.OrganisationName;
                childStatementModel.PartialNameForSuffixSearches = childStatementModel.OrganisationName;

                yield return childStatementModel.SetSearchDocumentKey();
            }
        }

        /// <summary>
        /// Returns the search index document for the parent organisation of a submitted statement
        /// </summary>
        /// <param name="submittedStatement">The submitted statement for a parent organisations</param>
        /// <returns>The search index document for the parent organisation</returns>
        private OrganisationSearchModel CreateParentOrganisationSearchModel(Statement submittedStatement)
        {
            if (submittedStatement == null) throw new ArgumentNullException(nameof(submittedStatement));
            if (submittedStatement.Status != StatementStatuses.Submitted) throw new ArgumentException($"Cannot create search model for statement with status={submittedStatement.Status}", nameof(submittedStatement));
            if (submittedStatement.Organisation.Status != OrganisationStatuses.Active) throw new ArgumentException($"Cannot create search model for statement with organisation status={submittedStatement.Organisation.Status}", nameof(submittedStatement));

            var parentStatementModel = new OrganisationSearchModel {
                GroupSubmission = submittedStatement.StatementOrganisations.Any(),
                GroupOrganisationCount = submittedStatement.StatementOrganisations.Count + 1, // add one for parent

                StatementUrl = submittedStatement.StatementUrl,
                StatementEmail = submittedStatement.StatementEmail,
                StatementStartDate = submittedStatement.StatementStartDate,
                StatementEndDate = submittedStatement.StatementEndDate,
                ApprovingPerson = submittedStatement.ApprovingPerson,
                ApprovedDate = submittedStatement.ApprovedDate,

                IncludesStructure = submittedStatement.IncludesStructure,
                StructureDetails = submittedStatement.StructureDetails,
                IncludesPolicies = submittedStatement.IncludesPolicies,
                PolicyDetails = submittedStatement.PolicyDetails,
                IncludesRisks = submittedStatement.IncludesRisks,
                RisksDetails = submittedStatement.RisksDetails,
                IncludesDueDiligence = submittedStatement.IncludesDueDiligence,
                DueDiligenceDetails = submittedStatement.DueDiligenceDetails,
                IncludesTraining = submittedStatement.IncludesTraining,
                TrainingDetails = submittedStatement.TrainingDetails,
                IncludesGoals = submittedStatement.IncludesGoals,
                GoalsDetails = submittedStatement.GoalsDetails,

                Sectors = submittedStatement.Sectors.Select(s => new OrganisationSearchModel.KeyName { Key = s.StatementSectorTypeId, Name = s.StatementSectorType.Description }).ToList(),
                OtherSectors = submittedStatement?.OtherSectors,

                Turnover = _autoMapper.Map<OrganisationSearchModel.KeyName>(submittedStatement.Turnover),
                StatementYears = _autoMapper.Map<OrganisationSearchModel.KeyName>(submittedStatement.StatementYears),

                Summary = _autoMapper.Map<OrganisationSearchModel.SummarySearchModel>(submittedStatement.Summary),

                StatementId = submittedStatement.StatementId,
                ParentOrganisationId = submittedStatement.Organisation.OrganisationId,
                SubmissionDeadlineYear = submittedStatement.SubmissionDeadline.Year,
                OrganisationName = submittedStatement.Organisation.OrganisationName,
                CompanyNumber = submittedStatement.Organisation.CompanyNumber,
                SectorType = _autoMapper.Map<OrganisationSearchModel.KeyName>(submittedStatement.Organisation.SectorType),
                Address = AddressModel.Create(submittedStatement.Organisation.LatestAddress),

                Modified = submittedStatement.Modified,
                Abbreviations = CreateOrganisationNameAbbreviations(submittedStatement.Organisation.OrganisationName),
                PartialNameForCompleteTokenSearches = submittedStatement.Organisation.OrganisationName,
                PartialNameForSuffixSearches = submittedStatement.Organisation.OrganisationName
            };

            return parentStatementModel.SetSearchDocumentKey();
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


        #region SearchOrganisationsAsync
        public async Task<PagedSearchResult<OrganisationSearchModel>> SearchOrganisationsAsync(
            string keywords,
            IEnumerable<byte> turnovers,
            IEnumerable<short> sectors = null,
            IEnumerable<int> deadlineYears = null,
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

            string filter = string.Join(" and ", queryFilter);
            #endregion

            //Execute the search
            return await _organisationSearchRepository.SearchDocumentsAsync(keywords, currentPage, pageSize, selectFields: selectFields, facetFields: facetFields, orderBy: orderBy, filter: filter).ConfigureAwait(false);
        }
        #endregion

        #region GetOrganisationAsync
        public async Task<OrganisationSearchModel> GetOrganisationAsync(long parentOrganisationId, int submissionDeadlineYear)
        {
            //Ensure we have an organisation
            if (parentOrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(parentOrganisationId));

            //Ensure we have an year
            if (submissionDeadlineYear == 0) throw new ArgumentOutOfRangeException(nameof(submissionDeadlineYear));

            //Check there is a statement in the database
            var statement = _dataRepository.GetAll<Statement>().FirstOrDefault(s => s.Status == StatementStatuses.Submitted && s.OrganisationId == parentOrganisationId && s.SubmissionDeadline.Year == submissionDeadlineYear);
            if (statement == null) return default;

            //Create the key for parent organisation
            var key = $"{parentOrganisationId}-{submissionDeadlineYear}";

            var statementSummary = await _organisationSearchRepository.GetDocumentAsync(key).ConfigureAwait(false);

            //If the document isnt yet on the searh index then create from database
            if (statementSummary == null) statementSummary = CreateParentOrganisationSearchModel(statement);

            return statementSummary;

        }
        #endregion

        #region ListGroupOrganisationsAsync
        public async Task<IEnumerable<OrganisationSearchModel>> ListGroupOrganisationsAsync(long parentOrganisationId, int submissionDeadlineYear)
        {
            //Ensure we have an organisation
            if (parentOrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(parentOrganisationId));

            //Ensure we have an year
            if (submissionDeadlineYear == 0) throw new ArgumentOutOfRangeException(nameof(submissionDeadlineYear));

            //Check there is a statement in the database
            var statement = _dataRepository.GetAll<Statement>().FirstOrDefault(s => s.Status == StatementStatuses.Submitted && s.OrganisationId == parentOrganisationId && s.SubmissionDeadline.Year == submissionDeadlineYear);
            if (statement == null || !statement.StatementOrganisations.Any()) return default;

            //Create the filter for all parent organisations
            var filter = $"{nameof(OrganisationSearchModel.ParentName)} ne null and {nameof(OrganisationSearchModel.ParentOrganisationId)} eq {parentOrganisationId} and {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} eq {submissionDeadlineYear}";

            var groupSummaries = await _organisationSearchRepository.ListDocumentsAsync(filter: filter).ConfigureAwait(false);

            //If the document isnt yet on the searh index then create from database
            if (!groupSummaries.Any()) groupSummaries = CreateOrganisationSearchModels(statement).Skip(1).ToList();//Skip the first parent organisation

            return groupSummaries;
        }
        #endregion

    }
}