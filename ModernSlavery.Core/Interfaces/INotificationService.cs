using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface INotificationService
    {
        IQueue SendNotifyEmailQueue { get; }

        void SendPinEmail(string emailAddress, string pin, string organisationName);
        void SendRemovedUserFromOrganisationEmail(string emailAddress, string organisationName, string removedUserName);
        void SendScopeChangeInEmail(string emailAddress, string organisationName);
        void SendScopeChangeOutEmail(string emailAddress, string organisationName);

        void SendSuccessfulSubmissionEmail(string emailAddress, string organisationName, string submittedOrUpdated,
            string reportingPeriod, string reportLink);

        void SendSuccessfulSubmissionEmailToRegisteredUsers(Return postedReturn, string reportLink,
            string submittedOrUpdated);

        void SendUserAddedEmailToExistingUsers(Organisation organisation, User addedUser);
        void SendUserAddedToOrganisationEmail(string emailAddress, string organisationName, string username);
    }
}