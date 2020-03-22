using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Tests.Common.TestHelpers;

namespace ModernSlavery.Hosts.Webjob.Tests.TestHelpers
{
    public static class OrganisationHelper
    {
        public static Organisation CreateTestOrganisation(long organisationId)
        {
            return new Organisation
            {
                OrganisationId = organisationId,
                OrganisationName = $"TestOrg{organisationId:000}",
                SectorType = organisationId % 2 > 0 ? SectorTypes.Private : SectorTypes.Public,
                Status = OrganisationStatuses.Active,
                EmployerReference = $"EMP{organisationId:00000}",
                DUNSNumber = $"EMP{organisationId:000000000}"
            };
        }

        public static IEnumerable<Organisation> CreateTestOrganisations(int totalOrganisations)
        {
            //Create the test organisations
            var results = new ConcurrentBag<Organisation>();
            Parallel.For(1, totalOrganisations + 1,
                organisationId => { results.Add(CreateTestOrganisation(organisationId)); });
            return results;
        }

        public static void AddScopeStatus(ScopeStatuses scopeStatus, int year = 0, params Organisation[] orgs)
        {
            foreach (var org in orgs)
                org.OrganisationScopes.Add(
                    new OrganisationScope
                    {
                        Organisation = org,
                        ScopeStatus = scopeStatus,
                        SnapshotDate = org.SectorType.GetAccountingStartDate(year),
                        Status = ScopeRowStatuses.Active
                    });
        }
    }
}