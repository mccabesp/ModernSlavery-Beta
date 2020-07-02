﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Areas.Account.ViewModels.CloseAccount;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Account.CloseAccountController
{

    public class CloseAccountTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController.CloseAccount));
            mockRouteData.Values.Add("Controller", "CloseAccount");
        }

        [Test]
        public void GET_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.CloseAccount();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [TestCase(23322, true)]
        [TestCase(21555, false)]
        public void GET_SoleRegistrationFlagIsSavedInViewModel(long testUserId, bool expectedToBeSoleUser)
        {
            // Arrange
            object[] registations = UserOrganisationHelper.CreateRegistrations();

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    testUserId,
                    mockRouteData,
                    registations);

            // Act
            var viewResult = controller.CloseAccount() as ViewResult;

            // Assert
            Assert.NotNull(viewResult);

            var actualViewModel = viewResult.Model as CloseAccountViewModel;
            Assert.NotNull(actualViewModel);
            Assert.AreEqual(expectedToBeSoleUser, actualViewModel.IsSoleUserOfOneOrMoreOrganisations);
        }

        [Test]
        public async Task POST_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = await controller.CloseAccount(new CloseAccountViewModel());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_FailsWhenCurrentPasswordIsWrong()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var testPassword = "Password123";
            var testSalt = "testSalt";
            verifiedUser.Salt = testSalt;
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(testSalt));
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            // Act
            var viewResult = await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = "WrongPassword123"}) as ViewResult;

            // Assert
            Assert.NotNull(viewResult, "Expected a ViewResult");
            Assert.AreEqual(nameof(ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController.CloseAccount), viewResult.ViewName);
            Assert.AreEqual(
                "Could not verify your current password",
                viewResult.ViewData.ModelState["CurrentPassword"].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task POST_RetiresUserAndSignsOut()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var testPassword = "Password123";
            var testSalt = "testSalt";
            verifiedUser.Salt = testSalt;
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(verifiedUser.Salt));
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            // Act
            var signOutResult = await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = testPassword}) as SignOutResult;

            // Assert
            Assert.NotNull(signOutResult, "Expected a SignOutResult");
            Assert.AreEqual(UserStatuses.Retired, verifiedUser.Status, "Expected status to be retired");
        }

        [Test]
        public async Task POST_RemovesAllRetiredUserRegistrations()
        {
            // Arrange
            var testPassword = "ad5bda75-e514-491b-b74d-4672542cbd15";
            object[] registrations = UserOrganisationHelper.CreateRegistrationsInScope();

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    23322,
                    mockRouteData,
                    registrations);

            // check we start with the expected amount
            Assert.AreEqual(2, controller.CurrentUser.UserOrganisations.Count, "Expected to start with 2 registrations");

            // Act
            await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = testPassword});

            // Assert user org removed
            Assert.AreEqual(0, controller.CurrentUser.UserOrganisations.Count, "Expected no registrations after closing account");
        }

        [Test]
        public async Task POST_SendsAccountClosedNotification()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var testPassword = "Password123";
            var testSalt = "testSalt";
            verifiedUser.Salt = testSalt;
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(verifiedUser.Salt));
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            var mockEmailQueue = new Mock<IQueue>();

            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<QueueWrapper>()));

            // Act
            await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = testPassword});

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<QueueWrapper>(
                        inst => inst.Message.Contains(verifiedUser.EmailAddress)
                                && inst.Type == typeof(CloseAccountCompletedTemplate).FullName)),
                Times.Once(),
                $"Expected the users email address using {nameof(CloseAccountCompletedTemplate)} to be in the email send queue");

            mockEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<QueueWrapper>(
                        inst => inst.Message.Contains(ConfigHelpers.EmailOptions.GEODistributionList) && inst.Type == typeof(OrphanOrganisationTemplate).FullName)),
                Times.Never,
                $"Didnt expect the GEO Email addresses using {nameof(OrphanOrganisationTemplate)} to be in the email send queue");
        }

        [Test]
        public async Task POST_SendsAccountClosedNotifications()
        {
            // Arrange
            object[] registrations = UserOrganisationHelper.CreateRegistrationsInScope();

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    23322,
                    mockRouteData,
                    registrations);
            var verifiedUser = controller.SharedBusinessLogic.DataRepository.Get<User>((long) 23322);

            var mockEmailQueue = new Mock<IQueue>();

            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<QueueWrapper>()));

            // Act
            await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = "ad5bda75-e514-491b-b74d-4672542cbd15"});

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<QueueWrapper>(
                        inst => inst.Message.Contains(verifiedUser.EmailAddress)
                                && inst.Type == typeof(CloseAccountCompletedTemplate).FullName)),
                Times.Once(),
                $"Expected the users email address using {nameof(CloseAccountCompletedTemplate)} to be in the email send queue");

            mockEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<QueueWrapper>(
                        inst => inst.Message.Contains(ConfigHelpers.EmailOptions.GEODistributionList) && inst.Type == typeof(OrphanOrganisationTemplate).FullName)),
                Times.Once(),
                $"Expected the GEO Email addresses using {nameof(OrphanOrganisationTemplate)} to be in the email send queue");
        }

        [Test]
        public void ThrowsArgumentExceptionWhenUserIsNotActiveAndRollsback()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var testPassword = "Password123";
            var testSalt = "testSalt";
            verifiedUser.Salt = testSalt;
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testPassword, Convert.FromBase64String(testSalt));
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;

            // create an exception in the user repo
            verifiedUser.Status = UserStatuses.Unknown;

            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.CloseAccountController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            int expectedUserOrgCount = controller.CurrentUser.UserOrganisations.Count;

            // Act
            var actualException = Assert.ThrowsAsync<ArgumentException>(
                async () => await controller.CloseAccount(new CloseAccountViewModel {EnterPassword = testPassword}));

            // Assert
            Assert.AreEqual(
                $"Can only retire active users. UserId={verifiedUser.UserId}",
                actualException.Message,
                "Expected expetion message to match");
            Assert.AreEqual(
                expectedUserOrgCount,
                controller.CurrentUser.UserOrganisations.Count,
                "Expected registrations to still exist after close account fails");
        }

    }

}
