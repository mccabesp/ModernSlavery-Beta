using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IReportingDeadlineHelper
    {
        int FirstReportingYear { get; set; }
        int CurrentSnapshotYear { get; }
        DateTime PrivateReportingDeadline { get; }
        DateTime PublicReportingDeadline { get; }
        DateTime GetReportingStartDate(SectorTypes sectorType, int year = 0);
    }
}