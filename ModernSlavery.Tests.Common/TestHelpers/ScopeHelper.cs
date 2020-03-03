using System;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public static class ScopeHelper
    {

        public static OrganisationScope CreateScope(ScopeStatuses scopeStatus, DateTime snapshotDate)
        {
            return new OrganisationScope {ScopeStatus = scopeStatus, Status = ScopeRowStatuses.Active, SnapshotDate = snapshotDate};
        }

    }
}
