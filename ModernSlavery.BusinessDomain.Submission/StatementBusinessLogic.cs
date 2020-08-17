using AutoMapper;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
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
        private readonly SubmissionOptions _submissionOptions;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IMapper _mapper;

        public StatementBusinessLogic(SubmissionOptions submissionOptions, ISharedBusinessLogic sharedBusinessLogic, IMapper mapper)
        {
            _submissionOptions = submissionOptions;
            _sharedBusinessLogic = sharedBusinessLogic;
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

            //Return null if no draft file exists
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath)) return null;

            //Return the draft StatementModel deserialized from file
            var draftJson = await _sharedBusinessLogic.FileRepository.ReadAsync(draftFilePath);
            var statementModel = JsonConvert.DeserializeObject<StatementModel>(draftJson);
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

            //Return null if no draft file exists
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(backupFilePath)) return null;

            //Return the draft StatementModel deserialized from file
            var draftJson = await _sharedBusinessLogic.FileRepository.ReadAsync(backupFilePath);
            var statementModel = JsonConvert.DeserializeObject<StatementModel>(draftJson);
            //TODO: Check all TypeIds are still valid in case some new ones or old retired
            return statementModel;
        }

        public async Task<Statement> FindLatestSubmittedStatementAsync(long organisationId, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            return await FindLatestSubmittedStatementAsync(organisation, reportingDeadline);
        }

        public async Task<Statement> FindLatestSubmittedStatementAsync(Organisation organisation, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisation==null) throw new ArgumentNullException(nameof(organisation));

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            var statement = organisation.Statements.FirstOrDefault(s => s.SubmissionDeadline == reportingDeadline && s.Status == StatementStatuses.Submitted);
            return statement;
        }

        /// <summary>
        /// Returns any existing Submitted Statement Entity for this organisation and reporting deadline or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The Statement entity or null if not found</returns>
        private async Task<Statement> FindSubmittedStatementAsync(long organisationId, DateTime reportingDeadline)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline == reportingDeadline && s.Status == StatementStatuses.Submitted);
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

            //Map the sectors
            statement.Sectors.Where(s => !statementModel.Sectors.Contains(s.StatementSectorTypeId)).ForEach(s => { statement.Sectors.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.Sectors.ForEach(id =>
            {
                var sector = statement.Sectors.FirstOrDefault(s => s.StatementSectorTypeId == id);
                if (sector == null)
                {
                    sector = new StatementSector() { StatementSectorTypeId = id, StatementId = statement.StatementId };
                    statement.Sectors.Add(sector);
                }
            });

            //Map the Policies
            statement.Policies.Where(s => !statementModel.Policies.Contains(s.StatementPolicyTypeId)).ForEach(s => { statement.Policies.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.Policies.ForEach(id =>
            {
                var policy = statement.Policies.FirstOrDefault(s => s.StatementPolicyTypeId == id);
                if (policy == null)
                {
                    policy = new StatementPolicy() { StatementPolicyTypeId = id, StatementId = statement.StatementId };
                    statement.Policies.Add(policy);
                }
            });

            //Map the Relevant Risks
            statement.RelevantRisks.Where(s => !statementModel.RelevantRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.RelevantRisks.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.RelevantRisks.ForEach(model =>
            {
                var relevantRisk = statement.RelevantRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (relevantRisk == null)
                {
                    relevantRisk = new StatementRelevantRisk() { StatementRiskTypeId = model.Id, Details = model.Details, StatementId = statement.StatementId };
                    statement.RelevantRisks.Add(relevantRisk);
                }
                else
                    relevantRisk.Details = model.Details;
            });

            //Map the High Risks
            statement.HighRisks.Where(s => !statementModel.HighRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.HighRisks.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.HighRisks.ForEach(model =>
            {
                var highRisk = statement.HighRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (highRisk == null)
                {
                    highRisk = new StatementHighRisk() { StatementRiskTypeId = model.Id, Details = model.Details, StatementId = statement.StatementId };
                    statement.HighRisks.Add(highRisk);
                }
                else
                    highRisk.Details = model.Details;
            });

            //Map the Location Risks
            statement.LocationRisks.Where(s => !statementModel.LocationRisks.Any(model => model.Id == s.StatementRiskTypeId)).ForEach(s => { statement.LocationRisks.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.LocationRisks.ForEach(model =>
            {
                var locationRisk = statement.LocationRisks.FirstOrDefault(s => s.StatementRiskTypeId == model.Id);
                if (locationRisk == null)
                {
                    locationRisk = new StatementLocationRisk() { StatementRiskTypeId = model.Id, Details = model.Details, StatementId = statement.StatementId };
                    statement.LocationRisks.Add(locationRisk);
                }
                else
                    locationRisk.Details = model.Details;
            });

            //Map the Due Diligences
            statement.Diligences.Where(s => !statementModel.DueDiligences.Any(model => model.Id == s.StatementDiligenceTypeId)).ForEach(s => { statement.Diligences.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.DueDiligences.ForEach(model =>
            {
                var diligence = statement.Diligences.FirstOrDefault(s => s.StatementDiligenceTypeId == model.Id);
                if (diligence == null)
                {
                    diligence = new StatementDiligence() { StatementDiligenceTypeId = model.Id, Details = model.Details, StatementId = statement.StatementId };
                    statement.Diligences.Add(diligence);
                }
                else
                    diligence.Details = model.Details;
            });

            //Map the Training
            statement.Training.Where(s => !statementModel.Training.Contains(s.StatementTrainingTypeId)).ForEach(s => { statement.Training.Remove(s); _sharedBusinessLogic.DataRepository.Delete(s); });
            statementModel.Training.ForEach(id =>
            {
                var training = statement.Training.FirstOrDefault(s => s.StatementTrainingTypeId == id);
                if (training == null)
                {
                    training = new StatementTraining() { StatementTrainingTypeId = id, StatementId = statement.StatementId };
                    statement.Training.Add(training);
                }
            });
        }

        private void CheckReportingDeadline(Organisation organisation, DateTime reportingDeadline)
        {
            var firstDeadline = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType, _sharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear);
            var currentDeadline = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType);

            if (reportingDeadline < firstDeadline || reportingDeadline > currentDeadline.AddDays(1)) throw new ArgumentOutOfRangeException(nameof(reportingDeadline));

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

        #endregion

        #region Public Methods

        public async Task<StatementInfoModel> GetStatementInfoModelAsync(long organisationId, DateTime reportingDeadline)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);

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
            if (statement != null) statementInfoModel.SubmittedStatementModifiedDate = statement?.Modified;

            //Get the draft statement info
            var statementModel = await FindDraftStatementModelAsync(organisation.OrganisationId, reportingDeadline);
            if (statementModel != null)
            {
                statementInfoModel.DraftStatementModifiedDate = statementModel.EditTimestamp;
                statementInfoModel.DraftStatementIsEmpty = statementModel.IsEmpty();
            }

            return statementInfoModel;
        }

        public async IAsyncEnumerable<StatementInfoModel> GetStatementInfoModelsAsync(Organisation organisation)
        {
            var reportingDeadline = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType, _sharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear);
            var currentReportingDeadline = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType);
            while (reportingDeadline <= currentReportingDeadline)
            {
                yield return await GetStatementInfoModelAsync(organisation, reportingDeadline);

                reportingDeadline = reportingDeadline.AddYears(1);
            }
        }

        public async Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModelAsync(long organisationId, DateTime reportingDeadline)
        {
            var statement = FindLatestSubmittedStatementAsync(organisationId, reportingDeadline);
            if (statement == null) return new Outcome<StatementErrors, StatementModel>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline {reportingDeadline}");

            var statementModel = new StatementModel();

            //Copy the statement properties to the model
            _mapper.Map(statement, statementModel);

            //Return the successful model
            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<StatementModel> GetDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
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
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId)) throw new SecurityException($"User {userId} does not have permission to view statement for organisation {organisationId}");

            //Try and get the draft first
            var backupStatementModel = await FindDraftBackupStatementModelAsync(organisationId, reportingDeadline);

            return backupStatementModel;
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId), $"Invalid organisationId {organisationId}");

            //Check the reporting deadlin
            CheckReportingDeadline(organisation, reportingDeadline);

            //Check if it is too late to edit
            if (reportingDeadline.Date.AddDays(0 - _submissionOptions.DeadlineExtensionDays) < VirtualDateTime.Now.Date)
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.TooLate);

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId))
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.Unauthorised);

            //Try and get the draft first
            var draftStatement = await FindDraftStatementModelAsync(organisationId, reportingDeadline);

            var createBackup = false;
            if (draftStatement == null)
            {
                //Create a new draft statement
                draftStatement = new StatementModel();

                //Get the statement to map from
                var submittedStatement = await FindSubmittedStatementAsync(organisationId, reportingDeadline);
                if (submittedStatement == null)
                {
                    submittedStatement = GetEmptyStatement(organisation, reportingDeadline);
                }

                //Load the statement entity into the statementmodel
                _mapper.Map(submittedStatement, draftStatement);
            }

            else
            {
                //Check if the existing draft lock has expired
                var draftExpired = draftStatement.EditTimestamp.AddMinutes(_submissionOptions.DraftTimeoutMinutes) < VirtualDateTime.Now;

                //If the draft has expired then remember to create a backup
                createBackup = draftExpired;

                //Check the existing draft is not still locked by another user
                if (draftStatement.EditorUserId > 0 && draftStatement.EditorUserId != userId && !draftExpired)
                    return new Outcome<StatementErrors, StatementModel>(StatementErrors.Locked);
            }

            //Set the key priorities
            draftStatement.EditorUserId = userId;
            draftStatement.EditTimestamp = VirtualDateTime.Now;

            // opening statements with old data should be overwritten with the newest state
            draftStatement.OrganisationName = organisation.OrganisationName;
            draftStatement.SubmissionDeadline = reportingDeadline;
            draftStatement.Submitted = organisation.Statements.Any(s => s.Status == StatementStatuses.Submitted && s.SubmissionDeadline == reportingDeadline);

            //Save new statement model with timestamp to lock to new user
            await SaveDraftStatementModelAsync(draftStatement, createBackup);
            draftStatement.DraftBackupDate = null;

            //Get the draft backup date
            var draftBackupFilePath = GetDraftBackupFilepath(draftStatement.OrganisationId, draftStatement.SubmissionDeadline.Year);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                draftStatement.DraftBackupDate = await _sharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(draftBackupFilePath);

            return new Outcome<StatementErrors, StatementModel>(draftStatement);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate the parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModelAsync(organisationId, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadline.Year);

            //Get the backup draft filepath
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadline.Year);

            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
            {
                //Restore the original draft from the backup
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftBackupFilePath, draftFilePath, true);

                //Delete the backup draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
            }

            else if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
            {
                //Delete the draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);
            }

            return new Outcome<StatementErrors>();
        }

        public async Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate the parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModelAsync(organisationId, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            var statementModel = openOutcome.Result;
            statementModel.EditorUserId = 0;

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);

            if (statementModel.IsEmpty())
                //Delete the empty draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);
            else
                //Save the draft with no userId
                await SaveStatementModelToFileAsync(statementModel, draftFilePath);

            //Get the backup draft filepath
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadline.Year);

            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
            {
                //Delete the backup draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
            }

            return new Outcome<StatementErrors>();
        }

        public async Task SaveDraftStatementModelAsync(StatementModel statementModel, bool createBackup = false)
        {
            //Validate the parameters
            if (statementModel == null) throw new ArgumentNullException(nameof(statementModel));
            if (statementModel.OrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.OrganisationId));
            if (statementModel.EditorUserId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.EditorUserId));

            //Try and get the organisation
            var organisation = _sharedBusinessLogic.DataRepository.Get<Organisation>(statementModel.OrganisationId);
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
                if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath) && await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                    await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftFilePath, draftBackupFilePath, false);
            }

            await SaveStatementModelToFileAsync(statementModel, draftFilePath);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId)
        {
            //Validate the parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModelAsync(organisationId, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            var previousStatement = await FindSubmittedStatementAsync(organisationId, reportingDeadline);
            previousStatement?.SetStatus(StatementStatuses.Retired, userId);

            var newStatement = new Statement();
            newStatement.Organisation = previousStatement == null ? await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId) : previousStatement.Organisation;
            newStatement.SetStatus(StatementStatuses.Submitted, userId);
            newStatement.Organisation.Statements.Add(newStatement);

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

            //Save the changes to the database
            await _sharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Delete the draft file and its backup
            await DeleteDraftStatementModelAsync(organisationId, reportingDeadline);

            return new Outcome<StatementErrors>();
        }

        public async Task<IList<AutoMap.Diff>> CompareToDraftBackupStatement(StatementModel newStatementModel)
        {
            //Try and find a back draft first
            var oldStatementModel = await FindDraftBackupStatementModelAsync(newStatementModel.OrganisationId, newStatementModel.SubmissionDeadline);

            //If no backup then use an empty statement
            if (oldStatementModel == null)
            {
                var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(newStatementModel.OrganisationId);
                var statement = GetEmptyStatement(organisation, newStatementModel.SubmissionDeadline);

                oldStatementModel = new StatementModel();

                //Copy the statement properties to the model
                _mapper.Map(statement, oldStatementModel);
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
                var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(newStatementModel.OrganisationId); ;
                statement = GetEmptyStatement(organisation, newStatementModel.SubmissionDeadline);
            }

            var oldStatementModel = new StatementModel();

            //Copy the statement properties to the model
            _mapper.Map(statement, oldStatementModel);

            //Return the modifications
            return GetModifications(oldStatementModel, newStatementModel);
        }


        #endregion
    }
}
