using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public static class ScopeHelper
    {
        public static OrganisationScope CreateScope(ScopeStatuses scopeStatus, DateTime snapshotDate)
        {
            return new OrganisationScope
                {ScopeStatus = scopeStatus, Status = ScopeRowStatuses.Active, SnapshotDate = snapshotDate};
        }
    }
}