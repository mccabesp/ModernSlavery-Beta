﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Account.Resources;
using ModernSlavery.WebUI.Account.ViewModels.ChangeDetails;
using ModernSlavery.WebUI.Tests.TestHelpers;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Account.ChangeDetailsController
{

    public class ChangeDetailsTests
    {

        private RouteData mockRouteData;

        [SetUp]
        public void BeforeEach()
        {
            mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController.ChangeDetails));
            mockRouteData.Values.Add("Controller", "ChangeDetails");
        }

        [Test]
        public void GET_RedirectUnauthorizedUsersToSignIn()
        {
            // Arrange
            User unverifiedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    0,
                    mockRouteData,
                    unverifiedUser);

            // Act
            IActionResult actionResult = controller.ChangeDetails();

            // Assert
            Assert.NotNull(actionResult);
            Assert.IsInstanceOf(typeof(ChallengeResult), actionResult);
        }

        [Test]
        public void GET_ReturnsChangeDetailsViewWhenUserIsAuthorized()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            // Act
            var viewResult = controller.ChangeDetails() as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.IsInstanceOf(typeof(ChangeDetailsViewModel), viewResult.Model);

            var actualModel = (ChangeDetailsViewModel) viewResult.Model;
            actualModel.Compare(verifiedUser, caseSensitive: false);
        }

        [Test]
        public async Task POST_ReturnsChangeDetailsViewWhenModelStateIsInValid()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);
            var model = new ChangeDetailsViewModel();

            // Act
            controller.ModelState.AddModelError("FirstName", "Required");
            var viewResult = await controller.ChangeDetails(model) as ViewResult;

            // Assert
            Assert.NotNull(viewResult);
            Assert.AreEqual(
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController.ChangeDetails),
                viewResult.ViewName);
            Assert.IsFalse(controller.TempData.ContainsKey(AccountResources.ChangeDetailsSuccessAlert));
            Assert.IsInstanceOf(typeof(ChangeDetailsViewModel), viewResult.Model);
        }

        [Test]
        public async Task POST_SavesChangeDetailsModelToUserEntity()
        {
            // Arrange
            User verifiedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller =
                UiTestHelper.GetController<ModernSlavery.WebUI.Areas.Account.Controllers.ChangeDetailsController>(
                    verifiedUser.UserId,
                    mockRouteData,
                    verifiedUser);

            var testViewModel = new ChangeDetailsViewModel {
                FirstName = $"NewFirstName{verifiedUser.UserId}",
                LastName = $"NewLastName{verifiedUser.UserId}",
                JobTitle = $"NewJobTitle{verifiedUser.UserId}",
                ContactPhoneNumber = $"NewContactPhoneNumber{verifiedUser.UserId}",
                AllowContact = !verifiedUser.AllowContact,
                SendUpdates = !verifiedUser.SendUpdates
            };

            // Act
            var redirectToActionResult = await controller.ChangeDetails(testViewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(redirectToActionResult);
            Assert.AreEqual(
                nameof(ModernSlavery.WebUI.Areas.Account.Controllers.AccountController.ManageAccount),
                redirectToActionResult.ActionName);

            // Assert success flag
            Assert.IsTrue(controller.TempData.ContainsKey(nameof(AccountResources.ChangeDetailsSuccessAlert)));

            // Assert user details
            testViewModel.Compare(verifiedUser);

            // Assert contact point details
            Assert.AreEqual(verifiedUser.ContactFirstName, testViewModel.FirstName, "Expected ContactFirstName to match");
            Assert.AreEqual(verifiedUser.ContactLastName, testViewModel.LastName, "Expected ContactLastName to match");
            Assert.AreEqual(verifiedUser.ContactJobTitle, testViewModel.JobTitle, "Expected ContactJobTitle to match");
        }

    }

}
