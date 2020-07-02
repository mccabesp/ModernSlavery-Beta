﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangePassword;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Account.ChangePasswordController
{

    public class ChangePasswordTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add(
                "Action",
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangePasswordController.ChangePassword));
            mockRouteData.Values.Add("Controller", "ChangePassword");
        }

        [Test]
        public void GET_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangePassword();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = await controller.ChangePassword(new ChangePasswordViewModel());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_ChangesPasswordAndSendsCompletedNotification()
        {
            var testOldPassword = "OldPassword123";
            var testNewPassword = "NewPassword123";
            var salt = "TestSalt";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            verifiedUser.PasswordHash = Crypto.GetPBKDF2(testOldPassword, Convert.FromBase64String(salt));
            verifiedUser.Salt = salt;
            verifiedUser.HashingAlgorithm = HashingAlgorithm.PBKDF2;
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangePasswordController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            var mockEmailQueue = new Mock<IQueue>();
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<QueueWrapper>()));

            var model = new ChangePasswordViewModel {
                CurrentPassword = testOldPassword, NewPassword = testNewPassword, ConfirmNewPassword = testNewPassword
            };

            // Act
            var redirectToActionResult = await controller.ChangePassword(model) as RedirectToActionResult;

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(
                    It.Is<QueueWrapper>(
                        inst => inst.Message.Contains(verifiedUser.EmailAddress)
                                && inst.Type == typeof(ChangePasswordCompletedTemplate).FullName)),
                Times.Once(),
                $"Expected the users email address using {nameof(ChangePasswordCompletedTemplate)} to be in the email send queue");

            Assert.NotNull(redirectToActionResult);
            Assert.AreEqual(
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.AccountController.ManageAccount),
                redirectToActionResult.ActionName);

            Assert.AreEqual(controller.CurrentUser.PasswordHash, Crypto.GetPBKDF2(testNewPassword, Convert.FromBase64String(controller.CurrentUser.Salt)), "Expected password to be updated");
        }

    }

}
