using System;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Logging
{
    public class UserAuditLogger : AuditLogger, IUserLogger
    {
        public UserAuditLogger(
            SharedOptions sharedOptions,
            LogRecordQueue queue)
            : base(sharedOptions, queue, AppDomain.CurrentDomain.FriendlyName, Filenames.UserLog)
        {
        }

        public async Task LogEmailChangedAsync(string oldEmailAddress,
            string newEmailAddress,
            User userToUpdate,
            string actionByEmailAddress)
        {
            if (userToUpdate.EmailAddress.StartsWithI(SharedOptions.TestPrefix)) return;

            await WriteAsync(
                new UserLogModel(
                    userToUpdate.UserId.ToString(),
                    userToUpdate.EmailAddress,
                    UserAction.ChangedEmail,
                    nameof(User.EmailAddress),
                    oldEmailAddress,
                    newEmailAddress,
                    actionByEmailAddress));
        }

        public async Task LogPasswordChangedAsync(User userToUpdate, string actionByEmailAddress)
        {
            if (userToUpdate.EmailAddress.StartsWithI(SharedOptions.TestPrefix)) return;

            await WriteAsync(
                new UserLogModel(
                    userToUpdate.UserId.ToString(),
                    userToUpdate.EmailAddress,
                    UserAction.ChangedPassword,
                    nameof(User.PasswordHash),
                    null,
                    null,
                    actionByEmailAddress));
        }

        public async Task LogDetailsChangedAsync(UpdateDetailsModel originalDetails,
            UpdateDetailsModel changeDetails,
            User userToUpdate,
            string actionByEmailAddress)
        {
            if (userToUpdate.EmailAddress.StartsWithI(SharedOptions.TestPrefix)) return;

            await WriteAsync(
                new UserLogModel(
                    userToUpdate.UserId.ToString(),
                    userToUpdate.EmailAddress,
                    UserAction.ChangedDetails,
                    nameof(UserAction.ChangedDetails),
                    JsonConvert.SerializeObject(originalDetails),
                    JsonConvert.SerializeObject(changeDetails),
                    actionByEmailAddress));
        }

        public async Task LogUserRetiredAsync(User retiredUser, string actionByEmailAddress)
        {
            if (retiredUser.EmailAddress.StartsWithI(SharedOptions.TestPrefix)) return;

            await WriteAsync(
                new UserLogModel(
                    retiredUser.UserId.ToString(),
                    retiredUser.EmailAddress,
                    UserAction.Retired,
                    nameof(User.Status),
                    UserStatuses.Active.ToString(),
                    UserStatuses.Retired.ToString(),
                    actionByEmailAddress));
        }
    }
}