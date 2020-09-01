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

    public class Scope_Out_Mark_Org_As_OOS_LoggedIn : Private_Registration_Success
    {

       
        [Test, Order(30)]
        public async Task GoToManageOrgPage()
        {
            Goto("/");

            Click("Manage organisations");
            ExpectHeader(That.Contains, "Select an organisation");

            await Task.CompletedTask;
        }

        [Test, Order(31)]
        public async Task SelectOrg()
        {
            Click("Interfloor Limited");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "Yes");
            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task ChangeOrgStatus()
        {
            Click("Change");
            ExpectHeader("Tell us why your organisation is not required to publish a modern slavery statement");
            await Task.CompletedTask;
        }

        //[Test, Order(36)]
        //public async Task SubmittingIndentityFormLeadsToConfirmOrgDetails()
        //{
        //    Click("Continue");
        //    ExpectHeader("Confirm your organisation’s details");
        //    await Task.CompletedTask;
        //}

        //[Test, Order(38)]
        //public async Task VerifyOrgDetails()
        //{
        //    RightOfText("Name").Expect(Submission.OrgName_InterFloor);
        //    RightOfText("Reference").Expect(EmployerReference);
        //    //todo await helper implementation for address logic
        //    RightOfText("Registered address").Expect("");
        //    await Task.CompletedTask;
        //}

        //[Test, Order(30)]
        //public async Task ContinueonVerifyDetailsLeadsToTelUsWhy()
        //{
        //    Click("Confirm and Continue");

        //    ExpectHeader("Tell us why your organisation is not required to publish a modern slavery statement");


        //    await Task.CompletedTask;
        //}

        [Test, Order(33)]
        public async Task SelectingOtherOptionMakesPleaseSpecifyFieldAppear()
        {
            ExpectNo("Please specify");

            ClickLabel("Other");

            Expect(What.Contains, "Please specify");
            ExpectField("OtherReason");

            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task EnterOtherDetails()
        {
            Set("OtherReason").To("Here are the reasons why.");

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
            ExpectHeader("Organisation details");

            RightOfText("Name").Expect(Submission.OrgName_InterFloor);
            RightOfText("Reference").Expect(Submission.EmployerReference_InterFloor);
            //todo await helper implementation for address logic
            RightOfText("Registered address").Expect("");

            ExpectHeader("Declaration");
            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Other");
            RightOfText("Contact name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last);
            //todo await helper implementation for address logic
            RightOfText("Job title").Expect("Create_Account.roger_job_title");
            RightOfText("Contact email").Expect(Create_Account.roger_email);

            await Task.CompletedTask;
        }

        [Test, Order(40)]
        public async Task ConfirmAndSendLeadsToConfirmationPage()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task CompletePageContentCheck ()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");

            Expect("You have declared your organisation is not required to publish a modern slavery statement");

            Expect("We have sent you a confirmation email. We will contact you if we need more information.");

            ExpectHeader("Produced a statement voluntarily?");
            Expect("If you are not legally required to publish a modern slavery statement, but have produced one voluntarily, you can still submit it to our service.");
            Expect(What.Contains, "To submit a modern slavery statement to our service, ");
            ExpectLink(That.Contains,"create an account");
            Expect(What.Contains, " and register your organisation.");
            await Task.CompletedTask;
        }
    }
}