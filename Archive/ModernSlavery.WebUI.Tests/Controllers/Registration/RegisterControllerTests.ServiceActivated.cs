﻿using System;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using NUnit.Framework;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Registration.Controllers;
using ModernSlavery.WebUI.Registration.Models;

namespace ModernSlavery.WebUI.Tests.Controllers.Registration
{
    [TestFixture]
    public partial class RegistrationControllerTests
    {

        [Test]
        [Description("Check registration completes successfully when correct pin entered ")]
        public async Task RegistrationController_ActivateService_POST_CorrectPIN_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var org = new Core.Entities.Organisation {OrganisationId = 1, SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending};

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org.OrganisationScopes.Add(
                new OrganisationScope {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.InScope,
                    SnapshotDate = org.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                    Status = ScopeRowStatuses.Active
                });

            var address = new OrganisationAddress {AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending};
            var pin = "ASDFG";
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now,
                PIN = pin,
                AddressId = address.AddressId,
                Address = address
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegistrationController.ActivateService));
            routeData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<RegistrationController>(user.UserId, routeData, user, org, address, userOrg);
            controller.ReportingOrganisationId = org.OrganisationId;

            var model = new CompleteViewModel {PIN = pin};

            //ACT:
            var result = await controller.ActivateService(model) as RedirectToActionResult;

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.LatestAddress.AddressId == address.AddressId);
            Assert.That(address.Status == AddressStatuses.Active);

            //Check the organisation exists in search
            EmployerSearchModel actualIndex = await controller.RegistrationService.SearchBusinessLogic.EmployerSearchRepository.GetAsync(org.OrganisationId.ToString());
            EmployerSearchModel expectedIndex = EmployerSearchModel.Create(org);
            expectedIndex.Compare(actualIndex);
        }

        [Test]
        [Ignore("Needs fixing/deleting")]
        [Description("RegistrationController.ServiceActivated GET: When OrgScope is Not Null Then Return Expected ViewData")]
        public void RegistrationController_ServiceActivated_GET_When_OrgScope_is_Not_Null_Then_Return_Expected_ViewData()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Registration");

            var mockOrg = new Core.Entities.Organisation {
                OrganisationId = 52425, SectorType = SectorTypes.Private, OrganisationName = "Mock Organisation Ltd"
            };

            var mockUser = new User {UserId = 87654, EmailAddress = "mock@test.com", EmailVerifiedDate = VirtualDateTime.Now};

            var mockReg = new UserOrganisation {UserId = 87654, OrganisationId = 52425, PINConfirmedDate = VirtualDateTime.Now};

            var controller = UiTestHelper.GetController<RegistrationController>(87654, mockRouteData, mockUser, mockOrg, mockReg);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            // Act
            var viewResult = controller.ServiceActivated() as ViewResult;

            // Assert
            Assert.NotNull(viewResult, "ViewResult should not be null");
            Assert.AreEqual(viewResult.ViewName, "ServiceActivated", "Expected the ViewName to be 'ServiceActivated'");

            // Assert ViewData
            Assert.That(controller.ViewBag.OrganisationName == mockOrg.OrganisationName, "Expected OrganisationName");
        }

    }
}
