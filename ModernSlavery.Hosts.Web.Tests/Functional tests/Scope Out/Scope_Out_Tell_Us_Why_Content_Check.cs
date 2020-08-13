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

    class Scope_Out_Tell_Us_Why_Content_Check : UITest
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
        public async Task CheckReasonsWhy()
        {
            Try(
                 () => ExpectLabel("Its turnover or budget is less than £36 million per year"),
            () => { ExpectLabel("It does not provide goods or services"); },
            () => { ExpectLabel("It does not have a business presence in the UK"); },
            () => { ExpectLabel("It is in administration or liquidation, has closed or is dormant, or has merged with another organisation"); },
            () => { ExpectLabel("Other"); });

            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task CheckFieldVisibility()
        {
            ExpectNoField("What is your organistion’s annual turnover or budget?");
            ClickLabel("Its turnover or budget is less than £36 million per year");
            ExpectField("What is your organistion’s annual turnover or budget?");

            ExpectNoField("Please specify");
            ExpectNo("(Limit is 200 characters)");
            ClickLabel("Other");
            ExpectField("Please specify");
            ExpectNo("(Limit is 200 characters)");

            await Task.CompletedTask;
        }

        [Test, Order(36)]
        public async Task CheckContactUsFields()
        {
            ExpectHeader("Your contact details");

            Try(
                 () => ExpectField("First name"),
            () => { ExpectField("Last name"); },
            () => { ExpectField("Job title"); },
            () => { ExpectField("Email address"); });

            await Task.CompletedTask;
        }

        [Test, Order(38)]
        public async Task CheckGuidanceText()
        {
            Expect(What.Contains, "Guidance");
            Expect(What.Contains, " is available to help you work out whether your organisation is required to publish a modern slavery statement.");

            await Task.CompletedTask;
        }
    }
}
