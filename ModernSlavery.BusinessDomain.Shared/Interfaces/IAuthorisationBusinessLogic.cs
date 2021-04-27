using System.Security.Claims;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IAuthorisationBusinessLogic
    {
        bool IsAdministrator(User user);
        bool IsSuperAdministrator(User user);
        bool IsDatabaseAdministrator(User user);
        bool IsDevOpsAdministrator(User user);
        bool IsSystemUser(User user);
        bool IsAdministrator(ClaimsPrincipal  principle);
        bool IsSuperAdministrator(ClaimsPrincipal principle);
        bool IsDatabaseAdministrator(ClaimsPrincipal principle);
        bool IsDevOpsAdministrator(ClaimsPrincipal principle);
        bool IsSystemUser(ClaimsPrincipal principle);
        bool IsSubmitter(User user);
        bool IsSubmitter(string emailAddress);
        bool IsAdministrator(string emailAddress);
        bool IsTrustedAddress(string testIPAddress);
    }
}