using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface INotificationService
    {
        IQueue SendNotifyEmailQueue { get; }

        void SendPinEmail(string emailAddress, string pin, string organisationName);
        void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName, string removedUserName);
        void SendScopeChangeOutEmail(string emailAddress, string organisationName);
    }
}