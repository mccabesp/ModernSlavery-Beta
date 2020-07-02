﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.EmailTemplates;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Areas.Account.ViewModels.ChangeEmail;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Account.ChangeEmailController
{

    public class ChangeEmailTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController.ChangeEmail));
            mockRouteData.Values.Add("Controller", "ChangeEmail");
        }

        [Test]
        public void GET_RedirectsUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangeEmail();

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
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = await controller.ChangeEmail(new ChangeEmailViewModel());

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public async Task POST_SendsChangeEmailPendingVerification()
        {
            // Arrange
            var calledSendEmailQueue = false;
            var testNewEmail = "NewEmail@testemail.com";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            var mockEmailQueue = new Mock<IQueue>();
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<QueueWrapper>()))
                .Callback(
                    (QueueWrapper inst) => {
                        calledSendEmailQueue = true;
                        Assert.IsTrue(
                            inst.Message.Contains(testNewEmail),
                            "Expected the users email address to be in the email send queue");
                        Assert.AreEqual(
                            typeof(ChangeEmailPendingVerificationTemplate).FullName,
                            inst.Type,
                            $"Expected the {nameof(ChangeEmailPendingVerificationTemplate)} type to be in the email send queue");
                    });


            var model = new ChangeEmailViewModel {EmailAddress = testNewEmail, ConfirmEmailAddress = testNewEmail};

            // Act
            var redirectToActionResult = await controller.ChangeEmail(model) as RedirectToActionResult;

            // Assert
            Assert.IsTrue(calledSendEmailQueue, "Expected send email queue to be called");
            Assert.NotNull(redirectToActionResult);
            Assert.AreEqual(
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController.ChangeEmailPending),
                redirectToActionResult.ActionName);
        }

    }

}
