using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.WebUI.Helpers;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Storage;

namespace ModernSlavery.WebUI
{
    public class Startup:IStartup
    {

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;

        private readonly IConfiguration _Config;
        private readonly ILogger _Logger;
        private IServiceProvider _ServiceProvider;
        private OptionsBinder OptionsBinder;
        public Startup(IConfiguration config)
        {
            _Config = config;
            _Logger = Activator.CreateInstance<Logger<Startup>>();
        }

        public static HttpMessageHandler BackChannelHandler { get; set; }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Bind the options classes and register as services
            OptionsBinder = new OptionsBinder(services, _Config);
            OptionsBinder.BindAssemblies("ModernSlavery");
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            var coHoOptions = OptionsBinder.Get<CompaniesHouseOptions>();
            var responseCachingOptions = OptionsBinder.Get<ResponseCachingOptions>();
            #endregion

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(globalOptions.DateTimeOffset);

                        //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(globalOptions.DefaultEncryptionKey);

            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            services.AddHttpClient<IWebTracker, GoogleAnalyticsTracker>(nameof(IWebTracker), GoogleAnalyticsTracker.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GoogleAnalyticsTracker.GetRetryPolicy());

            //Add a dedicated httpclient for Companies house API with exponential retry policy
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
                        responseCachingOptions.CacheProfiles.ForEach(p=> options.CacheProfiles.Add(p));//Load the response cache profiles from options
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
            if (_Config["Environment"].EqualsI("Development","Local"))mvcBuilder.AddRazorRuntimeCompilation();


            //Add antiforgery token by default to forms
            services.AddAntiforgery();

            //Add services needed for sessions
            services.AddSession(
                o => {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = false; //Always use https cookies
                    o.Cookie.SameSite = SameSiteMode.Strict;
                    o.Cookie.Domain = globalOptions.EXTERNAL_HOST.BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    o.IdleTimeout =
                        TimeSpan.FromMinutes(_Config.GetValue("SessionTimeOutMinutes", 20)); //Equivalent to <sessionState timeout="20"> from old Web.config
                });

            //Add the distributed cache and data protection
            services.AddDistributedCache(_Config).AddDataProtection(_Config);

            services.AddApplicationInsightsTelemetry(_Config.GetValue("ApplicationInsights:InstrumentationKey", _Config["APPINSIGHTS-INSTRUMENTATIONKEY"]));

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Register StaticAssetsVersioningHelper
            services.AddSingleton<StaticAssetsVersioningHelper>();

            //Configure the services required for authentication by IdentityServer
            string authority = _Config["Environment"].EqualsI("Local") ? _Config["IDENTITY_ISSUER"] : $"{globalOptions.SiteAuthority}account/";
            services.AddIdentityServerClient(
                authority,
                globalOptions.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                _Config.GetValue("AuthSecret", "secret"),
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Register the external dependencies
            var dependencyBuilder = new DependencyBuilder(services);

            //Override any test services
            ConfigureTestContainer?.Invoke(dependencyBuilder.Builder);

            //Register the web host dependencies
            dependencyBuilder.Bind<WebHostDependencyModule>();

            return dependencyBuilder.Build();
        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(IApplicationBuilder app)
        {
            var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            var responseCachingOptions = OptionsBinder.Get<ResponseCachingOptions>();

            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (globalOptions.UseDeveloperExceptions)
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
                        if (responseCachingOptions.StaticCacheSeconds > 0)ctx.Context.SetResponseCache(responseCachingOptions.StaticCacheSeconds);
                    }
                }); //For the wwwroot folder

            // Include un-bundled js + css folders to serve the source files in dev environment
            if (_Config["Environment"].EqualsI("Local"))
            {
                app.UseStaticFiles(
                    new StaticFileOptions {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                        RequestPath = "",
                        OnPrepareResponse = ctx => {
                            //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                            if (responseCachingOptions.StaticCacheSeconds > 0)
                            {
                                ctx.Context.SetResponseCache(responseCachingOptions.StaticCacheSeconds);
                            }
                        }
                    });
            }

            app.UseRouting();
            app.UseResponseCaching();
            app.UseResponseBuffering(); //required otherwise JsonResult uses chunking and adds extra characters
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseMiddleware<MaintenancePageMiddleware>(globalOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(globalOptions.StickySessions); //Enable/Disable sticky sessions based on  

            //Force basic authentication
            if (_Config.GetValue("BasicAuthentication:Enabled",false))
                app.UseMiddleware<BasicAuthenticationMiddleware>(_Config.GetValue("BasicAuthentication:Username", _Config["BasicAuthentication:Password"])); 

            app.UseMiddleware<SecurityHeaderMiddleware>(); //Add/remove security headers from all responses

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
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.ShortCodes, globalOptions.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicCodes, globalOptions.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicSections, globalOptions.DataPath)
                    );


                    _Logger.LogInformation("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () => {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _Logger.LogInformation("Application Stopping");
                });
        }

    }
}
