using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]

    public class Scope_Out_Infomation_Not_Correct : UITest
    {
        private string EmployerReference;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestData.Organisation = TestRunSetup.TestWebHost
                .Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope));
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        }

        private bool CanBeSetOutOfScope(Organisation org)
            => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope);

        [Test, Order(18)]
        public async Task SetScope()
        {

            await OrganisationHelper.GetOrganisationBusinessLogic(TestRunSetup.TestWebHost).CreateOrganisationSecurityCodeAsync(Registration.Organisation.OrganisationReference, new DateTime(2030, 1, 10));




            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(ScopeConstants.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(EmployerReference);
            Set("Security Code").To("ABCD1234");
            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task SubmittingIndentityFormLeadsToConfirmOrgDetails()
        {
            Click("Continue");
            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask;
        }
               

        [Test, Order(38)]
        public async Task CheckIncorrectDetailsLink()
        {
            //info not correct, click link
            ExpectLink("modernslaverystatements@homeoffice.gov.uk");

            ExpectXPath("//a[@href = 'modernslaverystatements@homeoffice.gov.uk'");

            await Task.CompletedTask;
        }

    }
}