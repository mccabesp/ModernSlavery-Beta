using System;
using System.Collections.Generic;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

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
        IAuthorisationBusinessLogic AuthorisationBusinessLogic { get; }
        IAuthenticationBusinessLogic AuthenticationBusinessLogic { get; }
        IObfuscator Obfuscator { get; }

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        DateTime GetReportingStartDate(SectorTypes sectorType, int year = 0);
        DateTime GetReportingDeadline(SectorTypes sectorType, int year = 0);
        IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0);
    }

    public class SharedBusinessLogic : ISharedBusinessLogic
    {
        private readonly IReportingDeadlineHelper _snapshotDateHelper;
        public IObfuscator Obfuscator { get; }
        public SharedOptions SharedOptions { get; }
        public IFileRepository FileRepository { get; }
        public IDataRepository DataRepository { get; }
        public ISourceComparer SourceComparer { get; }
        public ISendEmailService SendEmailService { get; }
        public INotificationService NotificationService { get; }
        public IAuthorisationBusinessLogic AuthorisationBusinessLogic { get; }
        public IAuthenticationBusinessLogic AuthenticationBusinessLogic { get; }

        public SharedBusinessLogic(SharedOptions sharedOptions, IReportingDeadlineHelper snapshotDateHelper,
            ISourceComparer sourceComparer,
            ISendEmailService sendEmailService, 
            INotificationService notificationService, 
            IAuthorisationBusinessLogic authorisationBusinessLogic,
            IAuthenticationBusinessLogic authenticationBusinessLogic,
            IFileRepository fileRepository, IDataRepository dataRepository, IObfuscator obfuscator)
        {
            SharedOptions = sharedOptions;
            _snapshotDateHelper = snapshotDateHelper;
            SourceComparer = sourceComparer;
            SendEmailService = sendEmailService;
            NotificationService = notificationService;
            AuthorisationBusinessLogic = authorisationBusinessLogic;
            AuthenticationBusinessLogic = authenticationBusinessLogic;
            FileRepository = fileRepository;
            DataRepository = dataRepository;
            Obfuscator = obfuscator;
        }

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        public DateTime GetReportingStartDate(SectorTypes sectorType, int year = 0)
        {
            return _snapshotDateHelper.GetReportingStartDate(sectorType, year);
        }

        public DateTime GetReportingDeadline(SectorTypes sectorType, int year = 0)
        {
            return _snapshotDateHelper.GetReportingDeadline(sectorType, year);
        }

        public IList<DateTime> GetReportingDeadlines(SectorTypes sectorType, int recentYears = 0)
        {
            return _snapshotDateHelper.GetReportingDeadlines(sectorType, recentYears);
        }
    }
}