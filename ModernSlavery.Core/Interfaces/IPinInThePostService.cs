using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IPinInThePostService
    {
        Task<SendLetterResponse> SendPinInThePostAsync(UserOrganisation userOrganisation, string pin, string returnUrl);

        List<string> GetAddressInFourLineFormat(OrganisationAddress organisationAddress);
        List<string> GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(OrganisationAddress organisationAddress);
    }
}