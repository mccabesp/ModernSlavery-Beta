using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static IHostBuilder ConfigureWebHostBuilder<TStartupModule>(this IHostBuilder hostBuilder, string applicationName=null, string contentRoot = null, string webRoot = null) where TStartupModule : class, IDependencyModule
        {
            var dependencyBuilder = hostBuilder.ConfigureHost<TStartupModule>(applicationName,contentRoot);

            //Configure the host defaults
            hostBuilder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                webHostBuilder.CaptureStartupErrors(true).UseSetting(WebHostDefaults.DetailedErrorsKey,"true");
                
                webHostBuilder.ConfigureKestrel(options =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    });

                //Specify the root path of the content
                if (!string.IsNullOrWhiteSpace(contentRoot))webHostBuilder.UseContentRoot(contentRoot);

                //Specify the root path of the site
                if (!string.IsNullOrWhiteSpace(webRoot))webHostBuilder.UseWebRoot(webRoot);
                
               webHostBuilder.Configure(appBuilder =>
               {
                   dependencyBuilder.Autofac.RegisterInstance(appBuilder).As<IApplicationBuilder>();
                   dependencyBuilder.ConfigureModules();
               });
            });

            return hostBuilder;
        }
    }
}