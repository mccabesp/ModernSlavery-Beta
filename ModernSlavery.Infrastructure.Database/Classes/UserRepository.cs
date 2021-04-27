using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseOptions _databaseOptions;
        private readonly SharedOptions _sharedOptions;

        public UserRepository(DatabaseOptions databaseOptions, SharedOptions sharedOptions,
            IDataRepository dataRepository, IUserLogger userAuditLog, IMapper autoMapper)
        {
            _databaseOptions = databaseOptions ?? throw new ArgumentNullException(nameof(databaseOptions));
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            UserAuditLog = userAuditLog ?? throw new ArgumentNullException(nameof(userAuditLog));
            AutoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
        }

        public async Task<User> FindBySubjectIdAsync(string subjectId, params UserStatuses[] filterStatuses)
        {
            return await FindBySubjectIdAsync(subjectId.ToInt64(), filterStatuses).ConfigureAwait(false);
        }

        public async Task<User> FindBySubjectIdAsync(long userId, params UserStatuses[] filterStatuses)
        {
            return await DataRepository.FirstOrDefaultAsync<User>(u =>
                // filter by user id
                u.UserId == userId
                // skip or filter by user status
                && (filterStatuses.Length == 0 || filterStatuses.Contains(u.Status))).ConfigureAwait(false);
        }

        public async Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses)
        {
            if (_databaseOptions.EncryptEmails)
            {
                var encryptedEmail = Encryption.EncryptData(email.ToLower());

                var user = await DataRepository.FirstOrDefaultAsync<User>(u =>
                    // filter by email address
                    u.EmailAddress == encryptedEmail
                    // skip or filter by user status
                    && (filterStatuses.Length == 0 || filterStatuses.Contains(u.Status))).ConfigureAwait(false);

                if (user != null) return user;
            }

            return await DataRepository.FirstOrDefaultAsync<User>(u =>
                // filter by email address
                u.EmailAddress.ToLower() == email.ToLower()
                // skip or filter by user status
                && (filterStatuses.Length == 0 || filterStatuses.Contains(u.Status))).ConfigureAwait(false);
        }

        public async Task<List<User>> FindAllUsersByNameAsync(string name)
        {
            var nameForSearch = name?.ToLower();

            return await DataRepository.ToListAsync<User>(x =>
                x.Fullname.ToLower().Contains(nameForSearch) || x.ContactFullname.ToLower().Contains(nameForSearch)).ConfigureAwait(false);
        }

        public TimeSpan GetUserLoginLockRemaining(User user)=>user.LoginDate == null || user.LoginAttempts==0 || (user.LoginAttempts % _sharedOptions.MaxLoginAttempts)!=0
               ? TimeSpan.Zero
               : user.LoginDate.Value.AddMinutes(_sharedOptions.LockoutMinutes) - VirtualDateTime.Now;

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            try
            {
                user.LoginDate = VirtualDateTime.Now;
                if (!CheckPasswordBasedOnHashingAlgorithm(user, password))
                {
                    //Prevent overflow exception
                    if (user.LoginAttempts >= System.Data.SqlTypes.SqlInt32.MaxValue.Value)user.LoginAttempts = user.LoginAttempts % _sharedOptions.MaxLoginAttempts;
                    
                    user.LoginAttempts++;
                    return false;
                }

                user.LoginAttempts = 0;
                if (user.HashingAlgorithm != HashingAlgorithm.PBKDF2) await UpdateUserPasswordUsingPBKDF2Async(user, password);
                return true;
            }
            finally
            {
                //Save the changes
                await DataRepository.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateEmailAsync(User userToUpdate, string newEmailAddress)
        {
            if (userToUpdate is null) throw new ArgumentNullException(nameof(userToUpdate));

            if (string.IsNullOrWhiteSpace(newEmailAddress)) throw new ArgumentNullException(nameof(newEmailAddress));

            if (userToUpdate.Status != UserStatuses.Active)
                throw new ArgumentException($"Can only update emails for active users. UserId={userToUpdate.UserId}");

            var oldEmailAddress = userToUpdate.EmailAddress;

            // update email
            var now = VirtualDateTime.Now;
            userToUpdate.EmailAddress = newEmailAddress;
            userToUpdate.EmailVerifiedDate = now;
            userToUpdate.Modified = now;

            // save
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);

            // log email change
            await UserAuditLog.LogEmailChangedAsync(oldEmailAddress, newEmailAddress, userToUpdate,
                userToUpdate.EmailAddress).ConfigureAwait(false);
        }

        public async Task UpdatePasswordAsync(User userToUpdate, string newPassword)
        {
            if (userToUpdate is null) throw new ArgumentNullException(nameof(userToUpdate));

            if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentNullException(nameof(newPassword));

            if (userToUpdate.Status != UserStatuses.Active)
                throw new ArgumentException(
                    $"Can only update passwords for active users. UserId={userToUpdate.UserId}");

            await UpdateUserPasswordUsingPBKDF2Async(userToUpdate, newPassword);

            userToUpdate.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);

            // log password changed
            await UserAuditLog.LogPasswordChangedAsync(userToUpdate, userToUpdate.EmailAddress).ConfigureAwait(false);
        }

        public async Task<bool> UpdateDetailsAsync(User userToUpdate, UpdateDetailsModel changeDetails)
        {
            if (userToUpdate is null) throw new ArgumentNullException(nameof(userToUpdate));

            if (changeDetails is null) throw new ArgumentNullException(nameof(changeDetails));

            if (userToUpdate.Status != UserStatuses.Active)
                throw new ArgumentException($"Can only update details for active users. UserId={userToUpdate.UserId}");

            // check we have changes
            var originalDetails = AutoMapper.Map<UpdateDetailsModel>(userToUpdate);
            if (originalDetails.Equals(changeDetails)) return false;

            // update current user with new details
            userToUpdate.Firstname = changeDetails.FirstName;
            userToUpdate.Lastname = changeDetails.LastName;
            userToUpdate.JobTitle = changeDetails.JobTitle;
            userToUpdate.ContactFirstName = changeDetails.FirstName;
            userToUpdate.ContactLastName = changeDetails.LastName;
            userToUpdate.ContactJobTitle = changeDetails.JobTitle;
            userToUpdate.ContactPhoneNumber = changeDetails.ContactPhoneNumber;
            userToUpdate.SendUpdates = changeDetails.SendUpdates;
            userToUpdate.AllowContact = changeDetails.AllowContact;
            userToUpdate.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);

            // log details changed
            await UserAuditLog.LogDetailsChangedAsync(originalDetails, changeDetails, userToUpdate,
                userToUpdate.EmailAddress).ConfigureAwait(false);

            // success
            return true;
        }

        public async Task RetireUserAsync(User userToRetire)
        {
            if (userToRetire is null) throw new ArgumentNullException(nameof(userToRetire));

            if (userToRetire.Status != UserStatuses.Active)
                throw new ArgumentException($"Can only retire active users. UserId={userToRetire.UserId}");

            // update status
            userToRetire.SetStatus(UserStatuses.Retired, userToRetire, "User retired");
            userToRetire.Modified = VirtualDateTime.Now;

            // save
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);

            // log status changed
            await UserAuditLog.LogUserRetiredAsync(userToRetire, userToRetire.EmailAddress).ConfigureAwait(false);
        }

        public Task<User> AutoProvisionUserAsync(string provider, string providerUserId, List<Claim> list)
        {
            throw new NotImplementedException();
        }

        public Task<User> FindByExternalProviderAsync(string provider, string providerUserId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserPasswordUsingPBKDF2Async(User currentUser, string password)
        {
            if (currentUser == null) throw new ArgumentNullException(nameof(currentUser));

            var salt = Crypto.GetSalt();
            currentUser.Salt = Convert.ToBase64String(salt);
            currentUser.PasswordHash = Crypto.GetPBKDF2(password, salt);
            currentUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            await DataRepository.SaveChangesAsync();
        }

        private bool CheckPasswordBasedOnHashingAlgorithm(User user, string password)
        {
            switch (user.HashingAlgorithm)
            {
                case HashingAlgorithm.Unhashed:
                    if (_sharedOptions.IsProduction()) break;
                    return user.PasswordHash == password;
                case HashingAlgorithm.SHA512:
                    return user.PasswordHash == Crypto.GetSHA512Checksum(password);
                case HashingAlgorithm.PBKDF2:
                    return user.PasswordHash == Crypto.GetPBKDF2(password, Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.PBKDF2AppliedToSHA512:
                    return user.PasswordHash == Crypto.GetPBKDF2(Crypto.GetSHA512Checksum(password),
                        Convert.FromBase64String(user.Salt));
                case HashingAlgorithm.Unknown:
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Invalid enum argument: {user.HashingAlgorithm}");
            }

            throw new InvalidOperationException($"Hashing algorithm should not be {user.HashingAlgorithm}");
        }

        #region Dependencies

        public IDataRepository DataRepository { get; }

        public IUserLogger UserAuditLog { get; }
        public IMapper AutoMapper { get; }

        #endregion

        #region IDataTransaction

        public async Task ExecuteTransactionAsync(Func<Task> delegateAction)
        {
            await DataRepository.ExecuteTransactionAsync(delegateAction).ConfigureAwait(false);
        }

        public void BeginTransaction()
        {
            DataRepository.BeginTransaction();
        }

        public void CommitTransaction()
        {
            DataRepository.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            DataRepository.RollbackTransaction();
        }

        #endregion
    }
}