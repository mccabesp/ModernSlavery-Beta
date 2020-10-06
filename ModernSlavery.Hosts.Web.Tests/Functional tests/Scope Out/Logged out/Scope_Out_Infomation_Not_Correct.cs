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
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]


    public class Scope_Out_Infomation_Not_Correct : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Infomation_Not_Correct() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            TestData = new OrganisationTestData(this);
        }

        private string EmployerReference;
private bool TestRunFailed = false;
        [OneTimeSetUp]
        
        

        [SetUp]
        public void SetUp()
        {
            
            TestData.Organisation = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope));
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
            if (TestRunFailed)
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }




        [TearDown]
        public void TearDown()
        {

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        }
        private bool CanBeSetOutOfScope(Organisation org)
            => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope);

        [Test, Order(20)]
        public async Task SetSecurityCode()
        {


            await this.SetSecurityCode(TestData.Organisation, new DateTime(2021, 6, 10));


            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(TestData.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(TestData.Organisation.OrganisationReference);
            Set("Security Code").To(TestData.Organisation.SecurityCode);
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

            ExpectXPath("//a[contains(@href, 'mailto:modernslaverystatements@homeoffice.gov.uk')]");

            await Task.CompletedTask;
        }

    }
}