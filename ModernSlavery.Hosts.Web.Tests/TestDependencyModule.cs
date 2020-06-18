using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Testing.Helpers;
using Moq;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class TestDependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;

        public TestDependencyModule(ILogger<TestDependencyModule> logger)
        {
            _logger = logger;
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
        }

        public void RegisterModules(IList<Type> modules)
        {
            modules.AddDependency<Hosts.Web.DependencyModule>();
        }
    }
}