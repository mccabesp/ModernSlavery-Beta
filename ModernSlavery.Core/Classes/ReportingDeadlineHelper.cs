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
            _sharedOptions = sharedOptions;
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

        public DateTime GetReportingDeadline(SectorTypes sectorType, int reportingDeadlineYear = 0)
        {
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

            if (reportingDeadlineYear != 0) return new DateTime(reportingDeadlineYear, tempMonth, tempDay).Date;

            var now = VirtualDateTime.Now;

            var reportingDeadline = new DateTime(now.Year, tempMonth, tempDay).Date;

            return reportingDeadline.AddDays(1) < now ? reportingDeadline.AddYears(1) : reportingDeadline;
        }

        public bool IsReportingYearEditable(SectorTypes sectorType, int year)
        {
            if (year < FirstReportingDeadlineYear)
                return false;

            var currentYear = GetReportingDeadline(sectorType).Year;
            return year == currentYear || year == (currentYear - 1);
        }

        public IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0)
        {
            var firstReportingDeadline = GetFirstReportingDeadline(sectorType);
            var currentReportingDeadline = GetReportingDeadline(sectorType);

            var deadlines = new SortedSet<DateTime>();
            var deadline = currentReportingDeadline;
            var years = recentYears;
            while (deadline >= firstReportingDeadline && (recentYears == 0 || years > 0))
            {
                deadlines.Add(deadline);
                deadline = deadline.AddYears(-1);
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