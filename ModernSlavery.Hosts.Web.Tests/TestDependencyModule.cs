using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Testing.Helpers;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class TestDependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public TestDependencyModule(ILogger<TestDependencyModule> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO Use CoHo specific test key
            //TODO Disable GoogleAnalytics
            //TODO Disable App insights
            //TODO Mock Search service
            //TODO Mock Logs
            //TODO Mock SendGrid
            //TODO Load RCL from config
            //TODO Mock signin
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //File Storage uses AppSetting LocalStorageRoot to use ISystemFileRepository
            //Distributed cache uses AppSetting DistributedCache:Type" to use in-memory cache which is also used for session
            //GovNotify uses AppSetting Test key Email:Providers:GovNotify:ApiKey and AllowTestKeyOnly to simulate sending of emails and letters 

            //Create and In-memory database
            var databaseContext = DependencyFactory.CreateInMemoryDatabaseContext();
            builder.RegisterInstance(databaseContext).As<IDbContext>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Send logs to file for later upload to DevOps as test attachments
            var loggerFactory = lifetimeScope.Resolve<ILoggerFactory>();
            var logFilepath = Path.Combine(_configuration["Filepaths:LogFiles"], $"{AppDomain.CurrentDomain.FriendlyName}.{_configuration[HostDefaults.EnvironmentKey]}.log");
            loggerFactory.AddFile(logFilepath);
        }

        public void RegisterModules(IList<Type> modules)
        {
            modules.AddDependency<Hosts.Web.DependencyModule>();
        }
    }
}