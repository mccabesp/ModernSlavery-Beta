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
            if (IsSystemUser(user))return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return user.EmailAddress.LikeAny(_sharedOptions.AdminEmails.SplitI(";"));
        }

        public bool IsSuperAdministrator(User user)
        {
            if (IsAdministrator(user))return false;

            return user.EmailAddress.LikeAny(_sharedOptions.SuperAdminEmails.SplitI(";"));
        }

        public bool IsDatabaseAdministrator(User user)
        {
            if (IsAdministrator(user)) return false;

            return user.EmailAddress.LikeAny(_sharedOptions.DatabaseAdminEmails.SplitI(";"));
        }

        public bool IsSystemUser(User user)
        {
            return user.UserId == -1;
        }
    }
}
