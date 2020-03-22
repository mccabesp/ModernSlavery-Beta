using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class SecurityCodeBusinessLogicTests
    {
        [TestCase(OrganisationStatuses.Active, false)]
        [TestCase(OrganisationStatuses.Pending, false)]
        [TestCase(OrganisationStatuses.Deleted, true)]
        [TestCase(OrganisationStatuses.Retired, true)]
        [TestCase(OrganisationStatuses.New, true)]
        [TestCase(OrganisationStatuses.Suspended, true)]
        [TestCase(OrganisationStatuses.Unknown, true)]
        public void SecurityCodeBusinessLogic_CreateSecurityCode_Only_Applies_To_Active_Or_Pending(
            OrganisationStatuses organisationStatus,
            bool expectedToFail)
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation {Status = organisationStatus};
            var securityCodeExpiryDateTime = VirtualDateTime.Now.AddDays(7);

            // Act
            var creationResult =
                securityCodeBusinessLogic.CreateSecurityCode(organisation, securityCodeExpiryDateTime);

            // Assert
            var wasWasnt = expectedToFail ? "was" : "was not";
            var didDidnt = expectedToFail ? "didn't" : "did";
            Assert.AreEqual(
                expectedToFail,
                creationResult.Failed,
                $"Organisation status '{organisationStatus.ToString()}' {wasWasnt} expected to fail, but it {didDidnt}.");

            if (expectedToFail)
                Assert.AreEqual(
                    $"Generation of security codes cannot be performed for retired organisations. Organisation '' employerReference '' has status '{organisationStatus.ToString()}'.",
                    creationResult.ErrorMessage.Description);
        }

        [Test]
        public void SecurityCodeBusinessLogic_CreateSecurityCode_Must_Expire_In_The_Future()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation {Status = OrganisationStatuses.Active};
            var securityCodeExpiryDateTime = VirtualDateTime.Now.AddDays(-1);

            // Act
            var creationResult =
                securityCodeBusinessLogic.CreateSecurityCode(organisation, securityCodeExpiryDateTime);
            Assert.True(creationResult.Failed);
            Assert.Null(creationResult.Result);

            var actualError = creationResult.ErrorMessage;

            // Assert
            Assert.NotNull(actualError);
            Assert.AreEqual(4004, actualError.Code);
            Assert.AreEqual("Security code must expire in the future", actualError.Description);
        }

        [Test]
        public void SecurityCodeBusinessLogic_CreateSecurityCode_Succeeds()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation {Status = OrganisationStatuses.Active};
            var securityCodeExpiryDateTime = VirtualDateTime.Now.AddDays(1);

            // Act
            var creationResult =
                securityCodeBusinessLogic.CreateSecurityCode(organisation, securityCodeExpiryDateTime);
            Assert.True(creationResult.Succeeded);

            var modifiedOrganisation = creationResult.Result;

            // Assert
            Assert.NotNull(modifiedOrganisation.SecurityCode);
            Assert.AreEqual(securityCodeExpiryDateTime, modifiedOrganisation.SecurityCodeExpiryDateTime);
            Assert.NotNull(modifiedOrganisation.SecurityCodeCreatedDateTime);
        }

        [Test]
        public void SecurityCodeBusinessLogic_CreateSecurityCode_Throws_Exception()
        {
            // Arrange
            var mockedOrganisationToSetup = new Mock<Organisation>();
            mockedOrganisationToSetup
                .Setup(x => x.SetSecurityCode(It.IsAny<DateTime>()))
                .Throws(new Exception("Kaboom"));

            var mockedOrganisation = mockedOrganisationToSetup.Object;
            mockedOrganisation.Status = OrganisationStatuses.Active;

            // Act
            var creationResult =
                new SecurityCodeBusinessLogic().CreateSecurityCode(mockedOrganisation, VirtualDateTime.Now.AddDays(10));

            Assert.True(creationResult.Failed);
            Assert.Null(creationResult.Result);

            var actualError = creationResult.ErrorMessage;

            // Assert
            Assert.NotNull(actualError);
            Assert.AreEqual(0, actualError.Code);
            Assert.AreEqual("Kaboom", actualError.Description);
        }

        [Test]
        public void SecurityCodeBusinessLogic_ExpireSecurityCode_Cannot_Expire_An_Already_Expired_Organisation()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation {OrganisationId = 123456789};
            organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(1));
            organisation.SetSecurityCodeExpiryDate(VirtualDateTime.Now.AddDays(-1));

            // Act
            var creationResult = securityCodeBusinessLogic.ExpireSecurityCode(organisation);

            // Assert
            Assert.True(creationResult.Failed);
            Assert.AreEqual(
                "Cannot modify the security code information of an already expired security code",
                creationResult.ErrorMessage.Description);
            Assert.AreEqual(4004, creationResult.ErrorMessage.Code);
            Assert.Null(creationResult.Result);
            Assert.AreEqual(123456789, creationResult.ErrorRelatedObject.OrganisationId);
        }

        [Test]
        public void SecurityCodeBusinessLogic_ExpireSecurityCode_Cannot_Extend_An_Already_Expired_Organisation()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation {OrganisationId = 9988776655};
            organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(1));
            organisation.SetSecurityCodeExpiryDate(VirtualDateTime.Now.AddDays(-1));

            // Act
            var creationResult =
                securityCodeBusinessLogic.ExtendSecurityCode(organisation, VirtualDateTime.Now.AddDays(7));

            // Assert
            Assert.True(creationResult.Failed);
            Assert.AreEqual(
                "Cannot modify the security code information of an already expired security code",
                creationResult.ErrorMessage.Description);
            Assert.AreEqual(4004, creationResult.ErrorMessage.Code);
            Assert.Null(creationResult.Result);
            Assert.AreEqual(9988776655, creationResult.ErrorRelatedObject.OrganisationId);
        }

        [Test]
        public void SecurityCodeBusinessLogic_ExpireSecurityCode_Succeeds()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation();
            organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(1));

            // Act
            var creationResult = securityCodeBusinessLogic.ExpireSecurityCode(organisation);
            Assert.True(creationResult.Succeeded);

            var modifiedOrganisation = creationResult.Result;

            // Assert
            Assert.NotNull(modifiedOrganisation.SecurityCode);
            Assert.AreEqual(
                VirtualDateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                modifiedOrganisation.SecurityCodeExpiryDateTime?.ToString("dd/MM/yyyy"),
                "Security codes are expired by changing their expiry date to yesterday");
            Assert.NotNull(modifiedOrganisation.SecurityCodeCreatedDateTime);
        }

        [Test]
        public void SecurityCodeBusinessLogic_ExtendSecurityCode_Succeeds()
        {
            // Arrange
            var securityCodeBusinessLogic = new SecurityCodeBusinessLogic();
            var organisation = new Organisation();
            organisation.SetSecurityCode(VirtualDateTime.Now.AddDays(1));
            var securityCodeNewExpiryDateTime = VirtualDateTime.Now.AddDays(1);

            // Act
            var creationResult =
                securityCodeBusinessLogic.ExtendSecurityCode(organisation, securityCodeNewExpiryDateTime);
            Assert.True(creationResult.Succeeded);

            var modifiedOrganisation = creationResult.Result;

            // Assert
            Assert.NotNull(modifiedOrganisation.SecurityCode);
            Assert.AreEqual(securityCodeNewExpiryDateTime, modifiedOrganisation.SecurityCodeExpiryDateTime);
            Assert.NotNull(modifiedOrganisation.SecurityCodeCreatedDateTime);
        }
    }
}