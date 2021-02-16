using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Caching;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.Infrastructure.Telemetry.AppInsights;
using ModernSlavery.WebAPI.Public.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.Hosts.Web
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;

        private readonly SharedOptions _sharedOptions;
        private readonly TestOptions _testOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly BasicAuthenticationOptions _basicAuthenticationOptions;
        private readonly DynamicRoutesOptions _dynamicRoutesOptions;
        private readonly IdentityClientOptions _identityClientOptions;
        private readonly FeatureSwitchOptions _featureSwitchOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions,
            TestOptions testOptions,
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions, BasicAuthenticationOptions basicAuthenticationOptions,
            DynamicRoutesOptions dynamicRoutesOptions,
            IdentityClientOptions identityClientOptions,
            FeatureSwitchOptions featureSwitchOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _testOptions = testOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
            _basicAuthenticationOptions = basicAuthenticationOptions;
            _dynamicRoutesOptions = dynamicRoutesOptions;
            _identityClientOptions = identityClientOptions;
            _featureSwitchOptions = featureSwitchOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Allow creation of a static http context anywhere
            services.AddHttpContextAccessor();

            var mvcBuilder = services.AddControllersWithViews(options =>
                {
                    options.OutputFormatters.Add(new StatementSummaryCsvFormatter());
                    options.OutputFormatters.Add(new StatementSummaryXmlFormatter());
                    options.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValue.Parse("text/csv"));
                    options.RespectBrowserAcceptHeader = true; // false by default - Any 'Accept' header gets turned into application/json. If you want to allow the clients to accept different headers, you need to switch that translation off
                    options.AddSecureSimpleModelBinderProvider();//Note required yet as encryption/decryption not used yet
                    options.AddViewModelBinderProvider();
                    options.ModelMetadataDetailsProviders.Add(new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                    if (_responseCachingOptions.Enabled)_responseCachingOptions.CacheProfiles.ForEach(p =>options.CacheProfiles.Add(p)); //Load the response cache profiles from options
                    options.Filters.Add<XssValidationFilter>(); //Checks for Xss
                    options.Filters.Add<ViewModelResultFilter>(); 

                }).AddXmlSerializerFormatters().AddXmlDataContractSerializerFormatters();

            mvcBuilder.AddApplicationPart<WebAPI.Public.DependencyModule>();
            if (_featureSwitchOptions.IsEnabled("DevOps"))mvcBuilder.AddApplicationPart<WebUI.DevOps.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Identity.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Account.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Admin.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Registration.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Submission.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Viewing.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Shared.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.GDSDesignSystem.DependencyModule>();

            //Add the header forwarding when behind gateway/firewall
            if (!string.IsNullOrWhiteSpace(_sharedOptions.GatewayHosts))services.AddForwardedHeaders(_sharedOptions.GatewayHosts.SplitI());

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Development" for Development dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            // However this doesnt work on razor class/component libraries so we instead use this workaround 
            if (Debugger.IsAttached && _sharedOptions.IsDevelopment()) mvcBuilder.AddApplicationPartsRuntimeCompilation();

            //Log all the application parts when in development
            if (_sharedOptions.IsDevelopment())services.AddHostedService<ApplicationPartsLogger>();

            // Add controllers, taghelpers, views as services so attribute dependencies can be resolved in their contructors
            mvcBuilder.AddControllersAsServices();
            mvcBuilder.AddTagHelpersAsServices();
            mvcBuilder.AddViewComponentsAsServices();

            // Set the default resolver to use Pascalcase instead of the default camelCase which may break Ajaz responses
            mvcBuilder.AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            mvcBuilder.AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider =
                        DataAnnotationLocalizerProvider.DefaultResourceHandler;
                });

            //Add antiforgery token by default to forms
            services.AddAntiforgery();

            services.AddRazorPages();

            //Add services needed for sessions
            services.AddSession(options =>
                {
                    options.Cookie.IsEssential = true; //This is required otherwise session will not load
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    options.Cookie.HttpOnly = false; //Always use https cookies
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                    options.Cookie.Domain = _sharedOptions.EXTERNAL_HOSTNAME.BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    options.IdleTimeout = TimeSpan.FromMinutes(_sharedOptions.SessionTimeOutMinutes); //Equivalent to <sessionState timeout="20"> from old Web.config

                });

            //Add the distributed cache
            services.AddDistributedCache(_distributedCacheOptions);

            //Add data protection
            services.AddDataProtection(_dataProtectionOptions);

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            // configure the application cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                options.Cookie.Expiration= TimeSpan.FromMinutes(_sharedOptions.SessionTimeOutMinutes);
                
                options.SlidingExpiration = true;
                options.ExpireTimeSpan= TimeSpan.FromMinutes(_sharedOptions.SessionTimeOutMinutes);
            });

            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
            });

            #region Configure Identity Client
            //Configure the services required for authentication by IdentityServer
            services.AddIdentityServerClient(
                _sharedOptions,
                _identityClientOptions.IssuerUri,
                _identityClientOptions.ClientId,
                _identityClientOptions.ClientSecret,
                _identityClientOptions.SignOutUri,
                _identityClientOptions.AllowInvalidServerCertificates);
            #endregion

            //Register the AutoMapper configurations in all domain assemblies
            services.AddAutoMapper(_sharedOptions.IsDevelopment());

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register the user audit log repository
            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>()
                .SingleInstance();

            builder.Register(x =>
            {
                var actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                var factory = x.Resolve<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IApplicationBuilder>();
            var hostApplicationLifetime = lifetimeScope.Resolve<IHostApplicationLifetime>();

            //User the header forwarding when behind gateway/firewall
            if (!string.IsNullOrWhiteSpace(_sharedOptions.GatewayHosts))app.UseForwardedHeaders();

            //Add header debugging
            if (_sharedOptions.DebugHeaders) app.UseMiddleware<DebugHeadersMiddleware>();

            app.UseMiddleware<ExceptionMiddleware>();

            if (_sharedOptions.UseDeveloperExceptions)
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }


            //app.UseHttpsRedirection(); This always causes redirect to https://localhost from http://localhost:5000
            //app.UseResponseCompression(); //Disabled to use IIS compression which has better performance (see https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.1)

            app.UseRouting();
            if (_responseCachingOptions.Enabled)app.UseResponseCaching();

            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy =  Microsoft.AspNetCore.Http.SameSiteMode.Strict, //Change to default SameSiteMode.Lax when using cross-origin OAuth2 authentication
                Secure = CookieSecurePolicy.Always,                 
            });

            app.UseMiddleware<MaintenancePageMiddleware>(_sharedOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(_testOptions.StickySessions); //Enable/Disable sticky sessions based on  
            app.UseMiddleware<SecurityHeaderMiddleware>(); //Add/remove security headers from all responses

            //Force basic authentication
            if (_basicAuthenticationOptions.Enabled) app.UseMiddleware<BasicAuthenticationMiddleware>(_basicAuthenticationOptions.Username, _basicAuthenticationOptions.Password);

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints => {
                _dynamicRoutesOptions.MapDynamicRoutes(endpoints);
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            hostApplicationLifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Web Application Started");
                    app.ServerFeatures.LogHostAddresses(_logger);
                });
            hostApplicationLifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    _logger.LogInformation("Web Application Stopping");

                });
        }

        public void RegisterModules(IList<Type> modules)
        {
            modules.AddDependency<WebAPI.Public.DependencyModule>();
            if (_featureSwitchOptions.IsEnabled("DevOps")) modules.AddDependency<WebUI.DevOps.DependencyModule>();
            modules.AddDependency<WebUI.Identity.DependencyModule>();
            modules.AddDependency<WebUI.StaticFiles.DependencyModule>();
            modules.AddDependency<WebUI.Account.DependencyModule>();
            modules.AddDependency<WebUI.Admin.DependencyModule>();
            modules.AddDependency<WebUI.Registration.DependencyModule>();
            modules.AddDependency<WebUI.Submission.DependencyModule>();
            modules.AddDependency<WebUI.Viewing.DependencyModule>();

            //Register the file storage dependencies
            modules.AddDependency<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            modules.AddDependency<QueueStorageDependencyModule>();

            //Register the log storage dependencies
            modules.AddDependency<Infrastructure.Logging.DependencyModule>();

            //Register the search dependencies
            modules.AddDependency<Infrastructure.Search.DependencyModule>();

            //Register google analytics tracker
            modules.AddDependency<GoogleAnalyticsDependencyModule>();

            //Register the app insights dependencies
            modules.AddDependency<WebAppInsightsDependencyModule>();

            //Register the Companies House dependencies
            modules.AddDependency<Infrastructure.CompaniesHouse.DependencyModule>();

            //Register the Gov Notify dependencies
            modules.AddDependency<Infrastructure.Messaging.GovNotify.DependencyModule>();
        }
    }
}