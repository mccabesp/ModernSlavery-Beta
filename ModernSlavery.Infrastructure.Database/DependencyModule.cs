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
using ModernSlavery.Core.Options;
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

            builder.RegisterType<DataImporter>().As<IDataImporter>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Ensure we have a system user
            var _dataImporter = lifetimeScope.Resolve<IDataImporter>();
            _dataImporter.EnsureSystemUserExistsAsync().Wait();

            //Only when migrations is enabled for this application
            if (_databaseOptions.GetIsMigrationApp())
            {
                var importSicSections = false;
                var importSicCodes = false;
                var importStatementSectorTypes = false;
                var importImportPrivateOrganisations = false;
                var importImportPublicOrganisations = false;

                //Ensure import files exist on remote storage
                var fileRepository = lifetimeScope.Resolve<IFileRepository>();

                Task.WaitAll(
                    Task.Run(async () => { await fileRepository.PushRemoteFileAsync(Filenames.ShortCodes, _sharedOptions.AppDataPath); }), 
                    Task.Run(async () => { importSicSections = await fileRepository.PushRemoteFileAsync(Filenames.SicSections, _sharedOptions.AppDataPath).ConfigureAwait(false); }),
                    Task.Run(async () => { importSicCodes = await fileRepository.PushRemoteFileAsync(Filenames.SicCodes, _sharedOptions.AppDataPath).ConfigureAwait(false); }),
                    Task.Run(async () => { importStatementSectorTypes = await fileRepository.PushRemoteFileAsync(Filenames.StatementSectorTypes, _sharedOptions.AppDataPath).ConfigureAwait(false); }),
                    Task.Run(async () => { importImportPrivateOrganisations = await fileRepository.PushRemoteFileAsync(Filenames.ImportPrivateOrganisations, _sharedOptions.AppDataPath).ConfigureAwait(false); }),
                    Task.Run(async () => { importImportPublicOrganisations = await fileRepository.PushRemoteFileAsync(Filenames.ImportPublicOrganisations, _sharedOptions.AppDataPath).ConfigureAwait(false); })
                );

                if (_databaseOptions.ImportSeedData)
                {
                    //Reimport to database whenever migrations are applied or new file was imported
                    var dbContext = lifetimeScope.Resolve<IDbContext>();
                    if (dbContext.MigrationsApplied || importSicSections) _dataImporter.ImportSICSectionsAsync().Wait();
                    if (dbContext.MigrationsApplied || importSicCodes) _dataImporter.ImportSICCodesAsync().Wait();
                    if (dbContext.MigrationsApplied || importStatementSectorTypes) _dataImporter.ImportStatementSectorTypesAsync().Wait();
                    if (dbContext.MigrationsApplied || importImportPrivateOrganisations) _dataImporter.ImportPrivateOrganisationsAsync(-1).Wait();
                    if (dbContext.MigrationsApplied || importImportPublicOrganisations) _dataImporter.ImportPublicOrganisationsAsync(-1).Wait();
                }
            }
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}