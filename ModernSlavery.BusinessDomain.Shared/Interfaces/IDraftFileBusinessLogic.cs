using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IDraftFileBusinessLogic
    {
        Task<Draft> GetDraftIfAvailableAsync(long organisationId, int snapshotYear);

        /// <summary>
        ///     Gets a Draft object with information about the status of the draft.
        ///     It returns draft object locked to the current user, or flagged as locked by someone else via the variables
        ///     'IsUserAllowedAccess' and 'LastWrittenByUserId'.
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        /// <param name="userIdRequestingAccess"></param>
        /// <returns></returns>
        Task<Draft> GetExistingOrNewAsync(long organisationId, int snapshotYear,
            long userIdRequestingAccess);

        Task<Draft> UpdateAsync(ReturnViewModel postedReturnViewModel, Draft draft,
            long userIdRequestingAccess);

        Task KeepDraftFileLockedToUserAsync(Draft draftExpectedToBeLocked, long userIdRequestingLock);
        Task DiscardDraftAsync(Draft draftToDiscard);
        Task RestartDraftAsync(long organisationId, int snapshotYear, long userIdRequestingRollback);
        Task<bool> RollbackDraftAsync(Draft draftToDiscard);
        Task CommitDraftAsync(Draft draftToDiscard);
    }
}