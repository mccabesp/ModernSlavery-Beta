﻿using System;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Logging
{
    public class UserAuditLogger : AuditLogger, IUserLogger
    {
        public UserAuditLogger(
            SharedOptions sharedOptions,
            TestOptions testOptions,
            LogRecordQueue queue)
            : base(sharedOptions, testOptions, queue, Filenames.UserLog)
        {
        }

        public async Task LogEmailChangedAsync(string oldEmailAddress,
            string newEmailAddress,
            User userToUpdate,
            string actionByEmailAddress)
        {
            if (userToUpdate.EmailAddress.StartsWithI(TestOptions.TestPrefix)) return;

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
            if (userToUpdate.EmailAddress.StartsWithI(TestOptions.TestPrefix)) return;

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
            if (userToUpdate.EmailAddress.StartsWithI(TestOptions.TestPrefix)) return;

            await WriteAsync(
                new UserLogModel(
                    userToUpdate.UserId.ToString(),
                    userToUpdate.EmailAddress,
                    UserAction.ChangedDetails,
                    nameof(UserAction.ChangedDetails),
                    Core.Extensions.Json.SerializeObject(originalDetails),
                    Core.Extensions.Json.SerializeObject(changeDetails),
                    actionByEmailAddress));
        }

        public async Task LogUserRetiredAsync(User retiredUser, string actionByEmailAddress)
        {
            if (retiredUser.EmailAddress.StartsWithI(TestOptions.TestPrefix)) return;

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