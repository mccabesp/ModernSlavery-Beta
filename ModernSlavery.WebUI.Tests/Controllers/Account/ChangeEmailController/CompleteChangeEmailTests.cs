using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using ModernSlavery.Tests.TestHelpers;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.BusinessLogic.Models.Account;
using ModernSlavery.Core.EmailTemplates;
using Moq;

using NUnit.Framework;

namespace Account.Controllers.ChangeEmailController
{

    public class CompleteChangeEmailAsyncTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add(
                "Action",
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController.CompleteChangeEmailAsync));
            mockRouteData.Values.Add("Controller", "ChangeEmail");
        }

        [Test]
        public async Task FailsWhenTokenExpired()
        {
            // Arrange
            var testNewEmail = "new@testemail.com";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            string expectedCurrentEmailAddress = verifiedUser.EmailAddress;

            string code = Encryption.EncryptModel(
                new ChangeEmailVerificationToken {
                    UserId = verifiedUser.UserId, NewEmailAddress = testNewEmail, TokenTimestamp = VirtualDateTime.Now.AddDays(-1)
                });

            // Act
            var viewResult = await controller.CompleteChangeEmailAsync(code) as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.AreEqual("ChangeEmailFailed", viewResult.ViewName);
            Assert.AreEqual(1, viewResult.ViewData.ModelState.ErrorCount);
            Assert.AreEqual(
                "Cannot complete the change email process because your verify url has expired.",
                viewResult.ViewData.ModelState[nameof(controller.CompleteChangeEmailAsync)].Errors[0].ErrorMessage);

            Assert.AreEqual(expectedCurrentEmailAddress, verifiedUser.EmailAddress, "Expected the email address not to change");
        }

        [Test]
        public async Task FailsWhenEmailOwnedByNewOrActiveUser()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            User existingUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            string testNewEmail = existingUser.EmailAddress;

            var controller = UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                verifiedUser.UserId,
                mockRouteData,
                verifiedUser,
                existingUser);

            string expectedCurrentEmailAddress = verifiedUser.EmailAddress;

            string code = Encryption.EncryptModel(
                new ChangeEmailVerificationToken {
                    UserId = verifiedUser.UserId, NewEmailAddress = testNewEmail, TokenTimestamp = VirtualDateTime.Now
                });

            // Act
            var viewResult = await controller.CompleteChangeEmailAsync(code) as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.AreEqual("ChangeEmailFailed", viewResult.ViewName);
            Assert.AreEqual(1, viewResult.ViewData.ModelState.ErrorCount);
            Assert.AreEqual(
                "Cannot complete the change email process because the new email address has been registered since this change was requested.",
                viewResult.ViewData.ModelState[nameof(controller.CompleteChangeEmailAsync)].Errors[0].ErrorMessage);

            Assert.AreEqual(expectedCurrentEmailAddress, verifiedUser.EmailAddress, "Expected the email address not to change");
        }

        [Test]
        public async Task UpdatesUserEmailAddress()
        {
            // Arrange
            var testNewEmail = "new@testemail.com";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            string code = Encryption.EncryptModel(
                new ChangeEmailVerificationToken {
                    UserId = verifiedUser.UserId, NewEmailAddress = testNewEmail, TokenTimestamp = VirtualDateTime.Now
                });

            // Act
            var viewResult = await controller.CompleteChangeEmailAsync(code) as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.AreEqual("ChangeEmailCompleted", viewResult.ViewName);

            Assert.AreEqual(testNewEmail, verifiedUser.EmailAddress, "Expected new email address to be saved");
        }

        [Test]
        public async Task SendsChangeEmailToOldAndNewEmailAddresses()
        {
            // Arrange
            var calledSendEmailQueue = false;
            var testNewEmail = "new@testemail.com";
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeEmailController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            string testOldEmail = verifiedUser.EmailAddress;

            string code = Encryption.EncryptModel(
                new ChangeEmailVerificationToken {
                    UserId = verifiedUser.UserId, NewEmailAddress = testNewEmail, TokenTimestamp = VirtualDateTime.Now
                });

            var mockEmailQueue = new Mock<IQueue>();
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<QueueWrapper>()))
                .Callback(
                    (QueueWrapper inst) => {
                        calledSendEmailQueue = true;

                        if (inst.Type == typeof(ChangeEmailCompletedNotificationTemplate).FullName)
                        {
                            Assert.IsTrue(
                                inst.Message.Contains(testOldEmail),
                                "Expected the users old email address to be in the email send queue");
                        }
                        else if (inst.Type == typeof(ChangeEmailCompletedVerificationTemplate).FullName)
                        {
                            Assert.IsTrue(
                                inst.Message.Contains(testNewEmail),
                                "Expected the users new email address to be in the email send queue");
                        }
                        else
                        {
                            Assert.Fail("Expected new and old emails to be queued");
                        }
                    });

            // Act
            var viewResult = await controller.CompleteChangeEmailAsync(code) as ViewResult;

            // Assert
            Assert.IsTrue(calledSendEmailQueue, "Expected send email queue to be called");
            Assert.NotNull(viewResult);
            Assert.AreEqual("ChangeEmailCompleted", viewResult.ViewName);
        }

    }

}
