using System;
using ModernSlavery.Extensions;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessLogic
{
    public interface ICommonBusinessLogic
    {
        DateTime GetAccountingStartDate(SectorTypes sector, int year = 0);

    }

    public class CommonBusinessLogic : ICommonBusinessLogic
    {

        private readonly IConfiguration _configuration;
        private readonly ISnapshotDateHelper _snapshotDateHelper;
        public CommonBusinessLogic(IConfiguration configuration, ISnapshotDateHelper snapshotDateHelper)
        {
            _configuration = configuration;
            _snapshotDateHelper = snapshotDateHelper;
        }

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        public DateTime GetAccountingStartDate(SectorTypes sectorType, int year = 0)
        {
            return _snapshotDateHelper.GetSnapshotDate(sectorType, year);
        }
    }

}
