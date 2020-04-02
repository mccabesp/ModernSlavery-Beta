using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Classes
{
    public class SnapshotDateHelper : ISnapshotDateHelper
    {
        private readonly SharedOptions _sharedOptions;

        private int? _firstReportingYear;

        public SnapshotDateHelper(SharedOptions sharedOptions)
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

        public DateTime PrivateAccountingDate => _sharedOptions.PrivateAccountingDate;
        public DateTime PublicAccountingDate=> _sharedOptions.PublicAccountingDate;
        public int CurrentSnapshotYear => GetSnapshotDate(SectorTypes.Private).Year;

        public DateTime GetSnapshotDate(SectorTypes sectorType, int year = 0)
        {
            var tempDay = 0;
            var tempMonth = 0;

            var now = VirtualDateTime.Now;

            switch (sectorType)
            {
                case SectorTypes.Private:
                    tempDay = PrivateAccountingDate.Day;
                    tempMonth = PrivateAccountingDate.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = PublicAccountingDate.Day;
                    tempMonth = PublicAccountingDate.Month;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sectorType), sectorType,
                        "Cannot calculate accounting date for this sector type");
            }

            if (year == 0) year = now.Year;

            var tempDate = new DateTime(year, tempMonth, tempDay);

            return now > tempDate ? tempDate : tempDate.AddYears(-1);
        }
    }
}