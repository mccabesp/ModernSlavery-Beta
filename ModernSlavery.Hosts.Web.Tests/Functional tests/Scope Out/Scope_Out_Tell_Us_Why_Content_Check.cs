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
    [TestFixture]

    class Scope_Out_Tell_Us_Why_Content_Check : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Tell_Us_Why_Content_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            TestData = new OrganisationTestData(this);
        }
        protected string EmployerReference;
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


            await this.SetSecurityCode(org, new DateTime(2021, 6, 10)).ConfigureAwait(false);


            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            Goto(ScopeConstants.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(24)]
        public async Task EnterEmployerReferenceAndSecurityCode()
        {
            Set("Organisation Reference").To(org.OrganisationReference);
            Set("Security Code").To(org.SecurityCode);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(26)]
        public async Task SubmittingIndentityFormLeadsToConfirmOrgDetails()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(28)]
        public async Task VerifyOrgDetails()
        {
            RightOfText("Organisation Name").Expect(org.OrganisationName);
            RightOfText("Organisation Reference").Expect(org.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(30)]
        public async Task ContinueonVerifyDetailsLeadsToTelUsWhy()
        {
            Click("Confirm and Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Tell us why your organisation is not required to publish a modern slavery statement");


            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task CheckReasonsWhy()
        {
            Try(
                 () => ExpectLabel("Its turnover or budget is less than £36 million per year"),
            () => { ExpectLabel("It does not provide goods or services"); },
            () => { ExpectLabel("It does not have a business presence in the UK"); },
            () => { ExpectLabel("It is in administration or liquidation, has closed or is dormant, or has merged with another organisation"); },
            () => { ExpectLabel("Other"); });

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(34)]
        public async Task CheckFieldVisibility()
        {
            ExpectNoLabel("What is your organisation’s annual turnover or budget?");
            ClickLabel("Its turnover or budget is less than £36 million per year");
            ExpectLabel("What is your organisation’s annual turnover or budget?");

            ExpectNoLabel("Please specify");
            ExpectNo("You have 1000 characters remaining");
            ClickLabel("Other");
            ExpectLabel("Please specify");
            Expect("You have 1000 characters remaining");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(36)]
        public async Task CheckContactUsFields()
        {
            Expect("Your contact details");

            Try(
                 () => ExpectLabel("First name"),
            () => { ExpectLabel("Last name"); },
            () => { ExpectLabel("Job title"); },
            () => { ExpectLabel("Email address"); });

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(38)]
        public async Task CheckGuidanceText()
        {
            Expect(What.Contains, "Guidance");
            Expect(What.Contains, " is available to help you work out whether your organisation is required to publish a modern slavery statement.");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
