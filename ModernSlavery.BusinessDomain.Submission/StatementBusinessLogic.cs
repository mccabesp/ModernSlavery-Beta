using AutoMapper;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    partial class StatementBusinessLogic : IStatementBusinessLogic
    {
        private readonly ILogger<StatementBusinessLogic> _logger;
        private readonly SubmissionOptions _submissionOptions;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly IMapper _mapper;

        public StatementBusinessLogic(ILogger<StatementBusinessLogic> logger, SubmissionOptions submissionOptions, ISharedBusinessLogic sharedBusinessLogic, IOrganisationBusinessLogic organisationBusinessLogic, ISearchBusinessLogic searchBusinessLogic, IMapper mapper)
        {
            _logger = logger;
            _submissionOptions = submissionOptions;
            _sharedBusinessLogic = sharedBusinessLogic;
            _organisationBusinessLogic = organisationBusinessLogic;
            _searchBusinessLogic = searchBusinessLogic;
            _mapper = mapper;
        }

        #region Private Methods
        /// <summary>
        /// Returns the full file path and filename to use for draft files
        /// </summary>
        /// <param name="organisationId">The id of the organisation who owns the draft data </param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the draft data relates</param>
        /// <returns>The full filepath of the draft data file</returns>
        private string GetDraftFilepath(long organisationId, int reportingDeadlineYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingDeadlineYear}.json");

        /// <summary>
        /// Returns the full file path and filename to use for backup of a draft file
        /// </summary>
        /// <param name="organisationId">The id of the organisation who owns the draft data </param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the draft data relates</param>
        /// <returns>The full filepath of the draft data backup file</returns>
        private string GetDraftBackupFilepath(long organisationId, int reportingDeadlineYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingDeadlineYear}.bak");

        /// <summary>
        /// Returns any existing DraftStatementModel for this organisation and reporting deadline or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The StatementModel or null if file not found</returns>
        private async Task<StatementModel> FindDraftStatementModelAsync(long organisationId, DateTime reportingDeadline)
        {
            //Get the draft filepath for this organisation and reporting deadline
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadline.Year);

            //Return the draft StatementModel deserialized from file
            var statementModel = await LoadStatementModelFromFile(draftFilePath);

            //Check the contents are correct
            if (statementModel != null)
            {
                if (statementModel.SubmissionDeadline != reportingDeadline) throw new Exception($"Draft SubmissionDeadline: {statementModel.SubmissionDeadline} does not match parameter reportingDeadline: {reportingDeadline}");
                if (statementModel.OrganisationId != organisationId) throw new Exception($"Draft OrganisationId: {statementModel.OrganisationId} does not match parameter OrganisationId: {organisationId}");
            }
            //TODO: Check all TypeIds are still valid in case some new ones or old retired
            return statementModel;
        }

        /// <summary>
        /// Returns any existing backup DraftStatementModel for this organisation and reporting deadline or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The StatementModel or null if file not found</returns>
        private async Task<StatementModel> FindDraftBackupStatementModelAsync(long organisationId, DateTime reportingDeadline)
        {
            //Get the draft filepath for this organisation and reporting deadline
            var backupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadline.Year);

            //Return the draft StatementModel deserialized from file
            var statementModel = await LoadStatementModelFromFile(backupFilePath);

            //TODO: Check all TypeIds are still valid in case some new ones or old retired
            return statementModel;
        }

        public async Task<Statement> FindLatestSubmittedStatementAsync(long organisationId, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));

            //Try and get the organisation
            var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            return await FindLatestSubmittedStatementAsync(organisation, reportingDeadline);
        }

        public async Task<Statement> FindLatestSubmittedStatementAsync(Organisation organisation, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            var statement = organisation.Statements.FirstOrDefault(s => s.SubmissionDeadline == reportingDeadline && s.Status == StatementStatuses.Submitted);
            return statement;
        }

        /// <summary>
        /// Returns any existing Submitted Statement Entity for this organisation and reporting deadline or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline to which the statement data relates</param>
        /// <returns>The Statement entity or null if not found</returns>
        private async Task<Statement> FindSubmittedStatementAsync(long organisationId, DateTime reportingDeadline)
        {
            return await _organisationBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline == reportingDeadline && s.Status == StatementStatuses.Submitted);
        }

        /// <summary>
        /// Returns any existing Submitted Statement Entity for this organisation and reporting deadline or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The Statement entity or null if not found</returns>
        private async Task<Statement> FindSubmittedStatementAsync(long organisationId, int reportingDeadlineYear)
        {
            return await _organisationBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline.Year == reportingDeadlineYear && s.Status == StatementStatuses.Submitted);
        }

        /// <summary>
        /// Deletes any existing Draft Statement model files (and backups) for this organisation and reporting deadline
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>Nothing</returns>
        private async Task DeleteDraftStatementModelAsync(long organisationId, DateTime reportingDeadline)
        {
            //Delete the original
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadline.Year);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);

            //Delete the backup
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadline.Year);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
        }

        private void MapFromModel(StatementModel statementModel, Statement statement)
        {
            _mapper.Map(statementModel, statement);

            //Map the group organisations
            statementModel.StatementOrganisations.ForEach(groupOrganisation =>
            {
                var statementOrganisation = new StatementOrganisation()
                {
                    Statement = statement,
                    OrganisationName = groupOrganisation.OrganisationName,
                    OrganisationId = groupOrganisation.OrganisationId,
                    Included = groupOrganisation.Included
                };
                statement.StatementOrganisations.Add(statementOrganisation);
            });

            //Map the sectors
            statement.Sectors.Where(s => !statementModel.Sectors.Contains(s.StatementSectorTypeId)).ForEach(s => { statement.Sectors.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.Sectors.ForEach(id =>
            {
                var sector = statement.Sectors.FirstOrDefault(s => s.StatementSectorTypeId == id);
                if (sector == null)
                {
                    sector = new StatementSector()
                    {
                        StatementSectorTypeId = id,
                        StatementSectorType = _organisationBusinessLogic.DataRepository.Get<StatementSectorType>(id),
                        StatementId = statement.StatementId
                    };
                    statement.Sectors.Add(sector);
                }
            });

            //Map the Policies
            statement.Policies.Where(s => !statementModel.Policies.Contains(s.StatementPolicyTypeId)).ForEach(s => { statement.Policies.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.Policies.ForEach(id =>
            {
                var policy = statement.Policies.FirstOrDefault(s => s.StatementPolicyTypeId == id);
                if (policy == null)
                {
                    policy = new StatementPolicy()
                    {
                        StatementPolicyTypeId = id,
                        StatementPolicyType = _organisationBusinessLogic.DataRepository.Get<StatementPolicyType>(id),
                        StatementId = statement.StatementId
                    };

                    statement.Policies.Add(policy);
                }
            });

            //Map the Relevant Risks
            statement.RelevantRisks.Where(s => !statementModel.RelevantRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.RelevantRisks.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.RelevantRisks.ForEach(model =>
            {
                var relevantRisk = statement.RelevantRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (relevantRisk == null)
                {
                    relevantRisk = new StatementRelevantRisk()
                    {
                        StatementRiskTypeId = model.Id,
                        StatementRiskType = _organisationBusinessLogic.DataRepository.Get<StatementRiskType>(model.Id),
                        Details = model.Details,
                        StatementId = statement.StatementId
                    };

                    statement.RelevantRisks.Add(relevantRisk);
                }
                else
                    relevantRisk.Details = model.Details;
            });

            //Map the High Risks
            statement.HighRisks.Where(s => !statementModel.HighRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.HighRisks.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.HighRisks.ForEach(model =>
            {
                var highRisk = statement.HighRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (highRisk == null)
                {
                    highRisk = new StatementHighRisk()
                    {
                        StatementRiskTypeId = model.Id,
                        StatementRiskType = _organisationBusinessLogic.DataRepository.Get<StatementRiskType>(model.Id),
                        Details = model.Details,
                        StatementId = statement.StatementId
                    };

                    statement.HighRisks.Add(highRisk);
                }
                else
                    highRisk.Details = model.Details;
            });

            //Map the Location Risks
            statement.LocationRisks.Where(s => !statementModel.LocationRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.LocationRisks.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.LocationRisks.ForEach(model =>
            {
                var locationRisk = statement.LocationRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (locationRisk == null)
                {
                    locationRisk = new StatementLocationRisk()
                    {
                        StatementRiskTypeId = model.Id,
                        StatementRiskType = _organisationBusinessLogic.DataRepository.Get<StatementRiskType>(model.Id),
                        Details = model.Details,
                        StatementId = statement.StatementId
                    };

                    statement.LocationRisks.Add(locationRisk);
                }
                else
                    locationRisk.Details = model.Details;
            });

            //Map the Due Diligences
            statement.Diligences.Where(s => !statementModel.DueDiligences.Any(model => model.Id == s.StatementDiligenceTypeId)).ForEach(s => { statement.Diligences.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.DueDiligences.ForEach(model =>
            {
                var diligence = statement.Diligences.FirstOrDefault(s => s.StatementDiligenceTypeId == model.Id);
                if (diligence == null)
                {
                    diligence = new StatementDiligence()
                    {
                        StatementDiligenceTypeId = model.Id,
                        StatementDiligenceType = _organisationBusinessLogic.DataRepository.Get<StatementDiligenceType>(model.Id),
                        Details = model.Details,
                        StatementId = statement.StatementId
                    };

                    statement.Diligences.Add(diligence);
                }
                else
                    diligence.Details = model.Details;
            });

            //Map the Training
            statement.Training.Where(s => !statementModel.Training.Contains(s.StatementTrainingTypeId)).ForEach(s => { statement.Training.Remove(s); _organisationBusinessLogic.DataRepository.Delete(s); });
            statementModel.Training.ForEach(id =>
            {
                var training = statement.Training.FirstOrDefault(s => s.StatementTrainingTypeId == id);
                if (training == null)
                {
                    training = new StatementTraining()
                    {
                        StatementTrainingTypeId = id,
                        StatementTrainingType = _organisationBusinessLogic.DataRepository.Get<StatementTrainingType>(id),
                        StatementId = statement.StatementId
                    };

                    statement.Training.Add(training);
                }
            });
        }

        private async Task<Outcome<StatementErrors, (Organisation Organisation, DateTime ReportingDeadline)>> GetOrganisationAndDeadlineAsync(long organisationId, int reportingDeadlineYear)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));

            //Try and get the organisation
            var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) return new Outcome<StatementErrors, (Organisation, DateTime)>(StatementErrors.NotFound);

            //Get the reporting deadline
            var reportingDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(organisation.SectorType, reportingDeadlineYear);

            return new Outcome<StatementErrors, (Organisation, DateTime)>((organisation, reportingDeadline));
        }

        private void CheckReportingDeadline(Organisation organisation, DateTime reportingDeadline)
        {
            var firstDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(organisation.SectorType, _sharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear);
            var currentDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(organisation.SectorType);

            if (reportingDeadline < firstDeadline || reportingDeadline > currentDeadline.AddDays(1)) throw new ArgumentOutOfRangeException(nameof(reportingDeadline));
        }

        /// <summary>
        /// Deserialises a json file to a StatementModel 
        /// </summary>
        /// <param name="draftFilePath">The target filepath to loafd the json from</param>
        /// <returns>The deserialised statementmodel from the file</returns>
        private async Task<StatementModel> LoadStatementModelFromFile(string draftFilePath)
        {
            //Return null if no draft file exists
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath)) return null;

            //Return the draft StatementModel deserialized from file
            var draftJson = await _sharedBusinessLogic.FileRepository.ReadAsync(draftFilePath);
            var statementModel = JsonConvert.DeserializeObject<StatementModel>(draftJson);
            return statementModel;
        }

        /// <summary>
        /// Serialises a StatementModel to a json file
        /// </summary>
        /// <param name="statementModel">The source statementmodel to serialise</param>
        /// <param name="draftFilePath">The target filepath to save the json to</param>
        /// <returns></returns>
        private async Task SaveStatementModelToFileAsync(StatementModel statementModel, string draftFilePath)
        {
            //Save the new draft data 
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            var draftJson = JsonConvert.SerializeObject(statementModel, _sharedBusinessLogic.SharedOptions.IsProduction() ? Formatting.None : Formatting.Indented, jsonSettings);
            await _sharedBusinessLogic.FileRepository.WriteAsync(draftFilePath, Encoding.UTF8.GetBytes(draftJson));
        }

        /// <summary>
        /// Returns a differences between two StatementModels in json format
        /// </summary>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <returns></returns>
        private IList<AutoMap.Diff> GetModifications(StatementModel oldModel, StatementModel newModel)
        {
            if (oldModel == null) throw new ArgumentNullException(nameof(oldModel));
            if (newModel == null) throw new ArgumentNullException(nameof(newModel));

            //Compare the two statementModels
            var membersToIgnore = new[] { nameof(StatementModel.ReturnToReviewPage), nameof(StatementModel.DraftBackupDate), nameof(StatementModel.EditorUserId), nameof(StatementModel.EditTimestamp), nameof(StatementModel.StatementId), nameof(StatementModel.OrganisationName), nameof(StatementModel.Status), nameof(StatementModel.StatusDate), nameof(StatementModel.SubmissionDeadline), nameof(StatementModel.OrganisationId), nameof(StatementModel.ReturnToReviewPage), nameof(StatementModel.Modifications), nameof(StatementModel.EHRCResponse), nameof(StatementModel.LateReason), nameof(StatementModel.IncludedOrganisationCount), nameof(StatementModel.ExcludedOrganisationCount), nameof(StatementModel.Modified), nameof(StatementModel.Created) };
            var differences = oldModel.GetDifferences(newModel, membersToIgnore).ToList();
            return differences;
        }

        /// <summary>
        /// Returns a new statement instance.
        /// </summary>
        /// <param name="organisation">The organisation that the statement is for.</param>
        /// <param name="submissionDeadline">The submission deadline of the statement.</param>
        /// <returns></returns>
        private Statement GetEmptyStatement(Organisation organisation, DateTime submissionDeadline)
        {
            // These fields need to be set so that when mapped to the
            // statementmodel appropriate fields are populated
            return new Statement
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                SubmissionDeadline = submissionDeadline
            };
        }

        private async Task<IEnumerable<Statement>> FindGroupSubmissionStatementsAsync(long organisationId, int? reportingDeadlineYear = null)
        {
            var statementOrganisations = await _organisationBusinessLogic.DataRepository.ToListAsync<StatementOrganisation>
                (s => s.OrganisationId == organisationId);

            if (reportingDeadlineYear != null)
                statementOrganisations = statementOrganisations.Where(x => x.Statement.SubmissionDeadline.Year == reportingDeadlineYear).ToList();

            return statementOrganisations.Select(x => x.Statement).GroupBy(g => g.OrganisationId, (key, g) => g.OrderByDescending(e => e.Modified).First());
        }

        private IEnumerable<string> GetGroupSubmissionInformationString(IEnumerable<Statement> statements)
        {
            var result = new List<string>();
            foreach (var statement in statements)
            {
                result.Add($"{statement.Organisation.OrganisationName}'s {statement.SubmissionDeadline.Year - 1} to {statement.SubmissionDeadline.Year} group submission, published on {statement.Modified:d MMM yyyy}");

            }
            return result;
        }

        #endregion

        #region Public Methods

        public async Task<StatementInfoModel> GetStatementInfoModelAsync(long organisationId, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));

            //Try and get the organisation
            var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);

            return await GetStatementInfoModelAsync(organisation, reportingDeadline);
        }

        public async Task<StatementInfoModel> GetStatementInfoModelAsync(Organisation organisation, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            //Get the organisation info
            var statementInfoModel = new StatementInfoModel
            {
                ReportingDeadline = reportingDeadline,
                ScopeStatus = organisation.GetActiveScopeStatus(reportingDeadline)
            };

            //Get the submitted statement info
            var statement = organisation.Statements.FirstOrDefault(s => s.SubmissionDeadline == reportingDeadline);
            if (statement != null)
                statementInfoModel.SubmittedStatementModifiedDate = statement?.Modified;

            //Get the draft statement info
            var statementModel = await FindDraftStatementModelAsync(organisation.OrganisationId, reportingDeadline);
            if (statementModel != null)
            {
                statementInfoModel.DraftStatementModifiedDate = statementModel.EditTimestamp;
                statementInfoModel.DraftStatementIsEmpty = statementModel.IsEmpty();
            }

            //Get group status info
            var groupStatements = await FindGroupSubmissionStatementsAsync(organisation.OrganisationId, reportingDeadline.Year);
            if (groupStatements.Any()) statementInfoModel.GroupSubmissionInfo = GetGroupSubmissionInformationString(groupStatements).ToList();

            return statementInfoModel;
        }

        public async IAsyncEnumerable<StatementInfoModel> GetStatementInfoModelsAsync(Organisation organisation)
        {
            var earlistReportingDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetFirstReportingDeadline(organisation.SectorType);
            var latestReportingDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(organisation.SectorType);

            var current = latestReportingDeadline;

            while (current >= earlistReportingDeadline)
            {
                yield return await GetStatementInfoModelAsync(organisation, current);

                current = current.AddYears(-1);
            }
        }

        public async Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModelAsync(long organisationId, int reportingDeadlineYear)
        {
            //Get the organisation and reporting deadline
            var outcome = await GetOrganisationAndDeadlineAsync(organisationId, reportingDeadlineYear);
            if (outcome.Fail) new Outcome<StatementErrors, StatementModel>(outcome.Errors);

            //Get the latest statement
            return await GetLatestSubmittedStatementModelAsync(outcome.Result.Organisation, outcome.Result.ReportingDeadline);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModelAsync(Organisation organisation, DateTime reportingDeadline)
        {
            var statement = await FindLatestSubmittedStatementAsync(organisation, reportingDeadline);
            if (statement == null) return new Outcome<StatementErrors, StatementModel>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisation.OrganisationId} due for reporting deadline {reportingDeadline}");

            //Copy the statement properties to the model
            var statementModel = _mapper.Map<StatementModel>(statement);

            //Return the successful model
            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<StatementModel> GetDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Try and get the organisation
            var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId)) throw new SecurityException($"User {userId} does not have permission to view statement for organisation {organisationId}");

            //Try and get the draft first
            var statementModel = await FindDraftStatementModelAsync(organisationId, reportingDeadline);

            return statementModel;
        }

        public async Task<StatementModel> GetDraftBackupStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Try and get the organisation
            var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId)) throw new SecurityException($"User {userId} does not have permission to view statement for organisation {organisationId}");

            //Try and get the draft first
            var backupStatementModel = await FindDraftBackupStatementModelAsync(organisationId, reportingDeadline);

            return backupStatementModel;
        }

        public bool ReportingDeadlineHasExpired(DateTime reportingDeadline)
        {
            if (_submissionOptions.DeadlineExtensionMonths == -1 || _submissionOptions.DeadlineExtensionMonths == -1)
                return false;

            return reportingDeadline.Date.AddMonths(_submissionOptions.DeadlineExtensionMonths).AddDays(_submissionOptions.DeadlineExtensionDays) < VirtualDateTime.Now.Date;
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(long organisationId, int reportingDeadlineYear, long userId)
        {
            //Get the organisation and reporting deadline
            var outcome = await GetOrganisationAndDeadlineAsync(organisationId, reportingDeadlineYear);
            if (outcome.Fail) new Outcome<StatementErrors, StatementModel>(outcome.Errors);

            //Get the open draft
            return await OpenDraftStatementModelAsync(outcome.Result.Organisation, outcome.Result.ReportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(Organisation organisation, DateTime reportingDeadline, long userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            //Check if it is too late to edit
            if (ReportingDeadlineHasExpired(reportingDeadline))
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.TooLate);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId))
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.Unauthorised);

            //Try and get the draft first
            var draftStatement = await FindDraftStatementModelAsync(organisation.OrganisationId, reportingDeadline);

            //Get the statement to map from
            var submittedStatement = await FindSubmittedStatementAsync(organisation.OrganisationId, reportingDeadline);

            if (draftStatement != null)
            {
                //Check if the existing draft lock has expired
                var draftExpired = draftStatement.EditTimestamp.AddMinutes(_submissionOptions.DraftTimeoutMinutes) < VirtualDateTime.Now;

                //Check the existing draft is not still locked by another user
                if (draftStatement.EditorUserId > 0 && draftStatement.EditorUserId != userId && !draftExpired)
                    return new Outcome<StatementErrors, StatementModel>(StatementErrors.Locked);
            }
            else
            {
                if (submittedStatement != null)
                    //Create a draft from previously submitted data
                    draftStatement = _mapper.Map<StatementModel>(submittedStatement);
                else
                {
                    //Create an empty draft statement
                    draftStatement = _mapper.Map<StatementModel>(GetEmptyStatement(organisation, reportingDeadline));
                    //Create a backup for non-submitted drafts
                }
            }

            //Set the key priorities
            draftStatement.EditorUserId = userId;
            draftStatement.EditTimestamp = VirtualDateTime.Now;

            // opening statements with old data should be overwritten with the newest state
            draftStatement.OrganisationName = organisation.OrganisationName;
            draftStatement.SubmissionDeadline = reportingDeadline;
            draftStatement.Submitted = organisation.Statements.Any(s => s.Status == StatementStatuses.Submitted && s.SubmissionDeadline == reportingDeadline);

            //Save new statement model with timestamp to lock to new user
            await SaveDraftStatementModelAsync(draftStatement, true);
            draftStatement.DraftBackupDate = null;

            //Get the draft backup date
            var draftBackupFilePath = GetDraftBackupFilepath(draftStatement.OrganisationId, draftStatement.SubmissionDeadline.Year);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                draftStatement.DraftBackupDate = await _sharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(draftBackupFilePath);

            return new Outcome<StatementErrors, StatementModel>(draftStatement);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(long organisationId, int reportingDeadlineYear, long userId)
        {
            //Get the organisation and reporting deadline
            var outcome = await GetOrganisationAndDeadlineAsync(organisationId, reportingDeadlineYear);
            if (outcome.Fail) new Outcome<StatementErrors>(outcome.Errors);

            //Cancel the draft
            return await CancelDraftStatementModelAsync(outcome.Result.Organisation, outcome.Result.ReportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(Organisation organisation, DateTime reportingDeadline, long userId)
        {
            //Validate the parameters
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModelAsync(organisation, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(organisation.OrganisationId, reportingDeadline.Year);

            //Get the backup draft filepath
            var draftBackupFilePath = GetDraftBackupFilepath(organisation.OrganisationId, reportingDeadline.Year);

            //Restore draft from backup
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
            {
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftBackupFilePath, draftFilePath, true);
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
            }

            //Remove the draft if its empty or submitted
            await DeleteIfEmptyOrSubmittedAsync(draftFilePath);

            return new Outcome<StatementErrors>();
        }

        /// <summary>
        /// Deletes a draft file if it is empty or same as last submitted
        /// </summary>
        /// <param name="draftFilePath"></param>
        /// <returns>True if the file remains</returns>
        private async Task<bool> DeleteIfEmptyOrSubmittedAsync(string draftFilePath, StatementModel statementModel = null)
        {
            if (statementModel == null) statementModel = await LoadStatementModelFromFile(draftFilePath);
            if (statementModel == null) return false;

            var delete = statementModel.IsEmpty();
            if (!delete && statementModel.Submitted)
            {
                statementModel.Modifications = await CompareToSubmittedStatement(statementModel);
                delete = !statementModel.Modifications.Any();
            }
            //Delete the draft
            if (delete) await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);

            return !delete;
        }

        public async Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(long organisationId, int reportingDeadlineYear, long userId)
        {
            var outcome = await GetOrganisationAndDeadlineAsync(organisationId, reportingDeadlineYear);
            if (outcome.Fail) new Outcome<StatementErrors>(outcome.Errors);

            //Close the draft
            return await CloseDraftStatementModelAsync(outcome.Result.Organisation, outcome.Result.ReportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(Organisation organisation, DateTime reportingDeadline, long userId)
        {
            //Validate the parameters
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Get the draft
            var openOutcome = await OpenDraftStatementModelAsync(organisation, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);
            var statementModel = openOutcome.Result;

            //Remove the draft if its empty or submitted
            var draftFilePath = GetDraftFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
            var draftExists = await DeleteIfEmptyOrSubmittedAsync(draftFilePath, statementModel);

            //Save the draft with no userId
            if (draftExists)
            {
                statementModel.EditorUserId = 0;
                await SaveStatementModelToFileAsync(statementModel, draftFilePath);
            }

            //Delete the backup draft
            var draftBackupFilePath = GetDraftBackupFilepath(statementModel.OrganisationId, reportingDeadline.Year);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);

            return new Outcome<StatementErrors>();
        }

        public async Task SaveDraftStatementModelAsync(StatementModel statementModel, bool createBackup = false)
        {
            //Validate the parameters
            if (statementModel == null) throw new ArgumentNullException(nameof(statementModel));
            if (statementModel.OrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.OrganisationId));
            if (statementModel.EditorUserId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.EditorUserId));

            //Try and get the organisation
            var organisation = _organisationBusinessLogic.DataRepository.Get<Organisation>(statementModel.OrganisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(statementModel.OrganisationId));

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, statementModel.SubmissionDeadline);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(statementModel.EditorUserId))
                throw new SecurityException($"User {statementModel.EditorUserId} does not have permission to submit for organisation {statementModel.OrganisationId}");

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);

            //Create a backup if required
            if (createBackup)
            {
                var draftBackupFilePath = GetDraftBackupFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
                if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                {
                    if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                        await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftFilePath, draftBackupFilePath, false);
                    else
                        await SaveStatementModelToFileAsync(statementModel, draftBackupFilePath);
                }
            }
            await SaveStatementModelToFileAsync(statementModel, draftFilePath);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(long organisationId, int reportingDeadlineYear, long userId = -1)
        {
            var outcome = await GetOrganisationAndDeadlineAsync(organisationId, reportingDeadlineYear);
            if (outcome.Fail) new Outcome<StatementErrors>(outcome.Errors);

            //Submit the draft
            return await SubmitDraftStatementModelAsync(outcome.Result.Organisation, outcome.Result.ReportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(Organisation organisation, DateTime reportingDeadline, long userId = -1)
        {
            //Validate the parameters
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModelAsync(organisation, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            var previousStatement = await FindSubmittedStatementAsync(organisation.OrganisationId, reportingDeadline);
            previousStatement?.SetStatus(StatementStatuses.Retired, userId);

            var newStatement = new Statement { Organisation = organisation };
            newStatement.SetStatus(StatementStatuses.Submitted, userId);

            await _organisationBusinessLogic.DataRepository.BeginTransactionAsync(
                async () =>
                {
                    try
                    {
                        //Save new group organisations from companies house
                        var newOrganisations = statementModel.StatementOrganisations.Where(o => o.OrganisationId == null && !string.IsNullOrWhiteSpace(o.CompanyNumber));
                        foreach (var groupOrganisation in newOrganisations)
                        {
                            var organisation = _organisationBusinessLogic.DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.CompanyNumber == groupOrganisation.CompanyNumber);
                            if (organisation == null)
                            {
                                //Save the new organisation from companies house
                                organisation = _organisationBusinessLogic.CreateOrganisation(groupOrganisation.OrganisationName, "CoHo", SectorTypes.Private, OrganisationStatuses.Active, groupOrganisation.Address, AddressStatuses.Active, groupOrganisation.CompanyNumber, groupOrganisation.DateOfCessation, userId: userId);
                                await _organisationBusinessLogic.SaveOrganisationAsync(organisation);
                            }
                            else if (organisation.OrganisationId == groupOrganisation.OrganisationId)
                                throw new Exception("Attempt to add the a child group organisation to the same parent organisation");

                            groupOrganisation.OrganisationId = organisation.OrganisationId;
                        }

                        //Copy all the other model propertiesto the entity
                        MapFromModel(statementModel, newStatement);
                        newStatement.Modifications = null;

                        if (previousStatement != null)
                        {
                            //Create the previous StatementModel
                            var previousStatementModel = new StatementModel();
                            _mapper.Map(previousStatement, previousStatementModel);

                            //Compare the latest with the previous statementModel
                            var modifications = GetModifications(previousStatementModel, statementModel);
                            newStatement.Modifications = modifications.Any() ? JsonConvert.SerializeObject(modifications, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore }) : null;
                        }

                        //Set whether thestatement is late
                        newStatement.IsLateSubmission = newStatement.GetIsLateSubmission();

                        //Save the new statement to the database
                        _organisationBusinessLogic.DataRepository.Insert(newStatement);
                        await _organisationBusinessLogic.DataRepository.SaveChangesAsync();

                        //Delete the draft file and its backup
                        await DeleteDraftStatementModelAsync(organisation.OrganisationId, reportingDeadline);

                        _organisationBusinessLogic.DataRepository.CommitTransaction();

                    }
                    catch (Exception ex)
                    {
                        _organisationBusinessLogic.DataRepository.RollbackTransaction();
                        _logger.LogError(ex, JsonConvert.SerializeObject(statementModel));
                        throw;
                    }
                });

            //Update the search indexes
            await _searchBusinessLogic.RefreshSearchDocumentsAsync(newStatement.Organisation, newStatement.SubmissionDeadline.Year);

            return new Outcome<StatementErrors>();
        }

        public async Task<IList<AutoMap.Diff>> CompareToDraftBackupStatement(StatementModel newStatementModel)
        {
            //Try and find a back draft first
            var oldStatementModel = await FindDraftBackupStatementModelAsync(newStatementModel.OrganisationId, newStatementModel.SubmissionDeadline);

            //If no backup then use an empty statement
            if (oldStatementModel == null)
            {
                var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(newStatementModel.OrganisationId);
                var statement = GetEmptyStatement(organisation, newStatementModel.SubmissionDeadline);

                //Copy the statement properties to the model
                oldStatementModel = _mapper.Map<StatementModel>(statement);
            }

            //Return the modifications
            return GetModifications(oldStatementModel, newStatementModel);
        }

        public async Task<IList<AutoMap.Diff>> CompareToSubmittedStatement(StatementModel newStatementModel)
        {
            //Try and find a submitted statement first
            var statement = await FindLatestSubmittedStatementAsync(newStatementModel.OrganisationId, newStatementModel.SubmissionDeadline);

            //If no statement then use an empty statement
            if (statement == null)
            {
                var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(newStatementModel.OrganisationId); ;
                statement = GetEmptyStatement(organisation, newStatementModel.SubmissionDeadline);
            }

            var oldStatementModel = new StatementModel();

            //Copy the statement properties to the model
            _mapper.Map(statement, oldStatementModel);

            //Return the modifications
            return GetModifications(oldStatementModel, newStatementModel);
        }



        public async Task<List<string>> GetExistingStatementInformationAsync(long organisationId, int reportingDeadlineYear)
        {
            var results = new List<string>();
            if (organisationId > 0)
            {
                var submission = await FindSubmittedStatementAsync(organisationId, reportingDeadlineYear);
                if (submission != null)
                    results.Add($"{reportingDeadlineYear - 1} to {reportingDeadlineYear} statement for\n{submission.Organisation.OrganisationName}\n" +
                        $"(submitted to our service on {submission.Modified:d MMM yyyy})");
                else
                {
                    //See if a draft exists
                    var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);
                    var draftExists = await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath);

                    //Try and get the organisation
                    if (draftExists)
                    {
                        var organisation = await _organisationBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
                        if (organisation != null)
                            results.Add($"{reportingDeadlineYear - 1} to {reportingDeadlineYear} statement for\n{organisation.OrganisationName}\n" +
                                $"(draft submission in progress on our service)");
                    }
                }

                var groupSubmissions = await FindGroupSubmissionStatementsAsync(organisationId, reportingDeadlineYear);
                foreach (var groupSubmission in groupSubmissions)
                    results.Add($"{reportingDeadlineYear - 1} to {reportingDeadlineYear} statement for\n{groupSubmission.Organisation.OrganisationName}\n" +
                        $"(submitted to our service on {groupSubmission.Modified:d MMM yyyy})");
            }
            return results;
        }
        #endregion
    }
}
