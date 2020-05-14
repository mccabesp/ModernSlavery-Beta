using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Reflection;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebHost
    {
        public class WebHostEnvironment : IWebHostEnvironment
        {
            public IFileProvider ContentRootFileProvider { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string WebRootPath { get; set; }
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string ContentRootPath { get; set; }
        }

        public static IHostBuilder ConfigureWebHostBuilder<TStartupModule>(string applicationName = null, string contentRoot = null, string webRoot = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName = Assembly.GetEntryAssembly().GetName().Name;

            var hostBuilder = new HostBuilder();
            
            //Setup the configuration sources and builder
            var configBuilder=hostBuilder.ConfigureHost(applicationName, contentRoot, commandlineArgs: commandlineArgs);

            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(configBuilder.Build());
            optionsBinder.BindAssemblies();
            
            var dependencyBuilder = new DependencyBuilder();
            dependencyBuilder.Build<TStartupModule>(optionsBinder.Services);
            dependencyBuilder.PopulateHostContainer(hostBuilder);

            //Configure the host defaults
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                dependencyBuilder.PopulateHostServices(webHostBuilder);

                //Ensure the static assets of any RCL are copied to the consuming app location
                webHostBuilder.UseStaticWebAssets();

                //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                webHostBuilder.CaptureStartupErrors(true).UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                webHostBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });

                //Specify the root path of the content
                if (!string.IsNullOrWhiteSpace(contentRoot)) webHostBuilder.UseContentRoot(contentRoot);

                //Specify the root path of the site
                if (!string.IsNullOrWhiteSpace(webRoot)) webHostBuilder.UseWebRoot(webRoot);

                //Configure the services
                dependencyBuilder.ConfigureHost(webHostBuilder);
            });

            return hostBuilder;
        }
    }
}