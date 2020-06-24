using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.WebUI.Admin.Controllers;
using ModernSlavery.WebUI.Shared.Services;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Admin
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AdminUnconfirmedPinsControllerTests
    {

        private static UserOrganisation CreateUserOrganisation(Core.Entities.Organisation org, long userId, DateTime? pinConfirmedDate)
        {
            return new UserOrganisation {
                Organisation = org, UserId = userId, PINConfirmedDate = pinConfirmedDate, Address = new OrganisationAddress()
            };
        }

        private static Core.Entities.Organisation createOrganisation(long organisationId, string organisationName, int companyNumber)
        {
            return new Core.Entities.Organisation {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                SectorType = SectorTypes.Private, /**/
                Status = OrganisationStatuses.Active,
                CompanyNumber = companyNumber.ToString()
            };
        }

        private static User CreateUser(long userId, string emailAddress)
        {
            return new User {
                UserId = userId,
                EmailAddress = emailAddress,
                Firstname = "FirstName" + userId,
                Lastname = "LastName" + userId,
                EmailVerifiedDate = VirtualDateTime.Now,
                Status = UserStatuses.Active
            };
        }

        [Test]
        [Description("AdminUnconfirmedPinsController POST: When PIN has expired create new PIN and send email")]
        public async Task AdminUnconfirmedPinsController_POST_When_PIN_has_expired_create_new_PIN_and_send_email()
        {
            // Arrange

            // This PIN format is intentionally different for testing purposes to make sure
            // it doesn't clash with the new PIN that is generated
            var expiredPIN = "1111";

            var organisationId = 100;
            Core.Entities.Organisation organisation = createOrganisation(organisationId, "Company1", 12345678);
            User user = CreateUser(1, "user1@test.com");
            UserOrganisation userOrganisation = CreateUserOrganisation(organisation, user.UserId, VirtualDateTime.Now);
            userOrganisation.PIN = expiredPIN;
            userOrganisation.PINSentDate = VirtualDateTime.Now.AddYears(-1);

            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            govEqualitiesOfficeUser.EmailVerifiedDate = VirtualDateTime.Now;

            var routeData = new RouteData();
            routeData.Values.Add("Action", "SendPin");
            routeData.Values.Add("Controller", "AdminUnconfirmedPinsController");

            var controller = UiTestHelper.GetController<AdminUnconfirmedPinsController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser);

            var mockNotifyEmailQueue = new Mock<IQueue>();

            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<SendEmailRequest>()));

            // Act
            await controller.SendPin(user.UserId, organisationId);

            // Assert
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.EmailAddress.Contains(user.EmailAddress))),
                Times.Once(),
                "Expected the user's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.TemplateId.Contains(EmailTemplates.SendPinEmail))),
                Times.Once,
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.SendPinEmail}");

            // Check that a new PIN has been created
            Assert.That(userOrganisation.PIN, Is.Not.EqualTo(expiredPIN));
            Assert.NotNull(userOrganisation.PIN);

            // Check that the PINSentDate has been updated
            Assert.That(userOrganisation.PINSentDate.Value.Date, Is.EqualTo(VirtualDateTime.Now.Date));
        }
        
        [Test]
        [Description("AdminUnconfirmedPinsController POST: When PIN is still valid send email")]
        public async Task AdminUnconfirmedPinsController_POST_When_PIN_is_still_valid_send_email()
        {
            // Arrange
            
            var organisationId = 100;
            var pin = "6A519E7";
            Core.Entities.Organisation organisation = createOrganisation(organisationId, "Company1", 12345678);
            User user = CreateUser(1, "user1@test.com");
            UserOrganisation userOrganisation = CreateUserOrganisation(organisation, user.UserId, VirtualDateTime.Now);
            userOrganisation.PIN = pin;
            userOrganisation.PINSentDate = VirtualDateTime.Now.AddDays(-1);

            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            govEqualitiesOfficeUser.EmailVerifiedDate = VirtualDateTime.Now;

            var routeData = new RouteData();
            routeData.Values.Add("Action", "SendPin");
            routeData.Values.Add("Controller", "AdminUnconfirmedPinsController");

            var controller = UiTestHelper.GetController<AdminUnconfirmedPinsController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser);

            var mockNotifyEmailQueue = new Mock<IQueue>();

            mockNotifyEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<SendEmailRequest>()));

            // Act
            await controller.SendPin(user.UserId, organisationId);

            // Assert
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.EmailAddress.Contains(user.EmailAddress))),
                Times.Once(),
                "Expected the user's email address to be in the email send queue");
            mockNotifyEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<SendEmailRequest>(inst => inst.TemplateId.Contains(EmailTemplates.SendPinEmail))),
                Times.Once,
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.SendPinEmail}");

            // Check that the same PIN has been used
            Assert.That(userOrganisation.PIN, Is.EqualTo(pin));

            // Check that the PINSentDate has been updated
            Assert.That(userOrganisation.PINSentDate.Value.Date, Is.EqualTo(VirtualDateTime.Now.Date));
        }

    }
}
