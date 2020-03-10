using System;
using Microsoft.Extensions.Configuration;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.BusinessLogic
{
    public interface ICommonBusinessLogic
    {
        IFileRepository FileRepository { get; }
        IDataRepository DataRepository { get; }
        ISourceComparer SourceComparer { get; }
        ISendEmailService SendEmailService { get; }
        INotificationService NotificationService { get; }

        DateTime GetAccountingStartDate(SectorTypes sector, int year = 0);

    }

    public class CommonBusinessLogic : ICommonBusinessLogic
    {
        private readonly ISnapshotDateHelper _snapshotDateHelper;
        public IFileRepository FileRepository { get; }
        public IDataRepository DataRepository { get; }
        public ISourceComparer SourceComparer { get; }
        public ISendEmailService SendEmailService { get; }
        public INotificationService NotificationService { get; }

        public CommonBusinessLogic(ISnapshotDateHelper snapshotDateHelper, ISourceComparer sourceComparer, ISendEmailService sendEmailService, INotificationService notificationService, IFileRepository fileRepository, IDataRepository dataRepository)
        {
            _snapshotDateHelper = snapshotDateHelper;
            SourceComparer = sourceComparer;
            SendEmailService = sendEmailService;
            NotificationService = notificationService;
            FileRepository = fileRepository;
            DataRepository = dataRepository;
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
