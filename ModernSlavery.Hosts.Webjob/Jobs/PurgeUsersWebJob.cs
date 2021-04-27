using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class PurgeUsersWebJob : WebJob
    {
        #region Dependencies
        private readonly IAuditLogger _manualChangeLog;
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        #endregion

        public PurgeUsersWebJob(
            [KeyFilter(Filenames.ManualChangeLog)] IAuditLogger manualChangeLog,
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IDataRepository dataRepository)
        {
            _manualChangeLog = manualChangeLog;
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
        }
        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        [Disable(typeof(DisableWebJobProvider))]
        public async Task PurgeUsersAsync([TimerTrigger("%PurgeUsers%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeUsersAsync))) return;
            RunningJobs.Add(nameof(PurgeUsersAsync));
            try
            {
                var deletedCount = 0;
                var deadline = VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnverifiedUserDays);
                var users = await _dataRepository.GetAll<User>().Where(u => u.UserId > 0 && u.EmailVerifiedDate == null && (u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline)).ToListAsync().ConfigureAwait(false);
                var pinExpireyDate = VirtualDateTime.Now.AddDays(0 - _sharedOptions.PinInPostExpiryDays);
                foreach (var user in users)
                {
                    //Ignore if they have verified PIN
                    if (user.UserOrganisations.Any(uo => uo.PINConfirmedDate != null || uo.PINSentDate != null && uo.PINSentDate < pinExpireyDate))
                        continue;
                    
                    //Delete the AuditLog records and create log entries
                    var auditItems = await _dataRepository.GetAll<AuditLog>().Where(al => al.ImpersonatedUserId == user.UserId && (al.Action == AuditedAction.AdminResendVerificationEmail || al.Action == AuditedAction.AdminChangeUserContactPreferences)).ToListAsync().ConfigureAwait(false);
                    var auditLogItems = auditItems.Select(auditItem =>
                        new ManualChangeLogModel(
                            nameof(PurgeUsersAsync),
                            ManualActions.Delete,
                            $"TABLE {nameof(AuditLog)}",
                            nameof(user.UserId),
                            user.UserId.ToString(),
                            null,
                            Json.SerializeObject(new { auditItem.AuditLogId, auditItem.Action, ImpersonatedUserEmail = auditItem.ImpersonatedUser?.EmailAddress, ImpersonatedUserName = auditItem.ImpersonatedUser?.GetNameAndTitle(), OriginalUserEmail = auditItem.OriginalUser?.EmailAddress, OriginalUserName = auditItem.OriginalUser?.GetNameAndTitle(), auditItem.Details, auditItem.CreatedDate }),
                            null));
                    foreach (var auditItem in auditItems)
                        _dataRepository.Delete(auditItem);

                    //Delete the User records and create log entries
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeUsersAsync),
                        ManualActions.Delete,
                        $"TABLE {nameof(User)}",
                        nameof(user.UserId),
                        user.UserId.ToString(),
                        null,
                        Json.SerializeObject(new { user.UserId, user.EmailAddress, user.JobTitle, user.Fullname }),
                        null);
                    _dataRepository.Delete(user);

                    //Save the deletions
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    deletedCount++;

                    //Write the log entries
                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                    auditLogItems.ForEach(async auditLogItem => await _manualChangeLog.WriteAsync(auditLogItem).ConfigureAwait(false));
                }

                log.LogDebug($"Executed WebJob {nameof(PurgeUsersAsync)} successfully. Deleted: {deletedCount}.");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeUsersAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeUsersAsync));
            }

        }
    }
}