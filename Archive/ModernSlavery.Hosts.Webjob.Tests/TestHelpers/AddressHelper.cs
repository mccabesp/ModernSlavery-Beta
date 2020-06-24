﻿using ModernSlavery.Core.Entities;

namespace ModernSlavery.Hosts.Webjob.Tests.TestHelpers
{
    public static class AddressHelper
    {
        public static OrganisationAddress CreateTestAddress(long organisationId)
        {
            return new OrganisationAddress
            {
                OrganisationId = organisationId,
                Status = AddressStatuses.Active,
                CreatedByUserId = -1,
                Address1 = $"Address{organisationId:000}",
                TownCity = $"City{organisationId:000}",
                Country = $"Country{organisationId:000}"
            };
        }
    }
}