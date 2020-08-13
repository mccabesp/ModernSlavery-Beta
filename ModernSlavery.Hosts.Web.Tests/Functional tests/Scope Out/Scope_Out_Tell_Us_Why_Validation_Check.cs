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

namespace ModernSlavery.Hosts.Web.Tests.Functional_tests.Scope_Out
{
    [TestFixture, Ignore("Awaiting Scope Merge")]

    class Scope_Out_Tell_Us_Why_Validation_Check : UITest
    {
        protected string EmployerReference;

        [Test, Order(20)]
        public async Task AddOrgToDb()
        {
            //EmployerReference =  ModernSlavery.Testing.Helpers.Testing_Helpers.AddFastrackOrgToDB(Submission.OrgName_InterFloor, "ABCD1234");

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
        public async Task MustSelectAReason()
        {

            Click("Continue");

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

            Expect("The following errors were detected");
            //error message tbc
            Expect("More than 200 characters entered");

            Set("Please specify").To("Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five Five");

            Click("Continue");
            ExpectHeader("Check your information before sending");
            await Task.CompletedTask;
        }
    }
}
