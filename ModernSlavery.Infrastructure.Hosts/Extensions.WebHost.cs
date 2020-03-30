using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Configuration;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebHost
    {
        public static IHostBuilder ConfigureWebHostBuilder(string applicationName = null, string contentRoot = null, string webRoot = null, params string[] commandlineArgs)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureHost(applicationName, contentRoot, commandlineArgs: commandlineArgs);

            //Configure the host defaults
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
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
                webHostBuilder.Configure(appBuilder =>
                {
                    var dependencyBuilder = (DependencyBuilder)hostBuilder.Properties["dependencyBuilder"];
                    dependencyBuilder.ConfigureModules(appBuilder);
                });
            });

            return hostBuilder;
        }
    }
}