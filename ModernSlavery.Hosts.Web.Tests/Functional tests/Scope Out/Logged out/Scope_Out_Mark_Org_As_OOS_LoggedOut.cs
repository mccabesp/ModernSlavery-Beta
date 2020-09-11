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

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]

    public class Scope_Out_Mark_Org_As_OOS_LoggedOut : UITest
    {
       protected string EmployerReference;

        [Test, Order(20)]
        public async Task SetSecurityCode()
        {
            var result = Testing.Helpers.Extensions.OrganisationHelper.GetSecurityCodeBusinessLogic(TestRunSetup.TestWebHost).CreateSecurityCode(TestData.Organisation, new DateTime(2021, 6, 10));

            if (result.Failed)
            {
                throw new Exception("Unable to set security code");
            }

            await Testing.Helpers.Extensions.OrganisationHelper.SaveAsync(TestRunSetup.TestWebHost);

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
            ExpectHeader("Check your answers before sending");
            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task CheckDetails()
        {
            RightOfText("Organisation Name").Expect(TestData.OrgName);
            RightOfText("Organisatin Reference").Expect(TestData.Organisation.OrganisationReference);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");

            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Here are the reasons why.");
            RightOfText("Contact name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last) ;
            //todo await helper implementation for address logic
            RightOfText("Job title").Expect("Create_Account.roger_job_title");
            RightOfText("Contact email").Expect(Create_Account.roger_email);

            ClickLabel("I would like to receive a confirmation email");

            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task ConfirmAndSendLeadsToConfirmationPage()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task CompletePageContentCheck()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");

            Expect("You have declared your organisation is not required to publish a modern slavery statement");

            Expect("We have sent you a confirmation email. We will contact you if we need more information.");

            ExpectHeader("Produced a statement voluntarily?");
            Expect("If you are not legally required to publish a modern slavery statement, but have produced one voluntarily, you can still submit it to our service.");
            Expect(What.Contains, "To submit a modern slavery statement to our service, ");
            ExpectLink(That.Contains, "create an account");
            Expect(What.Contains, " and register your organisation.");
            await Task.CompletedTask;
        }
    }
}