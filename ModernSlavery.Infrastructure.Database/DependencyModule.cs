using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Database.Classes;

namespace ModernSlavery.Infrastructure.Database
{
    public class DependencyModule : IDependencyModule
    {

        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions)
        {
            //Set any required local IOptions here
            _logger = logger;
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContext>().As<IDbContext>().InstancePerLifetimeScope();

            builder.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerLifetimeScope();

            builder.RegisterType<ShortCodesRepository>().As<IShortCodesRepository>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Ensure ShortCodes, SicCodes and SicSections exist on remote 
            var fileRepository = lifetimeScope.Resolve<IFileRepository>(); 
            
            Task.WaitAll(
                fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.DataPath),
                fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.DataPath),
                fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.DataPath)
            );
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}