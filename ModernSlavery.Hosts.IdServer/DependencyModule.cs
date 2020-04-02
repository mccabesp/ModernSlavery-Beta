using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using AutoMapper;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Hosts.IdServer.Classes;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Options;

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

        public void Register(IDependencyBuilder builder)
        {
            #region Configure identity server

            builder.Services.AddSingleton<IEventSink, AuditEventSink>();

            var clients = new Clients(_sharedOptions);
            var resources = new Resources(_sharedOptions);

            var identityServer = builder.Services.AddIdentityServer(
                    options =>
                    {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.UserInteraction.LoginUrl = "/sign-in";
                        options.UserInteraction.LogoutUrl = "/sign-out";
                        options.UserInteraction.ErrorUrl = "/error";
                    })
                .AddInMemoryClients(clients.Get())
                .AddInMemoryIdentityResources(resources.GetIdentityResources())
                //.AddInMemoryApiResources(Resources.GetApiResources())
                .AddCustomUserStore();

            if (Debugger.IsAttached || _sharedOptions.IsDevelopment() || _sharedOptions.IsLocal())
                identityServer.AddDeveloperSigningCredential();
            else
                identityServer.AddSigningCredential(LoadCertificate(_sharedOptions));

            #endregion

            //Allow caching of http responses
            builder.Services.AddResponseCaching();

            //This is to allow access to the current http context anywhere
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews();

            var mvcBuilder = builder.Services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_sharedOptions.IsDevelopment() || _sharedOptions.IsLocal()) mvcBuilder.AddRazorRuntimeCompilation();

            //Add the distributed cache and data protection
            builder.Services.AddDistributedCache(_distributedCacheOptions)
                .AddDataProtection(_dataProtectionOptions);

            //Add app insights tracking
            builder.Services.AddApplicationInsightsTelemetry(_sharedOptions.AppInsights_InstrumentationKey);

            //This may now be required 
            builder.Services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Override any test services
            ConfigureTestServices?.Invoke(builder.Services);

            //Register the file storage dependencies
            builder.RegisterModule<FileStorageDependencyModule>();

            // Register queues (without key filtering)
            builder.Autofac
                .Register(c => new LogEventQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();
            builder.Autofac
                .Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();

            // Register log records (without key filtering)
            builder.Autofac.RegisterType<UserAuditLogger>().As<IUserLogger>().SingleInstance();

            // Register Action helpers
            builder.Autofac.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>()
                .SingleInstance();

            // Initialise AutoMapper
            var mapperConfig = new MapperConfiguration(config =>
            {
                // register all out mapper profiles (classes/mappers/*)
                // config.AddMaps(typeof(MvcApplication));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            builder.Autofac.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder.Autofac);
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Add configuration here
            var app = lifetimeScope.Resolve<IApplicationBuilder>();

            var lifetime = lifetimeScope.Resolve<IHostApplicationLifetime>();

            lifetimeScope.UseLogEventQueueLogger();

            app.UseMiddleware<ExceptionMiddleware>();
            if (Debugger.IsAttached || _sharedOptions.IsDevelopment() || _sharedOptions.IsLocal())
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


        private X509Certificate2 LoadCertificate(SharedOptions sharedOptions)
        {
            //Load the site certificate
            var certThumprint = sharedOptions.Website_Load_Certificates.SplitI(";").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(certThumprint))
                certThumprint = _sharedOptions.CertThumprint.SplitI(";").FirstOrDefault();

            X509Certificate2 cert = null;
            if (!string.IsNullOrWhiteSpace(certThumprint))
            {
                cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);
                _logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from thumbprint '{certThumprint}'");
            }
            else
            {
                var certPath = Path.Combine(Directory.GetCurrentDirectory(), @"LocalHost.pfx");
                cert = HttpsCertificate.LoadCertificateFromFile(certPath, "LocalHost");
                _logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from file '{certPath}'");
            }

            if (sharedOptions.CertExpiresWarningDays > 0)
            {
                var expires = cert.GetExpirationDateString().ToDateTime();
                if (expires < VirtualDateTime.UtcNow)
                {
                    _logger.LogError(
                        $"The website certificate for '{sharedOptions.ExternalHost}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                }
                else
                {
                    var remainingTime = expires - VirtualDateTime.Now;

                    if (expires < VirtualDateTime.UtcNow.AddDays(sharedOptions.CertExpiresWarningDays))
                        _logger.LogWarning(
                            $"The website certificate for '{sharedOptions.SiteAuthority}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
                }
            }

            return cert;
        }
    }
}