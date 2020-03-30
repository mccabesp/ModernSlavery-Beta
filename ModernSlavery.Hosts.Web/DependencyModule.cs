using System;
using System.IO;
using System.Net.Http;
using Autofac;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Options;
using AuditLogger = ModernSlavery.Infrastructure.Logging.AuditLogger;

namespace ModernSlavery.Hosts.Web
{
    [AutoRegister]
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

            //Configure the services required for authentication by IdentityServer
            builder.Services.AddIdentityServerClient(
                _sharedOptions.IdentityIssuer,
                _sharedOptions.SiteAuthority,
                "ModernSlaveryServiceWebsite",
                _sharedOptions.AuthSecret,
                BackChannelHandler);

            //Override any test services
            ConfigureTestServices?.Invoke(builder.Services);

            //Register the file storage dependencies
            builder.RegisterModule<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            builder.RegisterModule<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            builder.RegisterModule<Infrastructure.Logging.DependencyModule>();

            builder.Autofac.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            // register web ui services
            builder.Autofac.RegisterType<AuditLogger>().As<IAuditLogger>().InstancePerLifetimeScope();
            
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

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IApplicationBuilder>();
            var hostApplicationLifetime = lifetimeScope.Resolve<IHostApplicationLifetime>();

            lifetimeScope.UseLogEventQueueLogger();

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

            hostApplicationLifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Application Started");
                });
            hostApplicationLifetime.ApplicationStopping.Register(
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