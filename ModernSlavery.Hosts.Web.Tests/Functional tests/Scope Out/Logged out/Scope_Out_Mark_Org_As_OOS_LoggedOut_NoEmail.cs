﻿using Geeks.Pangolin;
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
    public class Scope_Out_Mark_Org_As_OOS_LoggedOut_NoEmail : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Mark_Org_As_OOS_LoggedOut_NoEmail() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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

            var result = this.GetSecurityCodeBusinessLogic().CreateSecurityCode(org, new DateTime(2021, 6, 10));

            if (result.Failed)
            {
                throw new Exception("Unable to set security code");
            }

            await this.SaveDatabaseAsync().ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(TestData.ScopeUrl);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
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
        public async Task SelectingOtherOptionMakesPleaseSpecifyFieldAppear()
        {
            ExpectNo("Please specify");

            ClickLabel("Other");

            Expect("Please specify");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(34)]
        public async Task EnterOtherDetails()
        {
            Set("OtherReason").To("Here are the reasons why.");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(36)]
        public async Task EnterContactDetails()
        {
            BelowLabel("First name").Set(The.Top).To(Create_Account.roger_first);
            BelowLabel("Last name").Set(The.Top).To(Create_Account.roger_last);
            BelowLabel("Job title").Set(The.Top).To(Create_Account.roger_job_title);
            BelowLabel("Email address").Set(The.Top).To(Create_Account.roger_email);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(38)]
        public async Task ContinueOnTellUsWhyFormLeadsToCheckYourAnswers()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Check your answers before sending");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(40)]
        public async Task CheckDetails()
        {
            RightOfText("Organisation Name").Expect(org.OrganisationName);
            RightOfText("Organisation Reference").Expect(org.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");

            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Here are the reasons why.");




            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(42)]
        public async Task ConfirmAndSendLeadsToConfirmationPage()
        {
            Click("Confirm and send");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Declaration complete");
            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(44)]
        public async Task CompletePageContentCheck()
        {

            Expect("You have declared your organisation is not required to publish a modern slavery statement on your website");


            ExpectHeader("Publishing a statement voluntarily");
            Expect("If you are not legally required to publish a modern slavery statement on your website, you can still create a statement voluntarily and submit it to our service.");
            Expect(What.Contains, "To submit a modern slavery statement to our service, ");
            ExpectLink(That.Contains, "create an account");
            Expect(What.Contains, " and register your organisation.");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}