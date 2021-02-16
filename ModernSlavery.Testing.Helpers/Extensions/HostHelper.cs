using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.WebUI.Shared.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http;
using ModernSlavery.BusinessDomain.DevOps.Testing;

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

        private static IServiceScopeFactory GetServiceScopeFactory(this IHost host)
        {
            return host.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public static IServiceScope CreateServiceScope(this IHost host)
        {
            var serviceScopeFactory = host.GetServiceScopeFactory();
            return serviceScopeFactory.CreateScope();
        }

        public static T GetDependency<T>(this IHost host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            return host.Services.GetRequiredService<T>();
        }
        public static T GetDependency<T>(this IServiceScope serviceScope)
        {
            if (serviceScope == null) throw new ArgumentNullException(nameof(serviceScope));
            return serviceScope.ServiceProvider.GetRequiredService<T>();
        }

        public static IConfiguration GetConfiguration(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IConfiguration>();
        }

        public static IDataRepository GetDataRepository(this IServiceScope serviceScope)
        {
            return serviceScope.GetDependency<IDataRepository>();
        }

        public static IDbContext GetDbContext(this IServiceScope serviceScope)
        {
            return serviceScope.GetDependency<IDbContext>();
        }

        public static IFileRepository GetFileRepository(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IFileRepository>();
        }

        public static ITestBusinessLogic GetTestBusinessLogic(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ITestBusinessLogic>();
        }

    }
}
