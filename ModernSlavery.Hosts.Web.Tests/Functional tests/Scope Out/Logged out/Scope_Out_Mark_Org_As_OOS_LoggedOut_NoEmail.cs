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

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture, Ignore("Awaiting Scope Merge")]

    public class Scope_Out_Mark_Org_As_OOS_LoggedOut_NoEmail : UITest
    {
       protected string EmployerReference;

        [Test, Order(20)]
        public async Task AddOrgToDb()
        {
            //EmployerReference =  ModernSlavery.Testing.Helpers.Testing_Helpers.AddFastrackOrgToDB(TestData.OrgName, "ABCD1234");

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
            RightOfText("Name").Expect(TestData.OrgName);
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
        public async Task CheckDetails_DontAskForEmail()
        {
            ExpectHeader("Organisation details");
            RightOfText("Name").Expect(TestData.OrgName);
            RightOfText("Reference").Expect(EmployerReference);
            //todo await helper implementation for address logic
            RightOfText("Registered address").Expect("");

            ExpectHeader("Declaration");
            RightOfText("Reason your organisation is not required to publish a modern slavery statement on your website").Expect("Other");
            RightOfText("Contact name").Expect(Create_Account.roger_first + " " + Create_Account.roger_last) ;
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

        [Test, Order(42)]
        public async Task CompletePageContentCheck()
        {
            Click("Confirm and send");
            ExpectHeader("Declaration complete");

            Expect("You have declared your organisation is not required to publish a modern slavery statement");

            //shouldn't see we have sent an email text, only we will contact
            Expect("We will contact you if we need more information.");

            ExpectHeader("Produced a statement voluntarily?");
            Expect("If you are not legally required to publish a modern slavery statement, but have produced one voluntarily, you can still submit it to our service.");
            Expect(What.Contains, "To submit a modern slavery statement to our service, ");
            ExpectLink(That.Contains, "create an account");
            Expect(What.Contains, " and register your organisation.");
            await Task.CompletedTask;
        }
    }
}