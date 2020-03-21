using Autofac;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.Classes;
using ModernSlavery.SharedKernel.Extensions;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebJob
{
    public class WebjobHostDependencyModule: IDependencyModule
    {
        private readonly GlobalOptions _globalOptions;
        private readonly StorageOptions _storageOptions;
        public WebjobHostDependencyModule(GlobalOptions globalOptions, StorageOptions storageOptions)
        {
            _globalOptions = globalOptions;
            _storageOptions = storageOptions;
        }

        public void Bind(ContainerBuilder builder)
        {
            //Register the database dependencies
            builder.BindResolvedDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.BindResolvedDependencyModule<FileStorageDependencyModule>();

            //Register the log storage dependencies
            builder.BindResolvedDependencyModule<LogStorageDependencyModule>();

            //Register the search dependencies
            builder.BindResolvedDependencyModule<SearchDependencyModule>();

            //Register the companies house dependencies
            builder.BindResolvedDependencyModule<CompaniesHouseModule>();

            //Register the messaging dependencies
            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

            // Register the email template dependencies
            builder.RegisterInstance(new EmailTemplateRepository(FileSystem.ExpandLocalPath("~/App_Data/EmailTemplates"))).As<IEmailTemplateRepository>().SingleInstance();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", _globalOptions.ObfuscationSeed);
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            // Register email provider dependencies
            builder.RegisterType<GovNotifyEmailProvider>().SingleInstance();
            builder.RegisterType<SmtpEmailProvider>().SingleInstance();
            builder.RegisterType<EmailProvider>().SingleInstance();

            #region Register the busines logic dependencies
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().InstancePerDependency();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerDependency();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerDependency();
            #endregion

            // Need to register webJob class in Autofac as well
            builder.RegisterType<Functions>().InstancePerDependency();
            builder.RegisterType<DisableWebjobProvider>().SingleInstance();

        }
    }
}
