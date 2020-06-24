﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Tests.TestHelpers;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Organisation
{

    public partial class OrganisationControllerTests
    {

        [Test(Author = "Oscar Lagatta")]
        [Description("ManageOrganisationsController.Home GET: When Creates New Model")]
        public void OrganisationController_Manageorganisations_GET_When_Creates_New_Model()
        {
            // ARRANGE
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            mockUser.UserSettings = new[] {new UserSetting(UserSettingKeys.AcceptedPrivacyStatement, VirtualDateTime.Now.ToString())};

            Core.Entities.Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            Core.Entities.Organisation mockOrg2 = OrganisationHelper.GetPublicOrganisation();
            Core.Entities.Organisation mockOrg3 = OrganisationHelper.GetPublicOrganisation();

            UserOrganisation mockUserOrg1 = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);
            UserOrganisation mockUserOrg2 = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg2);
            UserOrganisation mockUserOrg3 = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg3);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "ManageOrganisations");
            routeData.Values.Add("Controller", "Registrations");

            var controller = UiTestHelper.GetController<ManageOrganisationsController>(
                -1,
                routeData,
                mockUser,
                mockOrg,
                mockOrg2,
                mockOrg3,
                mockUserOrg1,
                mockUserOrg2,
                mockUserOrg3);

            // Acts
            var result = controller.Home() as ViewResult;

            object actualUserOrganisationViewModel = result.Model;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(actualUserOrganisationViewModel);
            Assert.AreEqual(result.ViewName, "ManageOrganisations");
        }
        
        [Test]
        [Description(
            "Organisation.ManageOrganisations GET: When UserSettings.AcceptedPrivacyStatement is Latest Then Redirect to ReadPrivacyStatement")]
        public void OrganisationController_ManageOrganisations_GET_When_UserSettings_AcceptedPrivacyStatement_is_Latest_Then_Return()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisations");
            mockRouteData.Values.Add("Controller", "Registrations");

            var mockUser = new User {
                UserId = 87654,
                EmailAddress = "mock@test.com",
                EmailVerifiedDate = VirtualDateTime.Now,
                UserSettings = new[] {new UserSetting(UserSettingKeys.AcceptedPrivacyStatement, VirtualDateTime.Now.ToString())}
            };

            var controller = UiTestHelper.GetController<ManageOrganisationsController>(-1, mockRouteData, mockUser);

            // Acts
            var result = controller.Home() as RedirectToActionResult;

            // Assert
            Assert.IsNull(result, "RedirectToActionResult should be null");
            Assert.AreNotEqual(result?.ActionName, "ReadPrivacyStatement", "Expected the Action NOT to be 'ReadPrivacyStatement'");
            Assert.AreNotEqual(result?.ControllerName, "Organisation", "Expected the Controller NOT to be 'Home'");
        }

        [Test]
        [Description(
            "ManageOrganisationsController.Home GET: When UserSettings.AcceptedPrivacyStatement is Null Then Redirect to ReadPrivacyStatement")]
        public void OrganisationController_ManageOrganisations_GET_When_UserSettings_AcceptedPrivacyStatement_is_Null_Then_Return()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisations");
            mockRouteData.Values.Add("Controller", "Registrations");

            var mockUser = new User {
                UserId = 87654, EmailAddress = "mock@test.com", EmailVerifiedDate = VirtualDateTime.Now, UserSettings = new UserSetting[0]
            };

            var controller = UiTestHelper.GetController<ManageOrganisationsController>(-1, mockRouteData, mockUser);

            // Acts
            var result = controller.Home() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "PrivacyPolicy", "Expected the Action to be 'PrivacyPolicy'");
            Assert.AreEqual(result.ControllerName, "Home", "Expected the Controller to be 'Home'");
        }

        [Test]
        [Description(
            "ManageOrganisationsController.Home GET: When UserSettings.AcceptedPrivacyStatement is Older Then Redirect to ReadPrivacyStatement")]
        public void OrganisationController_ManageOrganisations_GET_When_UserSettings_AcceptedPrivacyStatement_is_Older_Then_Return()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "Home");
            mockRouteData.Values.Add("Controller", "ManageOrganisations");

            var mockUser = new User {
                UserId = 87654,
                EmailAddress = "mock@test.com",
                EmailVerifiedDate = VirtualDateTime.Now,
                UserSettings = new[] {new UserSetting(UserSettingKeys.AcceptedPrivacyStatement, VirtualDateTime.Now.AddYears(-10).ToString())}
            };

            var controller = UiTestHelper.GetController<ManageOrganisationsController>(-1, mockRouteData, mockUser);

            // Acts
            var result = controller.Home() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "PrivacyPolicy", "Expected the Action to be 'ReadPrivacyStatement'");
            Assert.AreEqual(result.ControllerName, "Home", "Expected the Controller to be 'Home'");
        }

    }

}
