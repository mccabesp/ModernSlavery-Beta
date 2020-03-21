using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel.Options;
using ModernSlavery.Tests.Common;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Repositories.UserRepository
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class FindBySubjectIdAsyncTests : BaseTestFixture<FindBySubjectIdAsyncTests.DependencyModule>
    {
        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            // service under test
            testUserRepo =
                new ModernSlavery.Infrastructure.Data.UserRepository(new DatabaseOptions(), new GlobalOptions(),
                    mockDataRepo.Object, Mock.Of<IUserLogger>(), DependencyContainer.Resolve<IMapper>());
        }

        public class DependencyModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Initialise AutoMapper
                var mapperConfig = new MapperConfiguration(config =>
                {
                    config.AddMaps(typeof(ModernSlavery.Infrastructure.Data.UserRepository));
                });
                builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            }
        }

        private Mock<IDataRepository> mockDataRepo;
        private IUserRepository testUserRepo;

        [TestCase(23322, UserStatuses.Active)]
        [TestCase(23322, UserStatuses.Active, UserStatuses.Active)]
        [TestCase(235251, UserStatuses.New, UserStatuses.New)]
        [TestCase(980964, UserStatuses.Suspended, UserStatuses.Suspended)]
        [TestCase(707643, UserStatuses.Retired, UserStatuses.Retired)]
        public async Task FindsMatchingUserIdUsingSingleStatusFilter(long testFindId,
            UserStatuses testExpectedStatus,
            params UserStatuses[] testStatusFilter)
        {
            // Act
            var actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId, testStatusFilter);

            // Assert
            Assert.AreEqual(testFindId, actualUser.UserId, "Expected user id to match");
            Assert.AreEqual(testExpectedStatus, actualUser.Status, "Expected user status to match");
        }

        [TestCase(235251, UserStatuses.New, UserStatuses.Retired)]
        [TestCase(707643, UserStatuses.New, UserStatuses.Retired)]
        public async Task FindsMatchingUserIdUsingMultipleStatusFilters(long testFindId,
            params UserStatuses[] testStatusFilters)
        {
            // Act
            var actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId, testStatusFilters);

            // Assert
            Assert.AreEqual(testFindId, actualUser.UserId, "Expected user id to match");
            Assert.IsTrue(testStatusFilters.Contains(actualUser.Status), "Expected user status to match");
        }

        [TestCase(999999999)]
        public async Task ReturnsNullWhenUserIdDoesNotMatch(long testFindId)
        {
            // Act
            var actualUser = await testUserRepo.FindBySubjectIdAsync(testFindId);

            // Assert
            Assert.IsNull(actualUser, "Expected user to be null");
        }
    }
}