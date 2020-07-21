using AutoMapper;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    public enum StatementErrors : byte
    {
        Unknown = 0,
        Success = 1,
        InvalidPermissions = 2,
        Unauthorised = 3,
        Uneditable = 4,
        Locked = 5,
        TooLate = 6,
        NotFound = 7,
    }

    public interface IStatementBusinessLogic
    {
        /// <summary>
        /// Retrieves a readonly StatementModel of the latest submitted data
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data to delete</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to retrieve</param>
        /// <returns>The latest submitted statement model or a list of errors</returns>
        Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModel(long organisationId, int reportingDeadlineYear);

        /// <summary>
        /// Attempts to open an existing or create a new draft StaementModel for a specific user, organisation and reporting year
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to edit the statement data</param>
        /// <returns>The statement model or a list of errors</returns>
        Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Deletes any previously opened draft StatementModel and restores the backup (if any)
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data to delete</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to submit the draft statement data</param>
        /// <returns>Nothing</returns>
        Task<Outcome<StatementErrors>> CancelDraftStatementModel(long organisationId, int reportingDeadlineYear,long userId);

        /// <summary>
        /// Saves a statement model as draft data to storage and deletes any deletes any draft data and draft backups.
        /// </summary>
        /// <param name="statementModel">The statement model to save</param>
        /// <returns>Nothing</returns>
        Task SaveDraftStatementModel(StatementModel statementModel);

        /// <summary>
        /// Saves a statement model as submitted data to storage and deletes any deletes any draft data and draft backups.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the draft statement data to submit</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to submit</param>
        /// <param name="userId">The Id of the user who wishes to submit the draft statement data</param>
        /// <returns>???</returns>
        Task<Outcome<StatementErrors>> SubmitDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId);
    }

    partial class StatementBusinessLogic : IStatementBusinessLogic
    {

        private readonly SubmissionOptions _submissionOptions;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IMapper _mapper;

        public StatementBusinessLogic(SubmissionOptions submissionOptions,ISharedBusinessLogic sharedBusinessLogic, IMapper mapper)
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
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the draft data relates</param>
        /// <returns>The full filepath of the draft data file</returns>
        private string GetDraftFilepath(long organisationId, int reportingDeadlineYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingDeadlineYear}.json");

        /// <summary>
        /// Returns the full file path and filename to use for backup of a draft file
        /// </summary>
        /// <param name="organisationId">The id of the organisation who owns the draft data </param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the draft data relates</param>
        /// <returns>The full filepath of the draft data backup file</returns>
        private string GetDraftBackupFilepath(long organisationId, int reportingDeadlineYear) => Path.Combine(_submissionOptions.DraftsPath, $"{organisationId}_{reportingDeadlineYear}.bak");

        /// <summary>
        /// Returns any existing DraftStatementModel for this organisation and reporting year or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The StatementModel or null if file not found</returns>
        private async Task<StatementModel> FindDraftStatementModelAsync(long organisationId, int reportingDeadlineYear)
        {
            //Get the draft filepath for this organisation and reporting year
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);

            //Return null if no draft file exists
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath)) return null;

            //Return the draft StatementModel deserialized from file
            var draftJson =await _sharedBusinessLogic.FileRepository.ReadAsync(draftFilePath);
            return JsonConvert.DeserializeObject<StatementModel>(draftJson);
        }

        /// <summary>
        /// Returns any existing Submitted Statement Entity for this organisation and reporting year or null if it doesnt exist
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>The Statement entity or null if not found</returns>
        private async Task<Statement> FindSubmittedStatementAsync(long organisationId, int reportingDeadlineYear)
        {
            return await _sharedBusinessLogic.DataRepository .FirstOrDefaultAsync<Statement>(s => s.OrganisationId == organisationId && s.SubmissionDeadline.Year == reportingDeadlineYear);
        }

        /// <summary>
        /// Deletes any existing Draft Statement model files (and backups) for this organisation and reporting year
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadline to which the statement data relates</param>
        /// <returns>Nothing</returns>
        private async Task DeleteDraftStatementModelAsync(long organisationId, int reportingDeadlineYear)
        {
            //Delete the original
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftFilePath);

            //Delete the backup
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadlineYear);
            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
        }
        #endregion

        #region Public Methods
        public async Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModel(long organisationId, int reportingDeadlineYear)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (reportingDeadlineYear < _sharedBusinessLogic.SharedOptions.FirstReportingYear || reportingDeadlineYear > VirtualDateTime.Now.Year + 1) throw new ArgumentOutOfRangeException(nameof(reportingDeadlineYear));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId),$"Invalid organisationId {organisationId}");

            var statement = organisation.Statements.FirstOrDefault(s => s.SubmissionDeadline.Year == reportingDeadlineYear);
            if (statement == null) return new Outcome<StatementErrors, StatementModel>(StatementErrors.NotFound);

            var statementModel = new StatementModel();

            //Copy the statement properties to the model
            _mapper.Map(statement, statementModel);

            //Return the successful model
            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId)
        {
            //Validate method parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (reportingDeadlineYear < _sharedBusinessLogic.SharedOptions.FirstReportingYear || reportingDeadlineYear > VirtualDateTime.Now.Year + 1) throw new ArgumentOutOfRangeException(nameof(reportingDeadlineYear));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            //Try and get the organisation
            var organisation = await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId),$"Invalid organisationId {organisationId}");

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(userId))
                return new Outcome<StatementErrors, StatementModel>(StatementErrors.Unauthorised);

            //Try and get the draft first
            var statementModel = await FindDraftStatementModelAsync(organisationId, reportingDeadlineYear);

            //Check if the existing statement is owned by another user
            if (statementModel != null && statementModel.UserId != userId)
            { 
                //Check if the existing statement is still locked 
                if (statementModel.Timestamp.AddMinutes(_submissionOptions.DraftTimeoutMinutes) > VirtualDateTime.Now)
                    return new Outcome<StatementErrors, StatementModel>(StatementErrors.Locked);

                //Delete the other users draft file and its backup
                await DeleteDraftStatementModelAsync(organisationId, reportingDeadlineYear);

                statementModel = null;
            }

            if (statementModel == null)
            {
                statementModel = new StatementModel();
                statementModel.UserId = userId;
                statementModel.Timestamp = VirtualDateTime.Now;

                var submittedStatement = await FindSubmittedStatementAsync(organisationId, reportingDeadlineYear);
                if (submittedStatement != null)
                {
                    //Check its not too late to edit 
                    if (submittedStatement.SubmissionDeadline.Date.AddDays(0 - _submissionOptions.DeadlineExtensionDays) < VirtualDateTime.Now.Date)
                        return new Outcome<StatementErrors, StatementModel>(StatementErrors.TooLate);

                    //Load data from statement entity into the statementmodel
                    _mapper.Map(submittedStatement, statementModel);
                }
            }

            //Save new statement model with timestamp to lock to new user
            await SaveDraftStatementModel(statementModel);

            var draftBackupFilePath = GetDraftBackupFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
                statementModel.BackupDate=await _sharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(draftBackupFilePath);

            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId)
        {
            //Validate the parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (reportingDeadlineYear < _sharedBusinessLogic.SharedOptions.FirstReportingYear || reportingDeadlineYear > VirtualDateTime.Now.Year + 1) throw new ArgumentOutOfRangeException(nameof(reportingDeadlineYear));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModel(organisationId, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(organisationId, reportingDeadlineYear);

            //Get the backup draft filepath
            var draftBackupFilePath = GetDraftBackupFilepath(organisationId, reportingDeadlineYear);

            if (await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath))
            {
                //Restore the original draft from the backup
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftBackupFilePath, draftFilePath, true);

                //Delete the backup draft
                await _sharedBusinessLogic.FileRepository.DeleteFileAsync(draftBackupFilePath);
            }
            return new Outcome<StatementErrors>();
        }

        public async Task SaveDraftStatementModel(StatementModel statementModel)
        {
            //Validate the parameters
            if (statementModel == null) throw new ArgumentNullException(nameof(statementModel));
            if (statementModel.OrganisationId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.OrganisationId));
            if (statementModel.UserId == 0) throw new ArgumentOutOfRangeException(nameof(statementModel.UserId));
            if (statementModel.Year<_sharedBusinessLogic.SharedOptions.FirstReportingYear || statementModel.Year >VirtualDateTime.Now.Year+1) throw new ArgumentOutOfRangeException(nameof(statementModel.Year));

            //Try and get the organisation
            var organisation = _sharedBusinessLogic.DataRepository.Get<Organisation>(statementModel.OrganisationId);
            if (organisation==null) throw new ArgumentOutOfRangeException(nameof(statementModel.OrganisationId));

            //Check the user can edit this statement
            if (!organisation.GetUserIsRegistered(statementModel.UserId))
                throw new SecurityException($"User {statementModel.UserId} does not have permission to submit for organisation {statementModel.OrganisationId}");

            //Get the current draft filepath
            var draftFilePath = GetDraftFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);

            //Create a backup if required
            var draftBackupFilePath = GetDraftBackupFilepath(statementModel.OrganisationId, statementModel.SubmissionDeadline.Year);
            if (!await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftBackupFilePath) && await _sharedBusinessLogic.FileRepository.GetFileExistsAsync(draftFilePath))
                await _sharedBusinessLogic.FileRepository.CopyFileAsync(draftFilePath, draftBackupFilePath, false);

            //Save the new draft data 
            var draftJson = JsonConvert.SerializeObject(statementModel);
            await _sharedBusinessLogic.FileRepository.WriteAsync(draftFilePath, Encoding.UTF8.GetBytes(draftJson));
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId)
        {
            //Validate the parameters
            if (organisationId <= 0) throw new ArgumentOutOfRangeException(nameof(organisationId));
            if (reportingDeadlineYear < _sharedBusinessLogic.SharedOptions.FirstReportingYear || reportingDeadlineYear > VirtualDateTime.Now.Year + 1) throw new ArgumentOutOfRangeException(nameof(reportingDeadlineYear));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var openOutcome = await OpenDraftStatementModel(organisationId, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            var previousStatement = await FindSubmittedStatementAsync(organisationId, reportingDeadlineYear);
            previousStatement?.SetStatus(StatementStatuses.Retired, userId);

            var organisation = previousStatement.Organisation ?? await _sharedBusinessLogic.DataRepository.GetAsync<Organisation>(organisationId);
            var newStatement = new Statement();
            newStatement.Organisation = organisation;
            newStatement.SetStatus(StatementStatuses.Submitted, userId);
            organisation.Statements.Add(newStatement);

            //Copy all the other model propertiesto the entity
            _mapper.Map(statementModel, newStatement);

            //Save the changes to the database
            await _sharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Delete the draft file and its backup
            await DeleteDraftStatementModelAsync(organisationId, reportingDeadlineYear);

            return new Outcome<StatementErrors>();
        }
        #endregion
    }
}
