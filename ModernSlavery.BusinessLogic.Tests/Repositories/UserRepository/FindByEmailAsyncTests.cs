using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.BusinessLogic.Account.Abstractions;
using ModernSlavery.BusinessLogic.LogRecords;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Tests.Common;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class FindByEmailAsyncTests : BaseTestFixture<FindByEmailAsyncTests.DependencyModule>
    {
        public class DependencyModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Initialise AutoMapper
                MapperConfiguration mapperConfig = new MapperConfiguration(config => {
                    config.AddMaps(typeof(ModernSlavery.BusinessLogic.Account.Repositories.UserRepository));
                });
                builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            }
        }

        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            // service under test
            testUserRepo =
                new ModernSlavery.BusinessLogic.Account.Repositories.UserRepository(mockDataRepo.Object, Mock.Of<IUserLogRecord>(),DependencyContainer.Resolve<IMapper>());
        }

        private Mock<IDataRepository> mockDataRepo;
        private IUserRepository testUserRepo;

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active)]
        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.New)]
        [TestCase("suspended1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Suspended, UserStatuses.Suspended)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Active, UserStatuses.Active)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired, UserStatuses.Retired)]
        public async Task FindsMatchingEmailUsingSingleStatusFilter(string testFindEmail,
            UserStatuses testExpectedStatus,
            params UserStatuses[] testStatusFilter)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail, testStatusFilter);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.AreEqual(testExpectedStatus, actualUser.Status, "Expected user status to match");
        }

        [TestCase("new1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        [TestCase("retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New, UserStatuses.Retired)]
        public async Task FindsMatchingEmailUsingMultipleStatusFilters(string testFindEmail, params UserStatuses[] testStatusFilters)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail, testStatusFilters);

            // Assert
            Assert.AreEqual(testFindEmail, actualUser.EmailAddress, "Expected email to match");
            Assert.IsTrue(testStatusFilters.Contains(actualUser.Status), "Expected user status to match");
        }

        [TestCase("missing@ad5bda75-e514-491b-b74d-4672542cbd15.com")]
        public async Task ReturnsNullWhenEmailDoesNotMatch(string testFindEmail)
        {
            // Act
            User actualUser = await testUserRepo.FindByEmailAsync(testFindEmail);

            // Assert
            Assert.IsNull(actualUser, "Expected user to be null");
        }

    }

}
