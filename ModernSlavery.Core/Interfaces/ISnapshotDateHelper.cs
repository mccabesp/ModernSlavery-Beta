using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface ISnapshotDateHelper
    {
        int FirstReportingYear { get; set; }
        int CurrentSnapshotYear { get; }
        DateTime PrivateAccountingDate { get; }
        DateTime PublicAccountingDate { get; }
        DateTime GetSnapshotDate(SectorTypes sectorType, int year = 0);
    }
}