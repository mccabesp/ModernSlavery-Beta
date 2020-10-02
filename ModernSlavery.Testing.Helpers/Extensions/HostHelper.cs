using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.WebUI.Shared.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ModernSlavery.Testing.Helpers
{
    public static class HostHelper
    {
        public static IHost CreateTestWebHost<TStartupTestModule>(string environment="Test", string applicationName = null) where TStartupTestModule : class, IDependencyModule
        {
            //Build the web host using the default dependencies
            var commandArgs = new List<string>();
            if (!string.IsNullOrWhiteSpace(environment)) 
            {
                commandArgs.Add($"--{HostDefaults.EnvironmentKey}"); 
                commandArgs.Add(environment); 
            }
            
            if (!string.IsNullOrWhiteSpace(applicationName)) 
            { 
                commandArgs.Add($"--{HostDefaults.ApplicationKey}");
                commandArgs.Add(applicationName); 
            }

            var testWebHostBuilder = WebHost.ConfigureWebHostBuilder<TStartupTestModule>(commandlineArgs: commandArgs.ToArray());

            return testWebHostBuilder.Build();
        }

        public static T GetDependency<T>(this IHost host)
        {
            return host.Services.GetRequiredService<T>();
        }

        public static IServiceScopeFactory GetServiceScopeFactory(this IHost host)
        {
            return host.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public static IServiceScope CreateServiceScope(this IServiceScopeFactory serviceScopeFactory)
        {
            return serviceScopeFactory.CreateScope();
        }

        public static IServiceScope CreateServiceScope(this IHost host)
        {
            var serviceScopeFactory= host.GetServiceScopeFactory();
            return serviceScopeFactory.CreateScope();
        }

        public static IConfiguration GetConfiguration(this IHost host)
        {
            return host.Services.GetRequiredService<IConfiguration>();
        }

        public static IDataRepository GetDataRepository(this IHost host, IServiceScope serviceScope=null)
        {
            //if (serviceScope == null) serviceScope = host.CreateServiceScope(); //May need this to create a new scope every time but needs chekcing first
            var serviceProvider = serviceScope?.ServiceProvider ?? host.Services;
            return serviceProvider.GetRequiredService<IDataRepository>();
        }

        public static IDbContext GetDbContext(this IHost host, IServiceScope serviceScope = null)
        {
            var serviceProvider = serviceScope?.ServiceProvider ?? host.Services;
            return serviceProvider.GetRequiredService<IDbContext>();
        }

        public static IFileRepository GetFileRepository(this IHost host)
        {
            return host.Services.GetRequiredService<IFileRepository>();
        }

        public static IHttpSession GetHttpSession(this IHost host)
        {
            return host.Services.GetRequiredService<IHttpSession>();
        }
    }
}
