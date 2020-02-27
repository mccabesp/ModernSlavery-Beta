﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Account.Models;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.BusinessLogic.Account.Abstractions
{

    public interface IUserRepository : IDataTransaction
    {

        Task<bool> CheckPasswordAsync(User user, string password);

        Task<User> FindBySubjectIdAsync(long subjectId, params UserStatuses[] filterStatuses);

        Task<User> FindBySubjectIdAsync(string subjectId, params UserStatuses[] filterStatuses);

        Task<User> FindByEmailAsync(string email, params UserStatuses[] filterStatuses);

        Task<List<User>> FindAllUsersByNameAsync(string name);

        Task<User> AutoProvisionUserAsync(string provider, string providerUserId, List<Claim> list);

        Task<User> FindByExternalProviderAsync(string provider, string providerUserId);

        Task UpdateEmailAsync(User userToUpdate, string newEmailAddress);

        Task UpdatePasswordAsync(User userToUpdate, string newPassword);

        Task<bool> UpdateDetailsAsync(User userToUpdate, UpdateDetailsModel changeDetails);

        Task RetireUserAsync(User userToRetire);
        void UpdateUserPasswordUsingPBKDF2(User currentUser, string password);
    }

}
