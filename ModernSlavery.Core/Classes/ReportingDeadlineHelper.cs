using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Classes
{
    public class ReportingDeadlineHelper : IReportingDeadlineHelper
    {
        private readonly SharedOptions _sharedOptions;

        private int? _firstReportingDeadlineYear;

        public ReportingDeadlineHelper(SharedOptions sharedOptions)
        {
            _sharedOptions= sharedOptions;
        }

        public int FirstReportingDeadlineYear
        {
            get
            {
                if (!_firstReportingDeadlineYear.HasValue)
                    _firstReportingDeadlineYear = _sharedOptions.FirstReportingDeadlineYear;
                return _firstReportingDeadlineYear.Value;
            }
            set
            {
                _firstReportingDeadlineYear = value;
                _sharedOptions.FirstReportingDeadlineYear = value;
            }
        }

        public DateTime PrivateReportingDeadline => _sharedOptions.PrivateReportingDeadline;
        public DateTime PublicReportingDeadline => _sharedOptions.PublicReportingDeadline;
        public int CurrentSnapshotYear => GetReportingStartDate(SectorTypes.Private).Year;

        public DateTime GetReportingStartDate(SectorTypes sectorType, int year = 0)
        {
            var tempDay = 0;
            var tempMonth = 0;

            var now = VirtualDateTime.Now;

            switch (sectorType)
            {
                case SectorTypes.Private:
                    tempDay = PrivateReportingDeadline.Day;
                    tempMonth = PrivateReportingDeadline.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = PublicReportingDeadline.Day;
                    tempMonth = PublicReportingDeadline.Month;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sectorType), sectorType,
                        "Cannot calculate accounting date for this sector type");
            }

            if (year == 0) year = now.Year;

            var reportingStartDate = new DateTime(year, tempMonth, tempDay).Date.AddDays(1);

            return reportingStartDate< now ? reportingStartDate.AddYears(-1) : reportingStartDate;
        }
        public DateTime GetReportingDeadline(SectorTypes sectorType, int reportingDeadlineYear = 0)
        {
            var now = VirtualDateTime.Now;

            int tempDay;
            int tempMonth;
            switch (sectorType)
            {
                case SectorTypes.Private:
                    tempDay = PrivateReportingDeadline.Day;
                    tempMonth = PrivateReportingDeadline.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = PublicReportingDeadline.Day;
                    tempMonth = PublicReportingDeadline.Month;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sectorType), sectorType,
                        "Cannot calculate accounting date for this sector type");
            }

            if (reportingDeadlineYear == 0) reportingDeadlineYear = now.Year;

            var reportingDeadline = new DateTime(reportingDeadlineYear, tempMonth, tempDay).Date;

            return reportingDeadline < now ? reportingDeadline.AddYears(1) : reportingDeadline;
        }

        public IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0)
        {
            var firstReportingDeadline = GetFirstReportingDeadline(sectorType);
            var currentReportingDeadline = GetReportingDeadline(sectorType);

            var deadlines = new SortedSet<DateTime>();
            var deadline = currentReportingDeadline;
            var years = recentYears;
            while (deadline >= firstReportingDeadline && (recentYears==0 || years>0))
            {
                deadlines.Add(deadline);
                deadline=deadline.AddYears(-1);
                years--;
            }
            return deadlines.ToList();
        }

        public DateTime GetFirstReportingDeadline(SectorTypes sectorType)
        {
            return GetReportingDeadline(sectorType, FirstReportingDeadlineYear);
        }
    }
}