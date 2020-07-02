using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using ModernSlavery.Infrastructure.Telemetry;
using ModernSlavery.WebUI.Shared.Classes.Middleware;

namespace ModernSlavery.Hosts.IdServer
{
    public class DependencyModule : IDependencyModule
    {
        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;
        
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly StorageOptions _storageOptions;
        private readonly DataProtectionOptions _dataProtectionOptions;
        private readonly DistributedCacheOptions _distributedCacheOptions; 
        private readonly ResponseCachingOptions _responseCachingOptions;

        public DependencyModule(ILogger<DependencyModule> logger, SharedOptions sharedOptions,
            StorageOptions storageOptions, DistributedCacheOptions distributedCacheOptions,
            DataProtectionOptions dataProtectionOptions, ResponseCachingOptions responseCachingOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
            _storageOptions = storageOptions;
            _distributedCacheOptions = distributedCacheOptions;
            _dataProtectionOptions = dataProtectionOptions;
            _responseCachingOptions = responseCachingOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Allow caching of http responses
            services.AddResponseCaching();

            //This is to allow access to the current http context anywhere
            services.AddHttpContextAccessor();

            var mvcBuilder = services.AddControllersWithViews();

            mvcBuilder.AddApplicationPart<WebUI.Identity.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.Shared.DependencyModule>();
            mvcBuilder.AddApplicationPart<WebUI.GDSDesignSystem.DependencyModule>();

            // Add controllers, taghelpers, views as services so attribute dependencies can be resolved in their contructors
            mvcBuilder.AddControllersAsServices();
            mvcBuilder.AddTagHelpersAsServices();
            mvcBuilder.AddViewComponentsAsServices();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Development" for Development dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            // However this doesnt work on razor class/component libraries so we instead use this workaround 
            if (Debugger.IsAttached && _sharedOptions.IsDevelopment()) mvcBuilder.AddApplicationPartsRuntimeCompilation();

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
            
            //Register the AutoMapper configurations in all domain assemblies
            services.AddAutoMapper(_sharedOptions.IsDevelopment());
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();
            builder.Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();

            //Register the user audit log repository
            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

            // Register log records (without key filtering)
            builder.RegisterType<UserAuditLogger>().As<IUserLogger>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>()
                .SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);
        }


        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            var lifetime = lifetimeScope.Resolve<IHostApplicationLifetime>();

            lifetimeScope.UseLogEventQueueLogger();

            app.UseMiddleware<ExceptionMiddleware>();
            if (Debugger.IsAttached || _sharedOptions.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;

                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500"); //This must use a subdirectory as must start with '/'
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            app.UseIdentityServer();

            app.UseRouting();
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseMiddleware<MaintenancePageMiddleware>(_sharedOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(_sharedOptions.StickySessions); //Enable/Disable sticky sessions based on  
            app.UseMiddleware<SecurityHeaderMiddleware>(); //Add/remove security headers from all responses

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

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

        public void RegisterModules(IList<Type> modules)
        {
            modules.AddDependency<WebUI.StaticFiles.DependencyModule>();
            modules.AddDependency<ModernSlavery.BusinessDomain.Account.DependencyModule>();

            //Register the file storage dependencies
            modules.AddDependency<FileStorageDependencyModule>();

            //Register the queue storage dependencies
            modules.AddDependency<QueueStorageDependencyModule>();

            //Register google analytics tracker
            modules.AddDependency<GoogleAnalyticsDependencyModule>();

            //Register the app insights dependencies
            modules.AddDependency<ApplicationInsightsDependencyModule>();

        }

       
    }
}