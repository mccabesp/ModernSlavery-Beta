using System;
using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IReportingDeadlineHelper
    {
        int FirstReportingDeadlineYear { get; set; }
        DateTime PrivateReportingDeadline { get; }
        DateTime PublicReportingDeadline { get; }
        DateTime GetReportingDeadline(SectorTypes sectorType, int year = 0);
        bool IsReportingYearEditable(SectorTypes sectorType, int year);
        IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0);
        DateTime GetFirstReportingDeadline(SectorTypes sectorType);
    }
}