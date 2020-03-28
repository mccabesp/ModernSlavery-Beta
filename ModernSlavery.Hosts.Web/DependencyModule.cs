using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.BusinessDomain.Admin;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Hosts.Web;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Account.Interfaces;
using ModernSlavery.WebUI.Account.ViewServices;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Registration.Classes;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Shared.Services;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Viewing.Presenters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Logging;
using AuditLogger = ModernSlavery.Infrastructure.Logging.AuditLogger;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.WebUI
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly CompaniesHouseOptions _coHoOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly BasicAuthenticationOptions _basicAuthenticationOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions, CompaniesHouseOptions coHoOptions,
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions, BasicAuthenticationOptions basicAuthenticationOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _coHoOptions = coHoOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
            _basicAuthenticationOptions = basicAuthenticationOptions;
        }

        #region Interface properties

        public bool AutoSetup { get; } = false;

        #endregion

        #region Static properties

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;
        public static HttpMessageHandler BackChannelHandler { get; set; }

        #endregion

        public void Register(IDependencyBuilder builder)
        {
            //Allow handler for caching of http responses
            builder.Services.AddResponseCaching();

            //Allow creation of a static http context anywhere
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews(
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

            var mvcBuilder = builder.Services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal()) mvcBuilder.AddRazorRuntimeCompilation();


            //Add antiforgery token by default to forms
            builder.Services.AddAntiforgery();

            //Add services needed for sessions
            builder.Services.AddSession(
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
            builder.Services.AddDistributedCache(_distributedCacheOptions)
                .AddDataProtection(_dataProtectionOptions);

            //Add app insights tracking
            builder.Services.AddApplicationInsightsTelemetry(_sharedOptions.AppInsights_InstrumentationKey);

            //This may now be required 
            builder.Services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Register StaticAssetsVersioningHelper
            builder.Services.AddSingleton<StaticAssetsVersioningHelper>();

            //Configure the services required for authentication by IdentityServer
            builder.Services.AddIdentityServerClient(
                _sharedOptions.IdentityIssuer,
                _sharedOptions.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                _sharedOptions.AuthSecret,
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(builder.Services);

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
            builder.Autofac.RegisterType<SendEmailService>().As<ISendEmailService>().SingleInstance();
            builder.Autofac.RegisterType<NotificationService>().As<INotificationService>().SingleInstance();

            builder.Autofac.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            //Register the messaging dependencies
            builder.RegisterModule<Infrastructure.Messaging.DependencyModule>();


            //Register the companies house dependencies
            builder.RegisterModule<Infrastructure.CompaniesHouse.DependencyModule>();

            //Register public and private repositories
            builder.Autofac.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();

            //Register business logic and services
            // BL Services
            builder.Autofac.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();

            builder.Autofac.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .SingleInstance();
            builder.Autofac.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.Autofac.RegisterType<UpdateFromCompaniesHouseService>()
                .As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            builder.Autofac.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>()
                .SingleInstance();
            builder.Autofac.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<RegistrationService>().As<IRegistrationService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // register web ui services
            builder.Autofac.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ViewingPresenter>().As<IViewingPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ComparePresenter>().As<IComparePresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
            builder.Autofac.RegisterType<AdminSearchService>().As<AdminSearchService>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<AuditLogger>().As<IAuditLogger>().InstancePerLifetimeScope();

            //Register some singletons
            builder.Autofac.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);
            builder.Autofac.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            //Register factories
            builder.Autofac.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();


            // Register Action helpers
            builder.Autofac.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>()
                .SingleInstance();
            builder.Autofac.Register(
                x =>
                {
                    var actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                    var factory = x.Resolve<IUrlHelperFactory>();
                    return factory.GetUrlHelper(actionContext);
                });

            //Register google analytics tracker
            builder.RegisterModule<GoogleAnalyticsDependencyModule>();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.Autofac.RegisterAssemblyTypes(typeof(BaseController).Assembly)
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

            builder.Autofac.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder.Autofac);
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //Add configuration here
            var app = serviceProvider.GetService<IApplicationBuilder>();

            var lifetime = serviceProvider.GetService<IHostApplicationLifetime>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var _fileRepository = app.ApplicationServices.GetService<IFileRepository>();

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(_sharedOptions.DateTimeOffset);

            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(_sharedOptions.DefaultEncryptionKey);

            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (_sharedOptions.UseDeveloperExceptions)
            {
                IdentityModelEventSource.ShowPII = true;

                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            app.UseHttpsRedirection();
            //app.UseResponseCompression(); //Disabled to use IIS compression which has better performance (see https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.1)
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
                        //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                        if (_responseCachingOptions.StaticCacheSeconds > 0)
                            ctx.Context.SetResponseCache(_responseCachingOptions.StaticCacheSeconds);
                    }
                }); //For the wwwroot folder

            // Include un-bundled js + css folders to serve the source files in dev environment
            if (_sharedOptions.IsLocal())
                app.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                        RequestPath = "",
                        OnPrepareResponse = ctx =>
                        {
                            //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                            if (_responseCachingOptions.StaticCacheSeconds > 0)
                                ctx.Context.SetResponseCache(_responseCachingOptions.StaticCacheSeconds);
                        }
                    });

            app.UseRouting();
            app.UseResponseCaching();
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseMiddleware<MaintenancePageMiddleware>(_sharedOptions
                .MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(_sharedOptions
                .StickySessions); //Enable/Disable sticky sessions based on  

            
            //Force basic authentication
            if (_basicAuthenticationOptions.Enabled)app.UseMiddleware<BasicAuthenticationMiddleware>(_basicAuthenticationOptions.Username, _basicAuthenticationOptions.Password);

            app.UseMiddleware<SecurityHeaderMiddleware>(); //Add/remove security headers from all responses

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            //Initialise the application
            //Ensure ShortCodes, SicCodes and SicSections exist on remote 
            Task.WaitAll(
                _fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.DataPath),
                _fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.DataPath),
                _fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.DataPath)
            );

            lifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _logger.LogInformation("Application Stopping");
                });
        }
    }
}