using System;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Account.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.Queues;
using ModernSlavery.Core.Models;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.BusinessLogic.LogRecords
{

    public interface IUserLogRecord : ILogRecordLogger
    {

        Task LogEmailChangedAsync(string oldEmailAddress, string newEmailAddress, User userToUpdate, string actionByEmailAddress);

        Task LogPasswordChangedAsync(User userToUpdate, string actionByEmailAddress);

        Task LogDetailsChangedAsync(UpdateDetailsModel originalDetails,
            UpdateDetailsModel changeDetails,
            User userToUpdate,
            string emailAddress);

        Task LogUserRetiredAsync(User userToUpdate, string actionByEmailAddress);

    }

    public class UserLogRecord : LogRecordLogger, IUserLogRecord
    {

        public UserLogRecord(LogRecordQueue queue)
            : base(queue, AppDomain.CurrentDomain.FriendlyName, Filenames.UserLog) { }

        public async Task LogEmailChangedAsync(string oldEmailAddress,
            string newEmailAddress,
            User userToUpdate,
            string actionByEmailAddress)
        {
            if (userToUpdate.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

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
            if (userToUpdate.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

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
            if (userToUpdate.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

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
            if (retiredUser.EmailAddress.StartsWithI(Global.TestPrefix))
            {
                return;
            }

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
