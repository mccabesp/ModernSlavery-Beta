using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IStatementBusinessLogic
    {
        /// <summary>
        /// Returns the summary information for a statement and its draft
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data</param>
        /// <returns>A summary of the statement submission and its draft</returns>
        Task<StatementInfoModel> GetStatementInfoModelAsync(long organisationId, DateTime reportingDeadline);

        /// <summary>
        /// Returns the summary information for a statement and its draft
        /// </summary>
        /// <param name="organisation">The organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting year of the statement data</param>
        /// <returns>A summary of the statement submission and its draft</returns>
        Task<StatementInfoModel> GetStatementInfoModelAsync(Organisation organisation, DateTime reportingDeadline);

        /// <summary>
        /// Returns summary information for all submitted and draft statements for an organisation
        /// </summary>
        /// <param name="organisation">The organisation who owns the statement data</param>
        /// <returns>An enumerable list the submitted and draft statements</returns>
        IAsyncEnumerable<StatementInfoModel> GetStatementInfoModelsAsync(Organisation organisation);

        /// <summary>
        /// Retrieves a readonly StatementModel of the latest submitted data
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data to delete</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <returns>The latest submitted statement model or a list of errors</returns>
        Task<Outcome<StatementErrors, StatementModel>> GetLatestSubmittedStatementModelAsync(long organisationId, DateTime reportingDeadline);

        /// <summary>
        /// Returns list of modifications between the specified statementModel and the draft backup)
        /// </summary>
        /// <param name="newStatementModel">The new statement</param>
        /// <returns>A list of modifications, null if no differences, or all changes from empty if no backup</returns>
        Task<IList<AutoMap.Diff>> CompareToDraftBackupStatement(StatementModel statementModel);

        /// <summary>
        /// Returns list of modifications between the specified statementModel and the latest submitted statement
        /// </summary>
        /// <param name="newStatementModel">The new statement</param>
        /// <returns>A list of modifications, null if no differences, or all changes from empty if no submitted statement</returns>
        Task<IList<AutoMap.Diff>> CompareToSubmittedStatement(StatementModel newStatementModel);

        /// <summary>
        /// Attempts to open an existing draft StaementModel (if it exists) for a specific user, organisation and reporting deadline
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to view the statement data</param>
        /// <returns>The statement model or a list of errors</returns>
        Task<StatementModel> GetDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId);

        /// <summary>
        /// Attempts to open an existing backup draft StatementModel (if it exists) for a specific organisation and reporting deadline
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to view the backup statement data</param>
        /// <returns>The statement model or a list of errors</returns>
        Task<StatementModel> GetDraftBackupStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId);

        /// <summary>
        /// Attempts to open an existing or create a new draft StaementModel for a specific user, organisation and reporting deadline
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to edit the statement data</param>
        /// <returns>The statement model or a list of errors</returns>
        Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId);

        /// <summary>
        /// Unlocks a open draft statement mode from a particular user 
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who currently owns the statement data</param>
        /// <returns></returns>
        Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId);

        /// <summary>
        /// Deletes any previously opened draft StatementModel and restores the backup (if any)
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data to delete</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to submit the draft statement data</param>
        /// <returns>Nothing</returns>
        Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId);


        /// <summary>
        /// Saves a statement model as draft data to storage and deletes any deletes any draft data and draft backups.
        /// </summary>
        /// <param name="statementModel">The statement model to save</param>
        /// <param name="createBackup">Whether to backup any previous file (default=false)</param>
        /// <param name="deleteBackup">Whether to delete any existing backup (default=false)</param>
        /// <returns>Nothing</returns>
        Task SaveDraftStatementModelAsync(StatementModel statementModel, bool createBackup = false);


        /// <summary>
        /// Saves a statement model as submitted data to storage and deletes any deletes any draft data and draft backups.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the draft statement data to submit</param>
        /// <param name="reportingDeadline">The reporting deadline of the statement data to submit</param>
        /// <param name="user">The the user who wishes to submit the draft statement data</param>
        /// <returns>???</returns>
        Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(long organisationId, DateTime reportingDeadline, long userId=-1);
    }
}
