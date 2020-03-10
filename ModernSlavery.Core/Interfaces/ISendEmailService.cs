using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface ISendEmailService
    {
        Task<bool> SendAccountClosedNotificationAsync(string emailAddress, bool test);
        Task<bool> SendChangeEmailCompletedNotificationAsync(string emailAddress);
        Task<bool> SendChangeEmailCompletedVerificationAsync(string emailAddress);
        Task<bool> SendChangeEmailPendingVerificationAsync(string verifyUrl, string emailAddress);
        Task<bool> SendChangePasswordNotificationAsync(string emailAddress);
        Task<bool> SendCreateAccountPendingVerificationAsync(string verifyUrl, string emailAddress);
        Task<bool> SendGeoMessageAsync(string subject, string message, bool test = false);
        Task<bool> SendGEOOrphanOrganisationNotificationAsync(string organisationName, bool test);
        Task<bool> SendGEORegistrationRequestAsync(string reviewUrl, string contactName, string reportingOrg, string reportingAddress, bool test = false);
        Task<bool> SendRegistrationApprovedAsync(string returnUrl, string emailAddress, bool test = false);
        Task<bool> SendRegistrationDeclinedAsync(string emailAddress, string reason);
        Task<bool> SendResetPasswordCompletedAsync(string emailAddress);
        Task<bool> SendResetPasswordNotificationAsync(string resetUrl, string emailAddress);
    }
}