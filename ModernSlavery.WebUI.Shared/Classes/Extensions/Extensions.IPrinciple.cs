using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        #region IPrinciple

        public static User FindUser(this IDataRepository repository, IPrincipal principal)
        {
            if (principal == null) return null;

            //GEt the logged in users identifier
            var userId = principal.GetSubject();

            //If internal user the load it using the identifier as the UserID
            if (userId > 0) return repository.Get<User>(userId);

            return null;
        }
        #endregion
    }
}