using Microsoft.Extensions.Configuration;
using ModernSlavery.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.SharedKernel.Interfaces
{
    public interface ISnapshotDateHelper
    {
        int FirstReportingYear { get; set; }
        int CurrentSnapshotYear { get; }
        DateTime PrivateAccountingDate { get; }
        DateTime PublicAccountingDate { get; }
        DateTime GetSnapshotDate(SectorTypes sectorType, int year = 0);
    }

    public class SnapshotDateHelper : ISnapshotDateHelper
    {
        public SnapshotDateHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            PrivateAccountingDate = _configuration.GetValue<DateTime>("PrivateAccountingDate");
            PublicAccountingDate = _configuration.GetValue<DateTime>("PublicAccountingDate");

        }

        public int? _FirstReportingYear;
        public int FirstReportingYear
        {
            get
            {

                if (!_FirstReportingYear.HasValue) _FirstReportingYear = _configuration.GetValue("FirstReportingYear", 2017);
                return _FirstReportingYear.Value;
            }
            set
            {
                _FirstReportingYear = value;
                _configuration["FirstReportingYear"] = value.ToString();
            }
        }

        public DateTime PrivateAccountingDate { get; }
        public DateTime PublicAccountingDate { get; }
        public int CurrentSnapshotYear => GetSnapshotDate(SectorTypes.Private).Year;

        readonly IConfiguration _configuration;

        public DateTime GetSnapshotDate(SectorTypes sectorType, int year = 0)
        {
            var tempDay = 0;
            var tempMonth = 0;

            DateTime now = VirtualDateTime.Now;

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
                    throw new ArgumentOutOfRangeException(nameof(sectorType), sectorType, "Cannot calculate accounting date for this sector type");
            }

            if (year == 0)
            {
                year = now.Year;
            }

            var tempDate = new DateTime(year, tempMonth, tempDay);

            return now > tempDate ? tempDate : tempDate.AddYears(-1);
        }
    }
}
