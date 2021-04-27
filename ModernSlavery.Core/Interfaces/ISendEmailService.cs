using System;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface ISendEmailService
    {
        Task<bool> SendAccountClosedNotificationAsync(string emailAddress);
        Task<bool> SendChangeEmailCompletedNotificationAsync(string emailAddress);
        Task<bool> SendChangeEmailCompletedVerificationAsync(string emailAddress);
        Task<bool> SendChangeEmailPendingVerificationAsync(string verifyUrl, string emailAddress);
        Task<bool> SendChangePasswordNotificationAsync(string emailAddress);
        Task<bool> SendCreateAccountPendingVerificationAsync(string verifyUrl, string emailAddress);
        Task<bool> SendMsuMessageAsync(string subject, string message);
        Task<bool> SendMsuOrphanOrganisationNotificationAsync(string organisationName);

        Task<bool> SendMsuRegistrationRequestAsync(string reviewUrl, string contactName, string reportingOrg,string reportingAddress, string details=null);

        Task<bool> SendRegistrationApprovedAsync(string organisationName, string returnUrl, string emailAddress);
        Task<bool> SendRegistrationDeclinedAsync(string organisationName, string emailAddress, string reason);
        Task<bool> SendResetPasswordCompletedAsync(string emailAddress);
        Task<bool> SendResetPasswordNotificationAsync(string resetUrl, string emailAddress);
        Task<bool> SendPinEmailAsync(string emailAddress, string pin, string organisationName, string url, DateTime expiresDate);
    }
}