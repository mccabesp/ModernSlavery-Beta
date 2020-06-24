using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Account.Models;
using ModernSlavery.WebUI.Registration.Controllers;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Registration
{

    [TestFixture]
    [SetCulture("en-GB")]
    public partial class RegistrationControllerTests : AssertionHelper
    {

        #region GET AboutYou()

        [Test]
        [Description("RegistrationController.AboutYou GET: When PendingFastrack Then ViewModel Should Contain Scope Contact Details")]
        public async Task
            RegistrationController_AboutYou_GET_When_PendingFastrack_Cookie_Then_ViewModel_Should_Contain_Scope_Contact_DetailsAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "AboutYou");
            mockRouteData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<RegistrationController>(0, mockRouteData);
            await controller.Cache.RemoveAsync($"{controller.HttpContext.GetUserHostAddress()}:lastFasttrackDate");

            // Ensure we call the scope service GetScopeFromFastTrackCode implementation
            Mock<IScopePresenter> mockScopeBL = Mock.Get(controller.ScopePresentation);
            mockScopeBL.CallBase = true;

            //Populate the PendingFasttrackCodes
            controller.PendingFasttrackCodes =
                "EmployerRef:SecurityCode:ExpectedContactFirstname:ExpectedContactLastname:ExpectedContactEmailAddress";

            // Act
            IActionResult actionResult = await controller.AboutYou();

            // Assert
            Assert.NotNull(actionResult, "ViewResult should not be null");

            var viewResult = actionResult as ViewResult;
            Assert.AreEqual(viewResult.ViewName, "AboutYou", "Expected the ViewName to be 'AboutYou'");

            var viewModel = (SignUpViewModel) viewResult.Model;
            Assert.AreEqual(viewModel.FirstName, "ExpectedContactFirstname", "Expected the FirstName to be 'ExpectedContactFirstname'");
            Assert.AreEqual(viewModel.LastName, "ExpectedContactLastname", "Expected the LastName to be 'ExpectedContactLastname'");
            Assert.AreEqual(
                viewModel.EmailAddress,
                "ExpectedContactEmailAddress",
                "Expected the EmailAddress to be 'ExpectedContactEmailAddress'");
        }

        #endregion

        #region POST AboutYou()

        [Test]
        [Ignore("msande")]
        [Description("RegistrationController.AboutYou POST: When PendingFastrack Then Save in the user settings")]
        public async Task RegistrationController_AboutYou_POST_When_PendingFastrack_Then_SaveInUserSettingsAsync()
        {
            // Arrange
            var user = new User {UserId = 0};

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "AboutYou");
            mockRouteData.Values.Add("Controller", "Registration");

            var mockViewModel = new SignUpViewModel {EmailAddress = "mock@test.com", Password = "12345678"};

            var controller = UiTestHelper.GetController<RegistrationController>(0, mockRouteData);

            // Ensure we call the scope service GetScopeFromFastTrackCode implementation
            Mock<IScopePresenter> mockScopeBL = Mock.Get(controller.ScopePresentation);
            mockScopeBL.CallBase = true;

            //Populate the PendingFasttrackCodes
            controller.PendingFasttrackCodes =
                "EmployerRef:SecurityCode:ExpectedContactFirstname:ExpectedContactLastname:ExpectedContactEmailAddress";

            // Act
            var result = await controller.AboutYou(mockViewModel) as RedirectToActionResult;
            object stashedModel = controller.Session[controller + ":Model"];

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "VerifyEmail", "Expected the Action to be 'VerifyEmail'");
            Assert.AreEqual(result.ControllerName, "Registration", "Expected the Controller to be 'Registration'");

            // Assert User Setting
            UserSetting userSetting = controller.SharedBusinessLogic.DataRepository.GetAll<User>()
                .FirstOrDefault()
                .UserSettings.FirstOrDefault(x => x.Key == UserSettingKeys.PendingFasttrackCodes);
            Assert.AreEqual(
                userSetting.Value,
                "EmployerRef:SecurityCode",
                "Expected the UserSetting to contain employer reference and security code");
        }

        #endregion

        #region GET OrganisationType()

        [Ignore("Not implemented")]
        [Test]
        [Description("RegistrationController.OrganisationType GET: When PendingFastrack Then start fasttrack registration")]
        public void RegistrationController_OrganisationType_GET_When_PendingFastrack_Then_StartFastTrackRegistration()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Confirm Organisation

        [Test(Author = "Oscar Lagatta")]
        [Description("Register Controller ConfirmOrganisation When User User Not Registered")]
        public async Task RegistrationController_ConfirmOrganisation_When_User_Not_RegisteredAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<RegistrationController>(-1, mockRouteData, mockUser);

            var result = await controller.ConfirmOrganisation() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel) result.Model).Title);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description(
            "RegistrationController.ConfirmOrganisation GET When Cannot Load Employers From Session Then Return Error View Model 1112")]
        public async Task RegistrationController_ConfirmOrganisation_When_Cannot_Load_Employers_From_Session_Then_Return_ErrorViewModelAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Core.Entities.Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockUserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Registration");

            Return mockReturn = ReturnHelper.GetNewReturnForOrganisationAndYear(mockUserOrg,ConfigHelpers.SharedOptions.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockOrg, mockReturn);

            var controller = UiTestHelper.GetController<RegistrationController>(-1, mockRouteData, mockUser, mockOrg, mockUserOrg, mockReturn);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();


            // ACT
            var result = await controller.ConfirmOrganisation() as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1112);
        }

        [Test]
        [Description("")]
        public async Task RegistrationController_ConfirmOrganisation_Get_SuccessAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Core.Entities.Organisation mockOrg = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockUserOrg = UserOrganisationHelper.LinkUserWithOrganisation(mockUser, mockOrg);

            Return mockReturn = ReturnHelper.GetNewReturnForOrganisationAndYear(mockUserOrg,ConfigHelpers.SharedOptions.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockOrg, mockReturn);

            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<RegistrationController>(-1, mockRouteData, mockUser, mockOrg, mockUserOrg, mockReturn);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            controller.StashModel(new OrganisationViewModel());

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            var result = await controller.ConfirmOrganisation() as ViewResult;

            Assert.NotNull(result);

            Assert.AreEqual("ConfirmOrganisation", result.ViewName);
        }

        // 

        #endregion

    }

}
