using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Account
{
    public class AuthenticationBusinessLogic : IAuthenticationBusinessLogic
    {
        private readonly SharedOptions _sharedOptions;
        public AuthenticationBusinessLogic(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }
        public TimeSpan GetUserLoginLockRemaining(User user) =>
            user.LoginDate == null || user.LoginAttempts < _sharedOptions.MaxLoginAttempts
                ? TimeSpan.Zero
                : user.LoginDate.Value.AddMinutes(_sharedOptions.LockoutMinutes) - VirtualDateTime.Now;


    }
}
