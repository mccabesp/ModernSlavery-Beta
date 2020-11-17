using System;
using System.Collections.Generic;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared
{
    public interface ISharedBusinessLogic
    {
        SharedOptions SharedOptions { get; }
        TestOptions TestOptions { get; }
        IFileRepository FileRepository { get; }
        IDataRepository DataRepository { get; }
        ISourceComparer SourceComparer { get; }
        ISendEmailService SendEmailService { get; }
        INotificationService NotificationService { get; }
        IAuthorisationBusinessLogic AuthorisationBusinessLogic { get; }
        IAuthenticationBusinessLogic AuthenticationBusinessLogic { get; }
        IReportingDeadlineHelper ReportingDeadlineHelper { get; }
        IObfuscator Obfuscator { get; }
    }

    public class SharedBusinessLogic : ISharedBusinessLogic
    {
        public IObfuscator Obfuscator { get; }
        public SharedOptions SharedOptions { get; }
        public TestOptions TestOptions { get; }
        public IFileRepository FileRepository { get; }
        public IDataRepository DataRepository { get; }
        public ISourceComparer SourceComparer { get; }
        public ISendEmailService SendEmailService { get; }
        public INotificationService NotificationService { get; }
        public IAuthorisationBusinessLogic AuthorisationBusinessLogic { get; }
        public IAuthenticationBusinessLogic AuthenticationBusinessLogic { get; }
        public IReportingDeadlineHelper ReportingDeadlineHelper { get; }
        public SharedBusinessLogic(SharedOptions sharedOptions, TestOptions testOptions, IReportingDeadlineHelper reportingDeadlineHelper,
            ISourceComparer sourceComparer,
            ISendEmailService sendEmailService, 
            INotificationService notificationService, 
            IAuthorisationBusinessLogic authorisationBusinessLogic,
            IAuthenticationBusinessLogic authenticationBusinessLogic,
            IFileRepository fileRepository, IDataRepository dataRepository, IObfuscator obfuscator)
        {
            SharedOptions = sharedOptions;
            TestOptions = testOptions;
            ReportingDeadlineHelper = reportingDeadlineHelper;
            SourceComparer = sourceComparer;
            SendEmailService = sendEmailService;
            NotificationService = notificationService;
            AuthorisationBusinessLogic = authorisationBusinessLogic;
            AuthenticationBusinessLogic = authenticationBusinessLogic;
            FileRepository = fileRepository;
            DataRepository = dataRepository;
            Obfuscator = obfuscator;
        }
    }
}