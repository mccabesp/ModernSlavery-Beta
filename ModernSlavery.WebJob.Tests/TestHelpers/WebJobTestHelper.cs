using Autofac;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Data;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.Mocks;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;

namespace ModernSlavery.WebJob.Tests.TestHelpers
{
    internal static class WebJobTestHelper
    {
        private static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<WebJob.Functions>().InstancePerDependency();

            //Create an in-memory version of the database
            if (dbObjects != null && dbObjects.Length > 0)
            {
                builder.RegisterInMemoryTestDatabase(dbObjects);
            }
            else
            {
                var mockDataRepo = MoqHelpers.CreateMockDataRepository();
                builder.Register(c => mockDataRepo.Object).As<IDataRepository>().InstancePerLifetimeScope();
            }

            builder.Register(c => Mock.Of<ICompaniesHouseAPI>()).As<ICompaniesHouseAPI>().SingleInstance();

            builder.Register(c => Mock.Of<ISearchServiceClient>()).As<ISearchServiceClient>().SingleInstance();

            //Create the mock repositories
            builder.Register(c => new MockFileRepository()).As<IFileRepository>().SingleInstance();
            builder.Register(c => new MockSearchRepository()).As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance();
            builder.RegisterType(typeof(AzureSicCodeSearchRepository)).As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance();

            // BL Services
            builder.RegisterInstance(ConfigHelpers.Config).SingleInstance();
            builder.RegisterType<CommonBusinessLogic>().As<ICommonBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SecurityCodeBusinessLogic>().As<ISecurityCodeBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.RegisterType<SearchBusinessLogic>().As<ISearchBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>()
                .InstancePerLifetimeScope();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", ConfigHelpers.GlobalOptions.ObfuscationSeed);
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();

            builder.Register(c => Mock.Of<IMessenger>()).As<IMessenger>().SingleInstance();
            builder.Register(c => Mock.Of<IGovNotifyAPI>()).As<IGovNotifyAPI>().SingleInstance();

            builder.RegisterInstance(new NullLoggerFactory()).As<ILoggerFactory>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            SetupHelpers.SetupMockLogRecordGlobals(builder);

            return builder.Build();
        }

        public static WebJob.Functions SetUp(params object[] dbObjects)
        {
            var containerIoc = BuildContainerIoC(dbObjects);
            var functions = containerIoc.Resolve<WebJob.Functions>();

            return functions;
        }
    }
}