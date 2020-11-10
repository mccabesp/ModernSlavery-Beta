using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Account
{
    public class AuthorisationBusinessLogic : IAuthorisationBusinessLogic
    {
        private readonly SharedOptions _sharedOptions;
        public AuthorisationBusinessLogic(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }

        public bool IsAdministrator(User user)
        {
            if (IsSystemUser(user))
                return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(_sharedOptions.AdminEmails))
                throw new ArgumentException("Missing AdminEmails from web.config");

            return user.EmailAddress.LikeAny(_sharedOptions.AdminEmails.SplitI(";"));
        }

        public bool IsSuperAdministrator(User user)
        {
            if (IsSystemUser(user))
                return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(_sharedOptions.SuperAdminEmails))
                throw new ArgumentException("Missing SuperAdminEmails from web.config");

            return user.EmailAddress.LikeAny(_sharedOptions.SuperAdminEmails.SplitI(";"));
        }

        public bool IsDatabaseAdministrator(User user)
        {
            if (IsSystemUser(user))
                return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(_sharedOptions.DatabaseAdminEmails))
                return IsSuperAdministrator(user);

            return user.EmailAddress.LikeAny(_sharedOptions.DatabaseAdminEmails.SplitI(";"));
        }

        public bool IsSystemUser(User user)
        {
            return user.UserId == -1;
        }
    }
}
