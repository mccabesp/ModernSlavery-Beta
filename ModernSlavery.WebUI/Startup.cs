using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Repositories;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.WebUI.Areas.Account.Abstractions;
using ModernSlavery.WebUI.Areas.Account.ViewServices;
using ModernSlavery.WebUI.Classes;
using ModernSlavery.WebUI.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.BusinessLogic.Admin;
using HttpSession = ModernSlavery.Extensions.AspNetCore.HttpSession;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.BusinessLogic.Classes;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Infrastructure.File;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Message;
using ModernSlavery.Infrastructure.Queue;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI
{
    public class Startup
    {

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;

        private readonly IConfiguration _Config;
        private readonly IWebHostEnvironment _Env;
        private readonly ILogger _Logger;

        public Startup(IWebHostEnvironment env, IConfiguration config, ILogger<Startup> logger)
        {
            this._Env = env;
            this._Config = config;
            this._Logger = logger;
        }

        public static HttpMessageHandler BackChannelHandler { get; set; }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add services to the collection. Don't build or return 
            // any IServiceProvider or the ConfigureContainer method
            // won't get called.

            // setup configuration
            services.ConfigureOptions<GlobalOptions>(_Config);

            services.Configure<ViewingOptions>(_Config.GetSection("Gpg:Viewing"));
            services.Configure<SubmissionOptions>(_Config.GetSection("Gpg:Submission"));

            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            services.AddHttpClient<IWebTracker, GoogleAnalyticsTracker>(nameof(IWebTracker), GoogleAnalyticsTracker.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GoogleAnalyticsTracker.GetRetryPolicy());

            //Add a dedicated httpclient for Companies house API with exponential retry policy
            var coHoOptions = new CompaniesHouseOptions();
            Config.Configuration.Bind("CompaniesHouse", coHoOptions);

            //Add a dedicated httpClient for Companies house API with exponential retry policy

            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), (httpClient) =>
                {
                    CompaniesHouseAPI.SetupHttpClient(httpClient, coHoOptions.ApiServer, coHoOptions.ApiKey);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            //Allow creation of a static http context anywhere
            services.AddHttpContextAccessor();

            services.AddControllersWithViews(
                    options => {
                        options.AddStringTrimmingProvider(); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
                        options.ModelMetadataDetailsProviders.Add(
                            new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                        options.AddCacheProfiles(); //Load the response cache profiles from appsettings file
                        options.Filters.Add<ErrorHandlingFilter>();
                    })
                .AddControllersAsServices() // Add controllers as services so attribute filters be resolved in contructors.
                // Set the default resolver to use Pascalcase instead of the default camelCase which may break Ajaz responses
                .AddJsonOptions(options => 
                { 
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive=true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; 
                })
                .AddDataAnnotationsLocalization(
                    options => { options.DataAnnotationLocalizerProvider = DataAnnotationLocalizerProvider.DefaultResourceHandler; });

            var mvcBuilder=services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (Config.IsDevelopment() || Config.IsLocal())mvcBuilder.AddRazorRuntimeCompilation();


            //Add antiforgery token by default to forms
            services.AddAntiforgery();

            //Add services needed for sessions
            services.AddSession(
                o => {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = false; //Always use https cookies
                    o.Cookie.SameSite = SameSiteMode.Strict;
                    o.Cookie.Domain = Global.ExternalHost.BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    o.IdleTimeout =
                        TimeSpan.FromMinutes(_Config.GetValue("SessionTimeOutMinutes", 20)); //Equivalent to <sessionState timeout="20"> from old Web.config
                });

            //Add the distributed cache and data protection
            services.AddDistributedCache(_Config).AddDataProtection(_Config);

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Configure the services required for authentication by IdentityServer
            string authority = Config.IsLocal() ? Config.GetAppSetting("IDENTITY_ISSUER") : $"{Config.SiteAuthority}account/";
            services.AddIdentityServerClient(
                authority,
                Config.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                Config.GetAppSetting("AuthSecret", "secret"),
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Create Inversion of Control container
            var containerIoC = BuildContainerIoC(services);

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(containerIoC);
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public IContainer BuildContainerIoC(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            // Note that Populate is basically a foreach to add things
            // into Autofac that are in the collection. If you register
            // things in Autofac BEFORE Populate then the stuff in the
            // ServiceCollection can override those things; if you register
            // AFTER Populate those registrations can override things
            // in the ServiceCollection. Mix and match as needed.
            builder.Populate(services);

            //Register the configuration
            builder.RegisterInstance(Config.Configuration).SingleInstance();

            builder.Register(c => new SqlRepository(new DatabaseContext(Global.DatabaseConnectionString, true)))
                .As<IDataRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();
            builder.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            // use the 'azureStorageConnectionString' and 'AzureStorageShareName' when connecting to a remote storage
            string azureStorageConnectionString = Config.GetConnectionString("AzureStorage");
            Console.WriteLine($"AzureStorageConnectionString: {azureStorageConnectionString}");

            // validate we have a storage connection
            if (string.IsNullOrWhiteSpace(azureStorageConnectionString))
            {
                throw new InvalidOperationException("No Azure Storage connection specified. Check the config.");
            }

            string azureStorageShareName = Config.GetAppSetting("AzureStorageShareName");
            // use the 'localStorageRoot' when hosting the storage in a local folder
            string localStorageRoot = Config.GetAppSetting("LocalStorageRoot");

            if (string.IsNullOrWhiteSpace(localStorageRoot))
            {
                builder.Register(
                        c => new AzureFileRepository(
                            azureStorageConnectionString,
                            azureStorageShareName,
                            new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>().SingleInstance();
            }

            // Register queues
            builder.RegisterAzureQueue(azureStorageConnectionString, QueueNames.SendEmail);
            builder.RegisterAzureQueue(azureStorageConnectionString, QueueNames.SendNotifyEmail);
            builder.RegisterAzureQueue(azureStorageConnectionString, QueueNames.ExecuteWebJob);

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register record loggers
            builder.RegisterLogRecord(Filenames.BadSicLog);
            builder.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.RegisterLogRecord(Filenames.RegistrationLog);
            builder.RegisterLogRecord(Filenames.SubmissionLog);
            builder.RegisterLogRecord(Filenames.SearchLog);

            // Register log records (without key filtering)
            builder.RegisterType<UserLogRecord>().As<IUserLogRecord>().SingleInstance();
            builder.RegisterType<RegistrationLogRecord>().As<IRegistrationLogRecord>().SingleInstance();

            // Setup azure search
            string azureSearchServiceName = Config.GetAppSetting("SearchService:ServiceName");
            string azureSearchAdminKey = Config.GetAppSetting("SearchService:AdminApiKey");
            var azureSearchDisabled = Config.GetAppSetting("SearchService:Disabled").ToBoolean();

            builder.Register(c => new SearchServiceClient(azureSearchServiceName, new SearchCredentials(azureSearchAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", azureSearchServiceName)
                .WithParameter("adminApiKey", azureSearchAdminKey)
                .WithParameter("disabled", azureSearchDisabled);

            builder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("disabled", azureSearchDisabled);

            builder.RegisterInstance(Config.Configuration).SingleInstance();

            // BL Services
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().SingleInstance();

            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RegistrationRepository>().As<IRegistrationRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();

            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>().SingleInstance();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().SingleInstance();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            // register web ui services
            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().SingleInstance();
            builder.RegisterType<DownloadableFileBusinessLogic>().As<IDownloadableFileBusinessLogic>().InstancePerLifetimeScope();

            builder.RegisterType<ChangeDetailsViewService>().As<IChangeDetailsViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangeEmailViewService>().As<IChangeEmailViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<ICloseAccountViewService>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingPresenter>().As<IViewingPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();
            builder.RegisterType<SearchPresenter>().As<ISearchPresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ComparePresenter>().As<IComparePresenter>().InstancePerLifetimeScope();
            builder.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
            builder.RegisterType<AdminSearchService>().As<AdminSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance();
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();
            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();


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
                .WithParameter("trackingId", Config.GetAppSetting("GoogleAnalyticsAccountId"));

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();

            //TODO: Implement AutoFac modules
            //builder.RegisterModule(new AutofacModule());

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

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
            if (Config.IsLocal() || Config.IsDevelopment())
                mapperConfig.AssertConfigurationIsValid();

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            IContainer container = builder.Build();

            return container;
        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (Global.UseDeveloperExceptions)
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
                new StaticFileOptions {
                    OnPrepareResponse = ctx => {
                        //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                        if (Global.StaticCacheSeconds > 0)
                        {
                            ctx.Context.SetResponseCache(Global.StaticCacheSeconds);
                        }
                    }
                }); //For the wwwroot folder

            // Include un-bundled js + css folders to serve the source files in dev environment
            if (Config.IsLocal())
            {
                app.UseStaticFiles(
                    new StaticFileOptions {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                        RequestPath = "",
                        OnPrepareResponse = ctx => {
                            //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                            if (Global.StaticCacheSeconds > 0)
                            {
                                ctx.Context.SetResponseCache(Global.StaticCacheSeconds);
                            }
                        }
                    });
            }

            app.UseRouting();
            app.UseResponseCaching();
            app.UseResponseBuffering(); //required otherwise JsonResult uses chunking and adds extra characters
            app.UseStaticHttpContext(); //Temporary fix for old static HttpContext 
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseMaintenancePageMiddleware(Global.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseStickySessionMiddleware(Global.StickySessions); //Enable/Disable sticky sessions based on  

            //Force basic authentication
            if (Config.GetAppSetting<bool>("BasicAuthentication:Enabled"))
                app.UseMiddleware<BasicAuthenticationMiddleware>(Config.GetAppSetting("BasicAuthentication:Username"), Config.GetAppSetting("BasicAuthentication:Password")); 

            app.UseSecurityHeaderMiddleware(); //Add/remove security headers from all responses

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            lifetime.ApplicationStarted.Register(
                () => {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.

                    //Initialise the application
                    //Ensure ShortCodes, SicCodes and SicSections exist on remote 
                    var _fileRepository = app.ApplicationServices.GetService<IFileRepository>();
                    Task.WaitAll(
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.ShortCodes, Global.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicCodes, Global.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicSections, Global.DataPath)
                    );


                    logger.LogInformation("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () => {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    logger.LogInformation("Application Stopping");
                });
        }

    }
}
