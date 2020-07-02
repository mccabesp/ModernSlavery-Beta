using Autofac;
using ModernSlavery.BusinessLogic.Admin;
using ModernSlavery.BusinessLogic.Registration;
using ModernSlavery.BusinessLogic.Registration.Models;
using ModernSlavery.BusinessLogic.Submission;
using ModernSlavery.BusinessLogic.Viewing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;
using Moq;

namespace ModernSlavery.BusinessLogic.Tests
{
    public class BaseBusinessLogicTests
    {
        private readonly ILifetimeScope _lifetimeScope;

        public BaseBusinessLogicTests()
        {
            var builder = new ContainerBuilder();

            builder.Register(c => Mock.Of<IOrganisationBusinessLogic>()).As<IOrganisationBusinessLogic>();
            builder.Register(c => Mock.Of<ISharedBusinessLogic>()).As<ISharedBusinessLogic>();
            builder.Register(c => Mock.Of<IDataRepository>()).As<IDataRepository>();
            builder.Register(c => Mock.Of<ISubmissionBusinessLogic>()).As<ISubmissionBusinessLogic>();
            builder.Register(c => Mock.Of<IScopeBusinessLogic>()).As<IScopeBusinessLogic>();
            builder.Register(c => Mock.Of<ISearchBusinessLogic>()).As<ISearchBusinessLogic>();
            builder.Register(c => Mock.Of<UpdateFromCompaniesHouseService>()).As<UpdateFromCompaniesHouseService>();
            builder.Register(c => Mock.Of<IEncryptionHandler>()).As<IEncryptionHandler>();
            builder.Register(c => Mock.Of<ISecurityCodeBusinessLogic>()).As<ISecurityCodeBusinessLogic>();
            builder.Register(c => Mock.Of<IObfuscator>()).As<IObfuscator>();

            var container = builder.Build();

            _lifetimeScope = container.BeginLifetimeScope();
        }

        public T Get<T>()
        {
            return _lifetimeScope.Resolve<T>();
        }
    }
}