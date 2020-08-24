using System;
using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IReportingDeadlineHelper
    {
        int FirstReportingDeadlineYear { get; set; }
        int CurrentSnapshotYear { get; }
        DateTime PrivateReportingDeadline { get; }
        DateTime PublicReportingDeadline { get; }
        DateTime GetReportingStartDate(SectorTypes sectorType, int year = 0);
        DateTime GetReportingDeadline(SectorTypes sectorType, int year = 0);
        IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0);
        DateTime GetFirstReportingDeadline(SectorTypes sectorType);
    }
}