using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Abstractions;
using ModernSlavery.BusinessLogic.LogRecords;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Entities;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;

using NUnit.Framework;

namespace Repositories.RegistrationRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class RemoveRegistrationAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            // mock data
            DatabaseContext dbContext = AutoFacHelpers.CreateInMemoryTestDatabase(UserOrganisationHelper.CreateRegistrations());

            mockDataRepo = new SqlRepository(dbContext);
            mockLogRecordLogger = new Mock<IRegistrationLogRecord>();

            // service under test
            testRegistrationRepo =
                new ModernSlavery.BusinessLogic.Repositories.RegistrationRepository(mockDataRepo, mockLogRecordLogger.Object);
        }

        private IDataRepository mockDataRepo;
        private Mock<IRegistrationLogRecord> mockLogRecordLogger;

        private IRegistrationRepository testRegistrationRepo;

        [Test]
        public async Task UserCanUnregisterAnotherUser()
        {
            // Arrange
            User testUnregisterUser = mockDataRepo.GetAll<User>()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            User testActionByUser = mockDataRepo.GetAll<User>()
                .Where(u => u.EmailAddress == "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testUnregisterUser.UserOrganisations.FirstOrDefault();
            var calledLogUnregisteredAsync = false;

            // Flag LogUnregisteredSelfAsync
            mockLogRecordLogger.Setup(x => x.LogUnregisteredAsync(It.IsAny<UserOrganisation>(), It.IsAny<string>()))
                .Callback(
                    (UserOrganisation uo, string abe) => {
                        calledLogUnregisteredAsync = true;
                        Assert.AreEqual(uo, testUserOrg, "Expected to log user org");
                        Assert.AreEqual(abe, testActionByUser.EmailAddress, "Expected log action by email to match");
                    })
                .Returns(Task.CompletedTask);

            // Act
            await testRegistrationRepo.RemoveRegistrationAsync(testUserOrg, testActionByUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());

            // Assert log
            Assert.IsTrue(calledLogUnregisteredAsync, "Expected LogUnregisteredSelfAsync to be called");
        }

        [Test]
        public async Task UserCanUnregisterThemselves()
        {
            // Arrange
            User testUnregisterUser = mockDataRepo.GetAll<User>()
                .Where(u => u.EmailAddress == "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com")
                .FirstOrDefault();

            UserOrganisation testUserOrg = testUnregisterUser.UserOrganisations.FirstOrDefault();
            var calledLogUnregisteredSelfAsync = false;

            // Flag LogUnregisteredSelfAsync
            mockLogRecordLogger.Setup(x => x.LogUnregisteredSelfAsync(It.IsAny<UserOrganisation>(), It.IsAny<string>()))
                .Callback(
                    (UserOrganisation uo, string abe) => {
                        calledLogUnregisteredSelfAsync = true;
                        Assert.AreEqual(uo, testUserOrg, "Expected to log user org");
                        Assert.AreEqual(abe, testUnregisterUser.EmailAddress, "Expected log action by email to match");
                    })
                .Returns(Task.CompletedTask);

            // Act
            await testRegistrationRepo.RemoveRegistrationAsync(testUserOrg, testUnregisterUser);

            // Assert user org removed
            Assert.IsNull(mockDataRepo.GetAll<UserOrganisation>().Where(uo => uo == testUserOrg).FirstOrDefault());

            // Assert log
            Assert.IsTrue(calledLogUnregisteredSelfAsync, "Expected LogUnregisteredSelfAsync to be called");
        }

    }

}
