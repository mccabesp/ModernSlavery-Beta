using System;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.BusinessLogic.Abstractions;
using ModernSlavery.BusinessLogic.Account.Models;
using ModernSlavery.BusinessLogic.LogRecords;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Tests.Common;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using ModernSlavery.Entities.Enums;

using NUnit.Framework;

namespace Repositories.UserRepository
{

    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateDetailsAsyncTests: BaseTestFixture<UpdateDetailsAsyncTests.DependencyModule>
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

            mockLogRecordLogger = new Mock<IUserLogRecord>();

            // service under test
            testUserRepo = new ModernSlavery.BusinessLogic.Account.Repositories.UserRepository(mockDataRepo.Object, mockLogRecordLogger.Object, DependencyContainer.Resolve<IMapper>());
        }

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IUserLogRecord> mockLogRecordLogger;
        private IUserRepository testUserRepo;

        [TestCase]
        public async Task SavesNewDetails()
        {
            // Arrange
            var saveChangesCalled = false;
            User testUserToUpdate = await testUserRepo.FindByEmailAsync("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com");

            var testUserDetails = new UpdateDetailsModel {
                FirstName = "NewFirstName",
                LastName = "NewLastName",
                JobTitle = "NewJobTitle",
                ContactPhoneNumber = "NewContactPhoneNumber",
                AllowContact = !testUserToUpdate.AllowContact,
                SendUpdates = !testUserToUpdate.SendUpdates
            };

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await testUserRepo.UpdateDetailsAsync(testUserToUpdate, testUserDetails);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            testUserDetails.Compare(testUserToUpdate);
        }

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Suspended)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            User testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            testUserToUpdate.Status = testStatus;

            // Act
            var actualException = Assert.ThrowsAsync<ArgumentException>(
                async () => await testUserRepo.UpdateDetailsAsync(testUserToUpdate, new UpdateDetailsModel()));

            // Assert
            Assert.AreEqual(
                "Can only update details for active users. UserId=23322",
                actualException.Message,
                "Expected exception message to match");
            Assert.AreEqual(testStatus, testUserToUpdate.Status, "Expected status to still be the same");
        }

        private static object[] ThrowsErrorWhenArgumentIsNullCases = {
            new object[] {null, null, "Value cannot be null.\r\nParameter name: userToUpdate"},
            new object[] {new User(), null, "Value cannot be null.\r\nParameter name: changeDetails"}
        };

        [TestCaseSource(nameof(ThrowsErrorWhenArgumentIsNullCases))]
        public void ThrowsErrorWhenArgumentIsNull(User testUserToUpdateArg,
            UpdateDetailsModel testChangeDetailsArg,
            string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await testUserRepo.UpdateDetailsAsync(testUserToUpdateArg, testChangeDetailsArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }

    }

}
