using System;
using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Admin;
using ModernSlavery.BusinessLogic.Classes;
using ModernSlavery.BusinessLogic.Registration;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewServices;
using ModernSlavery.WebUI.Classes;
using ModernSlavery.WebUI.Classes.Extensions;
using ModernSlavery.WebUI.Helpers;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.WebUI.Registration.Classes;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Shared.Services;

namespace ModernSlavery.WebUI
{
    public class AppDependencyModule: IDependencyModule
    {
        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;
        public static HttpMessageHandler BackChannelHandler { get; set; }

        private readonly SharedOptions _sharedOptions;
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;

        public AppDependencyModule(SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions, ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions, DataProtectionOptions dataProtectionOptions)
        {
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
        }

        public void Bind(ContainerBuilder builder, IServiceCollection services)
        {
            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Allow creation of a static http context anywhere
            services.AddHttpContextAccessor();

            services.AddControllersWithViews(
                    options => {
                        options.AddStringTrimmingProvider(); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
                        options.ModelMetadataDetailsProviders.Add(
                            new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                        _responseCachingOptions.CacheProfiles.ForEach(p => options.CacheProfiles.Add(p));//Load the response cache profiles from options
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
                    options => { options.DataAnnotationLocalizerProvider = DataAnnotationLocalizerProvider.DefaultResourceHandler; });

            var mvcBuilder = services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal()) mvcBuilder.AddRazorRuntimeCompilation();


            //Add antiforgery token by default to forms
            services.AddAntiforgery();

            //Add services needed for sessions
            services.AddSession(
                o => {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = false; //Always use https cookies
                    o.Cookie.SameSite = SameSiteMode.Strict;
                    o.Cookie.Domain = _sharedOptions.EXTERNAL_HOST.BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    o.IdleTimeout =
                        TimeSpan.FromMinutes(_sharedOptions.SessionTimeOutMinutes); //Equivalent to <sessionState timeout="20"> from old Web.config
                });

            //Add the distributed cache and data protection
            services.AddDistributedCache(_distributedCacheOptions).AddDataProtection(_dataProtectionOptions);

            //Add app insights tracking
            services.AddApplicationInsightsTelemetry(_sharedOptions.APPINSIGHTS_INSTRUMENTATIONKEY);

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Register StaticAssetsVersioningHelper
            services.AddSingleton<StaticAssetsVersioningHelper>();

            //Configure the services required for authentication by IdentityServer
            services.AddIdentityServerClient(
                $"{_sharedOptions.SiteAuthority}account/",
                _sharedOptions.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                _sharedOptions.AuthSecret,
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Register the database dependencies
            builder.RegisterDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.RegisterDependencyModule<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            builder.RegisterDependencyModule<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterDependencyModule<LoggingDependencyModule>();

            //Register the search dependencies
            builder.RegisterDependencyModule<SearchDependencyModule>();

            //Register Email queuers
            builder.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance();
            builder.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();

            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            //Register the messaging dependencies
            builder.RegisterDependencyModule<MessagingDependencyModule>();


            //Register the companies house dependencies
            builder.RegisterDependencyModule<CompaniesHouseDependencyModule>();

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
            builder.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();

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
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance().WithParameter("seed", _sharedOptions.ObfuscationSeed);
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

            //Register google analytics tracker
            builder.RegisterDependencyModule<GoogleAnalyticsDependencyModule>();

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
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal())
                mapperConfig.AssertConfigurationIsValid();

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

        }
    }
}
