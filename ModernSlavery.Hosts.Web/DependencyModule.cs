using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Database.Classes;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Classes.Providers;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.Hosts.Web
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;

        private readonly SharedOptions _sharedOptions;
        private readonly ResponseCachingOptions _responseCachingOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly BasicAuthenticationOptions _basicAuthenticationOptions;
        private readonly IdentityClientOptions _identityClientOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions, 
            ResponseCachingOptions responseCachingOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions, BasicAuthenticationOptions basicAuthenticationOptions,
            IdentityClientOptions identityClientOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _responseCachingOptions = responseCachingOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
            _basicAuthenticationOptions = basicAuthenticationOptions;
            _identityClientOptions = identityClientOptions;
    }

        #region Static properties

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;
        public static HttpMessageHandler BackChannelHandler { get; set; }

        #endregion

        public void ConfigureServices(IServiceCollection services)
        {
            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Allow creation of a static http context anywhere
            services.AddHttpContextAccessor();

            var mvcBuilder = services.AddControllersWithViews(
                    options =>
                    {
                        options.AddStringTrimmingProvider(); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
                        options.ModelMetadataDetailsProviders.Add(
                            new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                        if (_responseCachingOptions.Enabled)
                            _responseCachingOptions.CacheProfiles.ForEach(p =>
                                options.CacheProfiles.Add(p)); //Load the response cache profiles from options
                        options.Filters.Add<ErrorHandlingFilter>();
                    });

            mvcBuilder.AddApplicationPart<WebUI.Account.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Admin.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Registration.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Submission.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Viewing.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Shared.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.GDSDesignSystem.DependencyModule>();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Development" for Development dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            // However this doesnt work on razor class/component libraries so we instead use this workaround 
            if (Debugger.IsAttached && _sharedOptions.IsDevelopment()) mvcBuilder.AddApplicationPartsRuntimeCompilation();

            //Log all the application parts when in development
            if (_sharedOptions.IsDevelopment())
                services.AddHostedService<ApplicationPartsLogger>();

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

            mvcBuilder.AddDataAnnotationsLocalization(
                    options =>
                    {
                        options.DataAnnotationLocalizerProvider =
                            DataAnnotationLocalizerProvider.DefaultResourceHandler;
                    });

            //Add antiforgery token by default to forms
            services.AddAntiforgery();

            services.AddRazorPages();

            //Add services needed for sessions
            services.AddSession(
                o =>
                {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy =
                        CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = false; //Always use https cookies
                    o.Cookie.SameSite = SameSiteMode.Strict;
                    o.Cookie.Domain =
                        _sharedOptions.WEBSITE_HOSTNAME
                            .BeforeFirst(":"); //Domain cannot be an authority and contain a port number
                    o.IdleTimeout =
                        TimeSpan.FromMinutes(_sharedOptions
                            .SessionTimeOutMinutes); //Equivalent to <sessionState timeout="20"> from old Web.config
                });

            //Add the distributed cache and data protection
            services.AddDistributedCache(_distributedCacheOptions)
                .AddDataProtection(_dataProtectionOptions);

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            #region Configure Identity Client
            //Configure the services required for authentication by IdentityServer
            services.AddIdentityServerClient(
                _identityClientOptions.AuthorityUri,
                _identityClientOptions.ClientId,
                _identityClientOptions.ClientSecret,
                _identityClientOptions.SignOutUri,
                BackChannelHandler);
            #endregion

            //Register the AutoMapper configurations in all domain assemblies
            services.AddAutoMapper(_sharedOptions.IsDevelopment());

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register the queue storage dependencies
            builder.RegisterType<DnBOrgsRepository>().As<IDnBOrgsRepository>().WithParameter("dataPath", _sharedOptions.DataPath).WithAttributeFiltering();

            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

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

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

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

            //app.UseHttpsRedirection(); This always causes redirect to https://localhost from http://localhost:5000
            //app.UseResponseCompression(); //Disabled to use IIS compression which has better performance (see https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.1)

            app.UseRouting();
            if (_responseCachingOptions.Enabled)app.UseResponseCaching();
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseMiddleware<MaintenancePageMiddleware>(_sharedOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(_sharedOptions.StickySessions); //Enable/Disable sticky sessions based on  
            app.UseMiddleware<SecurityHeaderMiddleware>(); //Add/remove security headers from all responses

            //Force basic authentication
            if (_basicAuthenticationOptions.Enabled) app.UseMiddleware<BasicAuthenticationMiddleware>(_basicAuthenticationOptions.Username, _basicAuthenticationOptions.Password);

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints => { 
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            hostApplicationLifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    _logger.LogInformation("Application Started");
                    app.ServerFeatures.LogHostAddresses(_logger);
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

        public void RegisterModules(IList<Type> modules)
        {
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
            modules.AddDependency<ApplicationInsightsDependencyModule>();

            //Register the Companies House dependencies
            modules.AddDependency<Infrastructure.CompaniesHouse.DependencyModule>();
        }
    }
}