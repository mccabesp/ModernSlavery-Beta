using System;
using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.ViewServices;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Registration.Classes;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Shared.Services;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Viewing.Presenters;

namespace ModernSlavery.WebUI
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule(SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions,
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions)
        {
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }

        #region Interface properties

        public bool AutoSetup { get; } = false;

        #endregion

        public void Register(IDependencyBuilder builder)
        {
            //Allow handler for caching of http responses
            builder.ServiceCollection.AddResponseCaching();

            //Allow creation of a static http context anywhere
            builder.ServiceCollection.AddHttpContextAccessor();

            builder.ServiceCollection.AddControllersWithViews(
                    options =>
                    {
                        options.AddStringTrimmingProvider(); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
                        options.ModelMetadataDetailsProviders.Add(
                            new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                        _responseCachingOptions.CacheProfiles.ForEach(p =>
                            options.CacheProfiles.Add(p)); //Load the response cache profiles from options
                        options.Filters.Add<ErrorHandlingFilter>();
                    })
                .AddControllersAsServices() // Add controllers as services so attribute filters be resolved in contructors.
                // Set the default resolver to use Pascalcase instead of the default camelCase which may break Ajaz responses
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                })
                .AddDataAnnotationsLocalization(
                    options =>
                    {
                        options.DataAnnotationLocalizerProvider =
                            DataAnnotationLocalizerProvider.DefaultResourceHandler;
                    });

            var mvcBuilder = builder.ServiceCollection.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal()) mvcBuilder.AddRazorRuntimeCompilation();


            //Add antiforgery token by default to forms
            builder.ServiceCollection.AddAntiforgery();

            //Add services needed for sessions
            builder.ServiceCollection.AddSession(
                o =>
                {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy =
                        CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = false; //Always use https cookies
                    o.Cookie.SameSite = SameSiteMode.Strict;
                    o.Cookie.Domain =
                        _sharedOptions.ExternalHost
                            .BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    o.IdleTimeout =
                        TimeSpan.FromMinutes(_sharedOptions
                            .SessionTimeOutMinutes); //Equivalent to <sessionState timeout="20"> from old Web.config
                });

            //Add the distributed cache and data protection
            builder.ServiceCollection.AddDistributedCache(_distributedCacheOptions)
                .AddDataProtection(_dataProtectionOptions);

            //Add app insights tracking
            builder.ServiceCollection.AddApplicationInsightsTelemetry(_sharedOptions.AppInsights_InstrumentationKey);

            //This may now be required 
            builder.ServiceCollection.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Register StaticAssetsVersioningHelper
            builder.ServiceCollection.AddSingleton<StaticAssetsVersioningHelper>();

            //Configure the services required for authentication by IdentityServer
            builder.ServiceCollection.AddIdentityServerClient(
                _sharedOptions.IdentityIssuer,
                _sharedOptions.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                _sharedOptions.AuthSecret,
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(builder.ServiceCollection);

            //Register the database dependencies
            builder.RegisterModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.RegisterModule<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            builder.RegisterModule<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterModule<Infrastructure.Logging.DependencyModule>();

            //Register the search dependencies
            builder.RegisterModule<Infrastructure.Search.DependencyModule>();

            //Register Email queuers
            builder.ContainerBuilder.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance();
            builder.ContainerBuilder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();

            builder.ContainerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            //Register the messaging dependencies
            builder.RegisterModule<Infrastructure.Messaging.DependencyModule>();


            //Register the companies house dependencies
            builder.RegisterModule<Infrastructure.CompaniesHouse.DependencyModule>();

            //Register public and private repositories
            builder.ContainerBuilder.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();

            builder.ContainerBuilder.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();

            //Register business logic and services
            // BL Services
            builder.ContainerBuilder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();

            builder.ContainerBuilder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.ContainerBuilder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .SingleInstance();
            builder.ContainerBuilder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.ContainerBuilder.RegisterType<UpdateFromCompaniesHouseService>()
                .As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            builder.ContainerBuilder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>()
                .SingleInstance();
            builder.ContainerBuilder.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.ContainerBuilder.RegisterType<RegistrationService>().As<IRegistrationService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // register web ui services
            builder.ContainerBuilder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<ViewingPresenter>().As<IViewingPresenter>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<ComparePresenter>().As<IComparePresenter>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<AdminSearchService>().As<AdminSearchService>()
                .InstancePerLifetimeScope();
            builder.ContainerBuilder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();

            //Register some singletons
            builder.ContainerBuilder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);
            builder.ContainerBuilder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            //Register factories
            builder.ContainerBuilder.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();


            // Register Action helpers
            builder.ContainerBuilder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>()
                .SingleInstance();
            builder.ContainerBuilder.Register(
                x =>
                {
                    var actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                    var factory = x.Resolve<IUrlHelperFactory>();
                    return factory.GetUrlHelper(actionContext);
                });

            //Register google analytics tracker
            builder.RegisterModule<GoogleAnalyticsDependencyModule>();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.ContainerBuilder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();

            // Initialise AutoMapper
            var mapperConfig = new MapperConfiguration(config =>
            {
                // register all out mapper profiles (classes/mappers/*)
                config.AddMaps(typeof(Program));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            // only during development, validate your mappings; remove it before release
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal())
                mapperConfig.AssertConfigurationIsValid();

            builder.ContainerBuilder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder.ContainerBuilder);
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }

        #region Dependencies

        private readonly SharedOptions _sharedOptions;
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;

        #endregion

        #region Static properties

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;
        public static HttpMessageHandler BackChannelHandler { get; set; }

        #endregion
    }
}