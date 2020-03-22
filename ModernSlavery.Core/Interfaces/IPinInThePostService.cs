using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IPinInThePostService
    {
        bool SendPinInThePost(UserOrganisation userOrganisation, string pin, string returnUrl, out string letterId);
        List<string> GetAddressInFourLineFormat(Organisation organisation);
        List<string> GetAddressComponentsWithoutRepeatsOrUnnecessaryComponents(Organisation organisation);
    }
}