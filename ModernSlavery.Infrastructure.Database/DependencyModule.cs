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
        private readonly DatabaseOptions _databaseOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            DatabaseOptions databaseOptions,
            SharedOptions sharedOptions)
        {
            //Set any required local IOptions here
            _logger = logger;
            _databaseOptions = databaseOptions;
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

            builder.RegisterType<DataImporter>().As<IDataImporter>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Only when migrations is enabled
            if (_databaseOptions.UseMigrations)
            {
                //Ensure import files exist on remote storage
                var fileRepository = lifetimeScope.Resolve<IFileRepository>();
                Task.WaitAll(
                    fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.StatementDiligenceTypes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.StatementPolicyTypes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.StatementRiskTypes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.StatementSectorTypes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.StatementTrainingTypes, _sharedOptions.DataPath),
                    fileRepository.PushRemoteFileAsync(Filenames.ImportOrganisations, _sharedOptions.DataPath)
                );

                //Seed database whenver migrations are applied
                var dbContext = lifetimeScope.Resolve<IDbContext>();
                if (dbContext.MigrationsApplied)
                {
                    var _dataImporter = lifetimeScope.Resolve<IDataImporter>();

                    _dataImporter.ImportOrganisationsAsync().Wait();
                    _dataImporter.ImportSICCodesAsync().Wait();
                    _dataImporter.ImportSICSectionsAsync().Wait();
                    _dataImporter.ImportStatementDiligenceTypesAsync().Wait();
                    _dataImporter.ImportStatementPolicyTypesAsync().Wait();
                    _dataImporter.ImportStatementRiskTypesAsync().Wait();
                    _dataImporter.ImportStatementSectorTypesAsync().Wait();
                    _dataImporter.ImportStatementTrainingTypesAsync().Wait();
                }
            }
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}