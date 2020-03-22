using ModernSlavery.Core.Entities;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public class OrganisationScopeHelper
    {
        public static OrganisationScope GetOrganisationScope(int snapshotYear, SectorTypes organisationSectorType)
        {
            return new OrganisationScope
            {
                SnapshotDate = SectorTypeHelper.GetSnapshotDateForSector(snapshotYear, organisationSectorType),
                Status = ScopeRowStatuses.Active
            };
        }

        public static OrganisationScope GetOrganisationScope_InScope(int snapshotYear,
            SectorTypes organisationSectorType)
        {
            return GetOrgScopeWithThisScope(snapshotYear, organisationSectorType, ScopeStatuses.InScope);
        }

        public static OrganisationScope GetOrgScopeWithThisScope(int snapshotYear, SectorTypes organisationSectorType,
            ScopeStatuses scope)
        {
            if (snapshotYear == 0) snapshotYear = organisationSectorType.GetAccountingStartDate().Year;

            var org = GetOrganisationScope(snapshotYear, organisationSectorType);
            org.ScopeStatus = scope;
            return org;
        }
    }
}