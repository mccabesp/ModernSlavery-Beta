using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Admin;
using ModernSlavery.BusinessLogic.Classes;
using ModernSlavery.BusinessLogic.Register;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.SharedKernel.Extensions;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewServices;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.WebUI.Register.Classes;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Services;

namespace ModernSlavery.WebUI
{
    public class WebHostDependencyModule: IDependencyModule
    {
        private readonly GlobalOptions _globalOptions;
        public WebHostDependencyModule(GlobalOptions globalOptions)
        {
            _globalOptions = globalOptions;
        }

        public void Bind(ContainerBuilder builder)
        {
            //Register the database dependencies
            builder.BindResolvedDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.BindResolvedDependencyModule<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            builder.BindResolvedDependencyModule<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            builder.BindResolvedDependencyModule<LogStorageDependencyModule>();

            //Register the search dependencies
            builder.BindResolvedDependencyModule<SearchDependencyModule>();

            //Register Email queuers
            builder.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();

            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            //Register the messaging dependencies
            builder.BindResolvedDependencyModule<MessagingDependencyModule>();


            //Register the companies house dependencies
            builder.BindResolvedDependencyModule<CompaniesHouseModule>();

            //Register public and private repositories
            builder.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();

            builder.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();

            //Register business logic and services
            // BL Services
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();

            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();

            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().SingleInstance();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().SingleInstance();
            builder.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>().InstancePerLifetimeScope();

            builder.RegisterType<RegistrationService>().As<IRegistrationService>().InstancePerLifetimeScope();
            builder.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();
            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // register web ui services
            builder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingPresenter>().As<IViewingPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ComparePresenter>().As<IComparePresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
            builder.RegisterType<AdminSearchService>().As<AdminSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", _globalOptions.ObfuscationSeed);
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            //Register factories
            builder.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>().SingleInstance();

            //Register HttpCache and HttpSession
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.Register(
                x => {
                    ActionContext actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                    var factory = x.Resolve<IUrlHelperFactory>();
                    return factory.GetUrlHelper(actionContext);
                });

            //Register WebTracker
            builder.RegisterType<GoogleAnalyticsTracker>()
                .As<IWebTracker>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(IWebTracker)))
                .WithParameter("trackingId", _globalOptions.GoogleAnalyticsAccountId);

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();

            // Initialise AutoMapper
            MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                // register all out mapper profiles (classes/mappers/*)
                config.AddMaps(typeof(Program));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            // only during development, validate your mappings; remove it before release
            if (_globalOptions.IsDevelopment() || _globalOptions.IsLocal())
                mapperConfig.AssertConfigurationIsValid();

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();


        }
    }
}
