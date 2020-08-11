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

    [TestFixture, Ignore("Awaiting Scope Merge")]

    public class Scope_Out_Mark_Org_As_OOS_LoggedIn : CreateAccount
    {
        private string EmployerReference;
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Scope_Out_Mark_Org_As_OOS_LoggedIn() : base(_firstname, _lastname, _title, _email, _password)
        {
        }
        [Test, Order(20)]
        public async Task AddOrgToDb()
        {
            EmployerReference = ModernSlavery.Testing.Helpers.Testing_Helpers.AddFastrackOrgToDB(Submission.OrgName_InterFloor, "ABCD1234");

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
            Set("Employer Reference").To(EmployerReference);
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

        [Test, Order(28)]
        public async Task VerifyOrgDetails()
        {
            RightOfText("Name").Expect(Submission.OrgName_InterFloor);
            RightOfText("Reference").Expect(EmployerReference);
            //todo await helper implementation for address logic
            RightOfText("Registered address").Expect("");
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
            Set("Other").To("Here are the reasons why.");

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
            RightOfText("Name").Expect(Submission.OrgName_InterFloor);
            RightOfText("Reference").Expect(EmployerReference);
            //todo await helper implementation for address logic
            RightOfText("Registered address").Expect("");

            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Other");
            RightOfText("Contact name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last);
            //todo await helper implementation for address logic
            RightOfText("Job title").Expect("Create_Account.roger_job_title");
            RightOfText("Contact email").Expect(Create_Account.roger_email);

            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task ConfirmAndSendLeadsToConfirmationPage()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");
            await Task.CompletedTask;
        }
    }
}