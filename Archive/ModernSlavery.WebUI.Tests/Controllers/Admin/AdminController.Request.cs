﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Admin.Controllers;
using ModernSlavery.WebUI.Registration.Controllers;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Services;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Admin
{
    public partial class AdminControllerTests
    {

        [Test]
        [Description("Check manual registration completes successfully on admin approval")]
        public async Task AdminController_ReviewRequest_POST_ManualRegistration_ServiceActivated()
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
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                AddressId = address.AddressId,
                Address = address,
                User = user,
                Organisation = org
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegistrationController.OrganisationType));
            routeData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<AdminController>(user.UserId, routeData, user, org, address, userOrg);

            var model = new OrganisationViewModel {ReviewCode = userOrg.GetReviewCode()};
            controller.StashModel(model);

            //ACT:
            var result = await controller.ReviewRequest(model, "approve") as RedirectToActionResult;

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "RequestAccepted", "Expected redirect to RequestAccepted");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.LatestAddress.AddressId == address.AddressId);
            Assert.That(!string.IsNullOrWhiteSpace(userOrg.Organisation.EmployerReference));
            Assert.That(address.Status == AddressStatuses.Active);

            //Check the organisation exists in search
            EmployerSearchModel actualIndex = await controller.AdminService.SearchBusinessLogic.EmployerSearchRepository.GetAsync(org.OrganisationId.ToString());
            EmployerSearchModel expectedIndex = EmployerSearchModel.Create(org);
            expectedIndex.Compare(actualIndex);
        }
        [Test]
        [Description("RegistrationController POST: When User Added To Public Organisation Then Email Existing Users")]
        public async Task RegistrationController_POST_When_User_Added_To_Public_Organisation_Then_Email_Existing_Users()
        {
            // Arrange
            var organisationId = 100;
            Core.Entities.Organisation organisation = Registration.RegistrationControllerTests.createPublicOrganisation(organisationId, "Company1", 12345678);
            User existingUser1 = Registration.RegistrationControllerTests.CreateUser(1, "user1@test.com");
            User existingUser2 = Registration.RegistrationControllerTests.CreateUser(2, "user2@test.com");
            User newUser = Registration.RegistrationControllerTests.CreateUser(3, "user3@test.com");
            UserOrganisation existingUserOrganisation1 = Registration.RegistrationControllerTests.CreateUserOrganisation(organisation, existingUser1.UserId, VirtualDateTime.Now);
            UserOrganisation existingUserOrganisation2 = Registration.RegistrationControllerTests.CreateUserOrganisation(organisation, existingUser2.UserId, VirtualDateTime.Now);
            UserOrganisation newUserOrganisation = Registration.RegistrationControllerTests.CreateUserOrganisation(organisation, newUser.UserId, VirtualDateTime.Now);
            newUserOrganisation.PINConfirmedDate = null;
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            govEqualitiesOfficeUser.EmailVerifiedDate = VirtualDateTime.Now;

            var routeData = new RouteData();
            routeData.Values.Add("Action", "ReviewRequest");
            routeData.Values.Add("Controller", "Registration");

            var controller = UiTestHelper.GetController<AdminController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                existingUser1,
                existingUser2,
                newUser,
                existingUserOrganisation1,
                existingUserOrganisation2,
                newUserOrganisation,
                govEqualitiesOfficeUser);

            var testModel = new OrganisationViewModel { ReviewCode = newUserOrganisation.GetReviewCode() };
            controller.StashModel(testModel);

            var mockNotifyEmailQueue = new Mock<IQueue>();
            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<SendEmailRequest>()));

            // Act
            await controller.ReviewRequest(testModel, "approve");

            //ASSERT:
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.EmailAddress.Contains(existingUser1.EmailAddress))),
                Times.Once(),
                "Expected the existingUser1's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.EmailAddress.Contains(existingUser2.EmailAddress))),
                Times.Once(),
                "Expected the existingUser2's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.TemplateId.Contains(EmailTemplates.UserAddedToOrganisationEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.UserAddedToOrganisationEmail}");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.EmailAddress.Contains(newUser.EmailAddress))),
                Times.Never,
                "Do not expect new user's email address to be in the email send queue");
        }

    }
}
