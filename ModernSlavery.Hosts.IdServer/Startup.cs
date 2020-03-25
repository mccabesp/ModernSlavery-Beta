using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.IdServer
{
    public class Startup:IStartup
    {
        private readonly IConfiguration _Config;
        private readonly ILogger _Logger;
        private IServiceProvider _ServiceProvider;

        public Startup(IConfiguration config)
        {
            _Config = config;
            _Logger = Activator.CreateInstance<Logger<Startup>>();
        }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //Load all configuration options and use them to register all dependencies
            return services.SetupDependencies<DependencyModule>(_Config);
        }

       
        public void Configure(IApplicationBuilder app)
        {
            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(_Config["DefaultEncryptionKey"]);

            var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var sharedOptions = app.ApplicationServices.GetService<SharedOptions>();
            var responseCachingOptions = app.ApplicationServices.GetService<ResponseCachingOptions>();

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
            app.UseMiddleware<MaintenancePageMiddleware>(sharedOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(sharedOptions.StickySessions); //Enable/Disable sticky sessions based on  
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

    }
}
