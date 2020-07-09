using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Classes
{
    public class ReportingDeadlineHelper : IReportingDeadlineHelper
    {
        private readonly SharedOptions _sharedOptions;

        private int? _firstReportingYear;

        public ReportingDeadlineHelper(SharedOptions sharedOptions)
        {
            _sharedOptions= sharedOptions;
        }

        public int FirstReportingYear
        {
            get
            {
                if (!_firstReportingYear.HasValue)
                    _firstReportingYear = _sharedOptions.FirstReportingYear;
                return _firstReportingYear.Value;
            }
            set
            {
                _firstReportingYear = value;
                _sharedOptions.FirstReportingYear = value;
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
    }
}