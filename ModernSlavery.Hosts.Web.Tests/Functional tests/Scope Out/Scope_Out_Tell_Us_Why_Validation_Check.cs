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
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Fixing")]

    class Scope_Out_Tell_Us_Why_Validation_Check : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Tell_Us_Why_Validation_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            TestData = new OrganisationTestData(this);
        }
        private bool TestRunFailed = false;
        protected Organisation org;
        [OneTimeSetUp]


        public void OTSetUp()
        {
            //(TestRunSetup.TestWebHost.GetDbContext() as Microsoft.EntityFrameworkCore.DbContext)

            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null);
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)

        }
        [SetUp]
        public void SetUp()
        {
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
        [Test, Order(20)]
        public async Task SetSecurityCode()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);


            await this.SetSecurityCode(org, new DateTime(2021, 6, 10));


            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            Goto(ScopeConstants.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(org.OrganisationReference);
            Set("Security Code").To(org.SecurityCode);
            await Task.CompletedTask;
        }

        [Test, Order(26)]
        public async Task SubmittingIndentityFormLeadsToConfirmOrgDetails()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask;
        }
        [Test, Order(28)]
        public async Task VerifyOrgDetails()
        {
            RightOfText("Organisation Name").Expect(org.OrganisationName);
            RightOfText("Organisation Reference").Expect(org.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");
            await Task.CompletedTask;
        }

        [Test, Order(30)]
        public async Task ContinueonVerifyDetailsLeadsToTelUsWhy()
        {
            Click("Confirm and Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Tell us why your organisation is not required to publish a modern slavery statement");


            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task MustSelectAReason()
        {

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            Expect("The following errors were detected");
            //error message tbc
            Expect("Please select at least one reason why");
            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task DetailFieldsAreMandatory()
        {

            ClickLabel("Its turnover or budget is less than £36 million per year");
            ClickLabel("Other");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            Expect("The following errors were detected");
            //error message tbc
            ExpectNo("Please select at least one reason why");

            Expect("Please provide your turnover");
            Expect("You must provide your other reason for being out of scope");

            Set("What is your organisation's annual turnover or ?").To("5");

            await Task.CompletedTask;
        }

        [Test, Order(36)]
        public async Task PleaseSpecifyMustBe200CharsOrLess()
        {
            Set("What is your organisation's annual turnover or ?").To("5");

            Set("Please specify").To("Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five 1");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            Expect("The following errors were detected");
            //error message tbc
            Expect("More than 200 characters entered");

            Set("Please specify").To("Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Check your information before sending");
            await Task.CompletedTask;
        }
    }
}
