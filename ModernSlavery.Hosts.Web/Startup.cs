using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Helpers;
using ModernSlavery.WebUI.Shared.Classes.Middleware;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.Infrastructure.CompaniesHouse;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.Hosts.WebHost;
using ModernSlavery.Infrastructure.Logging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using ModernSlavery.Infrastructure.Telemetry;

namespace ModernSlavery.WebUI
{
    public class Startup:IStartup
    {
        private readonly IConfiguration _Config;
        private readonly ILogger _Logger;
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
            return services.ConfigureDependencies<AppDependencyModule>(_Config);
        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(IApplicationBuilder app)
        {
            var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var sharedOptions = app.ApplicationServices.GetService<SharedOptions>();
            var responseCachingOptions = app.ApplicationServices.GetService<ResponseCachingOptions>();

            //Initialise the virtual date and time
            VirtualDateTime.Initialise(sharedOptions.DateTimeOffset);

            //Set the default encryption key
            Encryption.SetDefaultEncryptionKey(sharedOptions.DefaultEncryptionKey);

            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (sharedOptions.UseDeveloperExceptions)
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
            app.UseMiddleware<MaintenancePageMiddleware>(sharedOptions.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<StickySessionMiddleware>(sharedOptions.StickySessions); //Enable/Disable sticky sessions based on  

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
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.ShortCodes, sharedOptions.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicCodes, sharedOptions.DataPath),
                        Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicSections, sharedOptions.DataPath)
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
