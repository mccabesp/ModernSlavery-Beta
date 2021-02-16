using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace ModernSlavery.Core.Extensions
{
    public static class Principle
    {
        #region IPrinciple

        public static string GetClaim(this IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated) return null;

            var claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            var claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        public static long GetSubject(this IPrincipal principal)
        {
            return principal.GetClaim("sub").ToLong();
        }

        public static string GetEmail(this IPrincipal principal)
        {
            return principal.GetClaim(ClaimTypes.Email);
        }

        public static string GetName(this IPrincipal principal)
        {
            return principal.GetClaim("name");
        }

        #endregion
    }
}