using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Extensions;
using ModernSlavery.IdentityServer4.Classes;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Database.Classes;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.Classes;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Hosts;

namespace ModernSlavery.IdentityServer4
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

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            #region Bind the options classes and register as services
            OptionsBinder = new OptionsBinder(services, _Config);
            OptionsBinder.BindAssemblies("ModernSlavery");
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            #endregion

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(globalOptions.DateTimeOffset);

            #region Configure identity server

            services.AddSingleton<IEventSink, AuditEventSink>();

            var clients=new Clients(globalOptions);
            var resources=new Resources(globalOptions);

            var identityServer=services.AddIdentityServer(
                    options => {
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

            if (Debugger.IsAttached || _Config["Environment"].EqualsI("Development","Local"))
                identityServer.AddDeveloperSigningCredential();
            else
                identityServer.AddSigningCredential(LoadCertificate(globalOptions));

            #endregion

            //Allow caching of http responses
            services.AddResponseCaching();

            //This is to allow access to the current http context anywhere
            services.AddHttpContextAccessor();

            services.AddControllersWithViews();

            var mvcBuilder = services.AddRazorPages();

            // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-3.1#runtime-compilation
            if (_Config["Environment"].EqualsI("Development", "Local")) mvcBuilder.AddRazorRuntimeCompilation();


            //Add the distributed cache and data protection
            services.AddDistributedCache(_Config).AddDataProtection(_Config);

            services.AddApplicationInsightsTelemetry(_Config.GetValue("ApplicationInsights:InstrumentationKey",_Config["APPINSIGHTS-INSTRUMENTATIONKEY"]));

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

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

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        private IContainer BuildContainer(ContainerBuilder builder)
        {
            var globalOptions = OptionsBinder.Get<GlobalOptions>();
            var databaseOptions = OptionsBinder.Get<DatabaseOptions>();
            var storageOptions = OptionsBinder.Get<StorageOptions>();

            //Register the configuration
            builder.RegisterInstance(_Config).SingleInstance();

            builder.Register(c => new SqlRepository(new DatabaseContext(globalOptions, databaseOptions)))
                .As<IDataRepository>()
                .InstancePerLifetimeScope();

            // Register storage
            string azureStorageShareName = _Config["AzureStorageShareName"];
            string localStorageRoot = _Config["LocalStorageRoot"];
            if (string.IsNullOrWhiteSpace(localStorageRoot))
            {
                //Exponential retry policy is recommended for background tasks - see https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#azure-storage
                builder.Register(
                        c => new AzureFileRepository(
                            storageOptions,
                            new ExponentialRetry(TimeSpan.FromSeconds(3), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(storageOptions)).As<IFileRepository>().SingleInstance();
            }

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register log records (without key filtering)
            builder.RegisterType<UserLogRecord>().As<IUserLogRecord>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();

            // Initialise AutoMapper
            MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                // register all out mapper profiles (classes/mappers/*)
                // config.AddMaps(typeof(MvcApplication));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            //Build the container
            return builder.Build();
        }

        public void Configure(IApplicationBuilder app)
        {
            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(_Config["DefaultEncryptionKey"]);

            var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var globalOptions = app.ApplicationServices.GetService<GlobalOptions>();
            var responseCachingOptions = OptionsBinder.Get<ResponseCachingOptions>();

            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (Debugger.IsAttached || _Config["Environment"].EqualsI("Development","Local"))
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
                            if (responseCachingOptions.StaticCacheSeconds > 0)ctx.Context.SetResponseCache(responseCachingOptions.StaticCacheSeconds);
                        }
                    });
            }

            app.UseRouting();
            app.UseMiddleware<MaintenancePageMiddleware>(globalOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(globalOptions.StickySessions); //Enable/Disable sticky sessions based on  
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

        private X509Certificate2 LoadCertificate(GlobalOptions globalOptions)
        {
            //Load the site certificate
            string certThumprint = _Config["WEBSITE_LOAD_CERTIFICATES"].SplitI(";").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(certThumprint))
            {
                certThumprint = _Config["CertThumprint"].SplitI(";").FirstOrDefault();
            }

            X509Certificate2 cert = null;
            if (!string.IsNullOrWhiteSpace(certThumprint))
            {
                cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);
                _Logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from thumbprint '{certThumprint}'");
            }
            else
            {
                string certPath = Path.Combine(Directory.GetCurrentDirectory(), @"LocalHost.pfx");
                cert = HttpsCertificate.LoadCertificateFromFile(certPath, "LocalHost");
                _Logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from file '{certPath}'");
            }

            if (globalOptions.CertExpiresWarningDays > 0)
            {
                DateTime expires = cert.GetExpirationDateString().ToDateTime();
                if (expires < VirtualDateTime.UtcNow)
                {
                    _Logger.LogError(
                        $"The website certificate for '{globalOptions.EXTERNAL_HOST}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                }
                else
                {
                    TimeSpan remainingTime = expires - VirtualDateTime.Now;

                    if (expires < VirtualDateTime.UtcNow.AddDays(globalOptions.CertExpiresWarningDays))
                    {
                        _Logger.LogWarning(
                            $"The website certificate for '{globalOptions.SiteAuthority}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
                    }
                }
            }

            return cert;
        }

    }
}
