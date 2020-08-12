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

    [TestFixture, Ignore("Awaiting Scope Merge, email functionality needs added")]

    public class Scope_Out_Infomation_Not_Correct : UITest
    {
        private string EmployerReference;

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

        [Test, Order(36)]
        public async Task EnterContactDetails()
        {
            Set("First name").To(Create_Account.roger_first);
            Set("Last name").To(Create_Account.roger_last);
            Set("Job title").To(Create_Account.roger_job_title);
            Set("Email address").To(Create_Account.roger_email);

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
            //info not correct, click link
            ExpectLink("modernslaverystatements@homeoffice.gov.uk");

            Click("modernslaverystatements@homeoffice.gov.uk");


            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task VerifyEmailTemplate()
        {
            //todo figure out how this can be tested
            throw new NotImplementedException();
        }
    }
}