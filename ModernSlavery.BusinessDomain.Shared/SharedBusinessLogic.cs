using System;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.BusinessDomain.Shared
{
    public interface ISharedBusinessLogic
    {
        SharedOptions SharedOptions { get; }
        IFileRepository FileRepository { get; }
        IDataRepository DataRepository { get; }
        ISourceComparer SourceComparer { get; }
        ISendEmailService SendEmailService { get; }
        INotificationService NotificationService { get; }
        IObfuscator Obfuscator { get; }
        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        DateTime GetAccountingStartDate(SectorTypes sectorType, int year = 0);
    }

    public class SharedBusinessLogic : ISharedBusinessLogic
    {
        private readonly ISnapshotDateHelper _snapshotDateHelper;

        public SharedBusinessLogic(SharedOptions sharedOptions, ISnapshotDateHelper snapshotDateHelper, ISourceComparer sourceComparer,
            ISendEmailService sendEmailService, INotificationService notificationService,
            IFileRepository fileRepository, IDataRepository dataRepository, IObfuscator obfuscator)
        {
            SharedOptions = sharedOptions;
            _snapshotDateHelper = snapshotDateHelper;
            SourceComparer = sourceComparer;
            SendEmailService = sendEmailService;
            NotificationService = notificationService;
            FileRepository = fileRepository;
            DataRepository = dataRepository;
            Obfuscator = obfuscator;
        }
        public IObfuscator Obfuscator { get; }
        public SharedOptions SharedOptions { get; }
        public IFileRepository FileRepository { get; }
        public IDataRepository DataRepository { get; }
        public ISourceComparer SourceComparer { get; }
        public ISendEmailService SendEmailService { get; }
        public INotificationService NotificationService { get; }

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