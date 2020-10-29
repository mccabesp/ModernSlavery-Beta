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
    public class Scope_Out_Mark_Org_As_OOS_LoggedOut_NoEmail : BaseUITest
    {
        protected readonly OrganisationTestData TestData;
        public Scope_Out_Mark_Org_As_OOS_LoggedOut_NoEmail() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            TestData = new OrganisationTestData(this);
        }
        protected string EmployerReference;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestData.Organisation = this.GetOrganisation("00032539 Public Limited Company");
        }

        [Test, Order(20)]
        public async Task SetSecurityCode()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            var result = this.GetSecurityCodeBusinessLogic().CreateSecurityCode(TestData.Organisation, new DateTime(2021, 6, 10));

            if (result.Failed)
            {
                throw new Exception("Unable to set security code");
            }

            await this.SaveDatabaseAsync();

            await Task.CompletedTask;
        }

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(TestData.ScopeUrl);
            await AxeHelper.CheckAccessibilityAsync(this);
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
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Confirm your organisation’s details");
            await Task.CompletedTask;
        }

        [Test, Order(28)]
        public async Task VerifyOrgDetails()
        {
            RightOfText("Organisation Name").Expect(TestData.OrgName);
            RightOfText("Organisation Reference").Expect(TestData.Organisation.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");
            await Task.CompletedTask;
        }

        [Test, Order(30)]
        public async Task ContinueonVerifyDetailsLeadsToTelUsWhy()
        {
            Click("Confirm and Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Tell us why your organisation is not required to publish a modern slavery statement");


            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task SelectingOtherOptionMakesPleaseSpecifyFieldAppear()
        {
            ExpectNo("Please specify");

            ClickLabel("Other");

            Expect("Please specify");

            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task EnterOtherDetails()
        {
            Set("OtherReason").To("Here are the reasons why.");

            await Task.CompletedTask;
        }

        [Test, Order(36)]
        public async Task EnterContactDetails()
        {
            BelowLabel("First name").Set(The.Top).To(Create_Account.roger_first);
            BelowLabel("Last name").Set(The.Top).To(Create_Account.roger_last);
            BelowLabel("Job title").Set(The.Top).To(Create_Account.roger_job_title);
            BelowLabel("Email address").Set(The.Top).To(Create_Account.roger_email);

            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task ContinueOnTellUsWhyFormLeadsToCheckYourAnswers()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Check your answers before sending");
            await Task.CompletedTask;
        }

        [Test, Order(40)]
        public async Task CheckDetails()
        {
            RightOfText("Organisation Name").Expect(TestData.OrgName);
            RightOfText("Organisation Reference").Expect(TestData.Organisation.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");

            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Here are the reasons why.");




            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ConfirmAndSendLeadsToConfirmationPage()
        {
            Click("Confirm and send");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Declaration complete");
            await Task.CompletedTask;
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
            await Task.CompletedTask;
        }
    }
}