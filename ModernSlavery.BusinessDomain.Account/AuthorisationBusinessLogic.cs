using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
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

        public bool IsSystemUser(User user)
        {
            return user.UserId == -1;
        }

        public bool IsAdministrator(User user)
        {
            if (IsSystemUser(user))return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return user.EmailAddress.LikeAny(_sharedOptions.AdminEmails.SplitI(';'));
        }

        public bool IsSuperAdministrator(User user)
        {
            if (!IsAdministrator(user))return false;

            return user.EmailAddress.LikeAny(_sharedOptions.SuperAdminEmails.SplitI(';'));
        }

        public bool IsDatabaseAdministrator(User user)
        {
            if (!IsAdministrator(user)) return false;

            return user.EmailAddress.LikeAny(_sharedOptions.DatabaseAdminEmails.SplitI(';'));
        }

        public bool IsDevOpsAdministrator(User user)
        {
            if (!IsAdministrator(user)) return false;

            return user.EmailAddress.LikeAny(_sharedOptions.DevOpsAdminEmails.SplitI(';'));
        }

        public bool IsSystemUser(ClaimsPrincipal principle)
        {
            return principle.HasClaim("sub", "-1");
        }

        public bool IsAdministrator(ClaimsPrincipal principle)
        {
            if (IsSystemUser(principle)) return false;

            return principle.HasClaim(ClaimTypes.Role, UserRoleNames.Admin);
        }

        public bool IsSuperAdministrator(ClaimsPrincipal principle)
        {
            if (!IsAdministrator(principle)) return false;

            return principle.HasClaim(ClaimTypes.Role, UserRoleNames.SuperAdmin);
        }

        public bool IsDatabaseAdministrator(ClaimsPrincipal principle)
        {
            if (!IsAdministrator(principle)) return false;

            return principle.HasClaim(ClaimTypes.Role, UserRoleNames.DatabaseAdmin);
        }

        public bool IsDevOpsAdministrator(ClaimsPrincipal principle)
        {
            if (!IsAdministrator(principle)) return false;

            return principle.HasClaim(ClaimTypes.Role, UserRoleNames.DevOpsAdmin);
        }
    }
}
