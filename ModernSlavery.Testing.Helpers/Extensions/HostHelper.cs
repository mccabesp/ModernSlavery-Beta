using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.WebUI.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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

        public static IDataRepository GetDataRepository(this IHost host)
        {
            return host.Services.GetRequiredService<IDataRepository>();
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
