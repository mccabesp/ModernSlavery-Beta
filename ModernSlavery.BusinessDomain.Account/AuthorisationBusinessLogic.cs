using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthorisationBusinessLogic> _logger;
        private readonly SharedOptions _sharedOptions;

        public AuthorisationBusinessLogic(ILogger<AuthorisationBusinessLogic> logger, SharedOptions sharedOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
        }

        public bool IsSystemUser(string emailAddress)
        {
            return emailAddress.EqualsI("SYSTEM");
        }

        public bool IsSystemUser(User user)
        {
            return user.UserId == -1;
        }

        public bool IsSubmitter(User user)
        {
            if (IsSystemUser(user)) return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return user.EmailAddress.LikeAny(_sharedOptions.SubmitterEmails.SplitI(';'));
        }

        public bool IsSubmitter(string emailAddress)
        {
            if (IsSystemUser(emailAddress)) return false;

            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return emailAddress.LikeAny(_sharedOptions.SubmitterEmails.SplitI(';'));
        }

        public bool IsAdministrator(User user)
        {
            if (IsSystemUser(user))return false;

            if (!user.EmailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return user.EmailAddress.LikeAny(_sharedOptions.AdminEmails.SplitI(';'));
        }

        public bool IsAdministrator(string emailAddress)
        {
            if (IsSystemUser(emailAddress)) return false;

            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            return emailAddress.LikeAny(_sharedOptions.AdminEmails.SplitI(';'));
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

            return principle.HasClaim(ClaimTypes.Role, UserRoleNames.BasicAdmin);
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

        public bool IsTrustedAddress(string testIPAddress)
        {
            if (_sharedOptions.TrustedDomainsOrIPArray == null || _sharedOptions.TrustedDomainsOrIPArray.Length == 0) return true;
            if (string.IsNullOrWhiteSpace(testIPAddress)) throw new ArgumentNullException(nameof(testIPAddress));
            try
            {
                return Networking.IsTrustedAddress(testIPAddress, _sharedOptions.TrustedDomainsOrIPArray);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed test of IP: '{testIPAddress}' against whitelist: '{_sharedOptions.TrustedDomainsOrIPs}'");
            }
            return false;
        }
    }
}
