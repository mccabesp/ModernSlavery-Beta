using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public static partial class Extensions
    {
        #region IPrinciple

        public static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            IEnumerable<Claim> claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            Claim claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        public static User FindUser(this IDataRepository repository, IPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            //GEt the logged in users identifier
            long userId = principal.GetUserId();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0)
            {
                return repository.Get<User>(userId);
            }

            return null;
        }

        public static long GetUserId(this IPrincipal principal)
        {
            return principal.GetClaim("sub").ToLong();
        }
        #endregion

    }
}
