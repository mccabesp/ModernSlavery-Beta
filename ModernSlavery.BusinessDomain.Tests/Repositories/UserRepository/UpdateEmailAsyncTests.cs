using System;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Tests.Common;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Repositories.UserRepository
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateEmailAsyncTests : BaseTestFixture<UpdateEmailAsyncTests.DependencyModule>
    {
        [SetUp]
        public void BeforeEach()
        {
            // mock data 
            mockDataRepo = new Mock<IDataRepository>().SetupGetAll(UserHelpers.CreateUsers());

            mockUserLogRecord = new Mock<IUserLogger>();

            // service under test
            testUserRepo =
                new ModernSlavery.Infrastructure.Database.Classes.UserRepository(new DatabaseOptions(), new SharedOptions(),
                    mockDataRepo.Object, mockUserLogRecord.Object, DependencyContainer.Resolve<IMapper>());
        }

        public class DependencyModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                // Initialise AutoMapper
                var mapperConfig = new MapperConfiguration(config =>
                {
                    config.AddMaps(typeof(ModernSlavery.Infrastructure.Database.Classes.UserRepository));
                });
                builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();
            }
        }

        private Mock<IDataRepository> mockDataRepo;
        private Mock<IUserLogger> mockUserLogRecord;

        private IUserRepository testUserRepo;

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
            "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com")]
        public async Task SavesExpectedEmailFields(string testCurrentEmail, string testNewEmail)
        {
            // Arrange
            var saveChangesCalled = false;
            var testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);

            // pretend user email was last verified 7 days ago
            testUserToUpdate.EmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);

            mockDataRepo.Setup(x => x.SaveChangesAsync())
                .Callback(() => saveChangesCalled = true)
                .Returns(Task.CompletedTask);

            // Act
            await testUserRepo.UpdateEmailAsync(testUserToUpdate, testNewEmail);

            // Assert
            Assert.IsTrue(saveChangesCalled, "Expected SaveChangesAsync to be called");
            Assert.AreEqual(testNewEmail, testUserToUpdate.EmailAddress, "Expected to change email");
            Assert.Zero(
                VirtualDateTime.Now.Subtract(testUserToUpdate.EmailVerifiedDate.Value).Minutes,
                "Expected to change email verify date");
        }

        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.New)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Unknown)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Suspended)]
        [TestCase("active1@ad5bda75-e514-491b-b74d-4672542cbd15.com", UserStatuses.Retired)]
        public async Task ThrowsErrorWhenUserStatusIsNotActive(string testCurrentEmail, UserStatuses testStatus)
        {
            // Arrange
            var testEmailVerifiedDate = VirtualDateTime.Now.Date.AddDays(-7);

            var testUserToUpdate = await testUserRepo.FindByEmailAsync(testCurrentEmail);
            testUserToUpdate.Status = testStatus;
            testUserToUpdate.EmailVerifiedDate = testEmailVerifiedDate;

            // Act
            var actualException = Assert.ThrowsAsync<ArgumentException>(
                async () => await testUserRepo.UpdateEmailAsync(testUserToUpdate, "change@email.com"));

            // Assert
            Assert.AreEqual(
                "Can only update emails for active users. UserId=23322",
                actualException.Message,
                "Expected exception message to match");
            Assert.AreEqual(testCurrentEmail, testUserToUpdate.EmailAddress,
                "Expected email address to still be the same");
            Assert.AreEqual(
                testEmailVerifiedDate,
                testUserToUpdate.EmailVerifiedDate.Value,
                "Expected email verify date to still be the same");
            Assert.AreEqual(testStatus, testUserToUpdate.Status, "Expected status to still be the same");
        }

        private static object[] ShouldThrowErrorWhenArgumentIsNullCases =
        {
            new object[]
            {
                null, "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                "Value cannot be null.\r\nParameter name: userToUpdate"
            },
            new object[] {new User(), null, "Value cannot be null.\r\nParameter name: newEmailAddress"},
            new object[] {new User(), "", "Value cannot be null.\r\nParameter name: newEmailAddress"},
            new object[] {new User(), " ", "Value cannot be null.\r\nParameter name: newEmailAddress"}
        };

        [TestCaseSource(nameof(ShouldThrowErrorWhenArgumentIsNullCases))]
        public void ThrowsErrorWhenArgumentIsNull(User testUserToUpdateArg, string testNewEmailAddressArg,
            string expectedErrorMessage)
        {
            // Act
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await testUserRepo.UpdateEmailAsync(testUserToUpdateArg, testNewEmailAddressArg));

            // Assert
            Assert.AreEqual(expectedErrorMessage, actualException.Message, "Expected exception message to match");
        }
    }
}