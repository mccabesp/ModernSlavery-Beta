using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.Infrastructure.Messaging;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.Classes;
using ModernSlavery.SharedKernel.Extensions;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Services;

namespace ModernSlavery.IdentityServer4
{
    public class WebHostDependencyModule: IDependencyModule
    {
        private readonly GlobalOptions _globalOptions;
        private readonly StorageOptions _storageOptions;
        public WebHostDependencyModule(GlobalOptions globalOptions, StorageOptions storageOptions)
        {
            _globalOptions = globalOptions;
            _storageOptions = storageOptions;
        }

        public void Bind(ContainerBuilder builder)
        {
            //Register the database dependencies
            builder.BindResolvedDependencyModule<DatabaseDependencyModule>();

            //Register the file storage dependencies
            builder.BindResolvedDependencyModule<FileStorageDependencyModule>();

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register log records (without key filtering)
            builder.RegisterType<UserLogRecord>().As<IUserLogRecord>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();

            // Initialise AutoMapper
            MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                // register all out mapper profiles (classes/mappers/*)
                // config.AddMaps(typeof(MvcApplication));
                // allows auto mapper to inject our dependencies
                //config.ConstructServicesUsing(serviceTypeToConstruct =>
                //{
                //    //TODO
                //});
            });

            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();


        }
    }
}
