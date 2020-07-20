using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.Submission
{
    public interface IStatementBusinessLogic
    {
        /// <summary>
        /// Retrieves a readonly StatementModel of the last submitted data
        /// </summary>
        /// <param name="organisation">The organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to retrieve</param>
        /// <returns>The StatemenModel containing the submitted statement data</returns>
        Task<Outcome<StatementErrors, StatementModel>> GetSubmittedStatementModel(long organisationId, int reportingDeadlineYear);

        /// <summary>
        /// Attempts to open an existing or create a new draft StaementModel for a specific user, organisation and reporting year
        /// </summary>
        /// <param name="organisationId">The Id of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The reporting year of the statement data to retrieve</param>
        /// <param name="userId">The Id of the user who wishes to edit the statement data</param>
        /// <returns>The statement model or a list of errors</returns>
        Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModel(long organisationId, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Deletes any previously opened draft StatementModel and restores the backup
        /// </summary>
        /// <param name="statementModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task CancelEditDraftStatementModel(long organisationId, int reportingDeadlineYear);

        Task SaveDraftStatementModel(StatementModel statementModel);

        Task SubmitDraftStatementModel(StatementModel statementModel);

    }

    public enum StatementErrors : byte
    {
        Unknown = 0,
        Success = 1,
        InvalidPermissions = 2,
        Unauthorised = 3,
        Uneditable = 4,
        Locked = 5,
        TooLate=6,
        NotFound=7,
    }
}
