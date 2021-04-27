using System;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface INotificationService
    {
        Task SendRemovedUserFromOrganisationEmailAsync(string emailAddress, string organisationName, string removedUserName, string removedByUserName);
        Task SendScopeChangeOutEmailAsync(string emailAddress, string organisationName, string contactName, string period, string address, string[] reasons);
    }
}