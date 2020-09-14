using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Extensions;
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


    public class Scope_Out_Mark_Org_As_OOS_LoggedIn : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public Scope_Out_Mark_Org_As_OOS_LoggedIn() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await OrganisationHelper.RegisterUserOrganisationAsync(TestRunSetup.TestWebHost, TestData.OrgName, UniqueEmail);
        }

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

            Click(TestData.OrgName);
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
        //    RightOfText("Name").Expect(TestData.OrgName);
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

            //Expect(What.Contains, "Please specify");
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

            RightOfText("Organisation Reference").Expect(Registration.Organisation.OrganisationReference);
            RightOfText("Organisation Name").Expect(TestData.OrgName);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");

            ExpectHeader("Declaration");
            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Here are the reasons why.");


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
        public async Task CompletePageContentCheck()
        {

            Expect("You have declared your organisation is not required to publish a modern slavery statement on your website");

            Expect("We've sent a confirmation email to you and any other user associated with this organisation on our system. We will contact you if we need more information.");

            ExpectHeader("Publishing a statement voluntarily");
            Expect("If you are not legally required to publish a modern slavery statement on your website, you can still create a statement voluntarily and submit it to our service.");

            ClickText("Continue");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            await Task.CompletedTask;
        }
    }
}