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

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture, Ignore("Need to review verif link visibility")]

    public class Scope_Out_Create_Account_After_Registration : Scope_Out_Mark_Org_As_OOS_LoggedOut
    {


        private string URL;

        [Test, Order(50)]
        public async Task ClickingCreateAccountLeadsToCreateAccountPage()
        {
              Click("Create an account");

            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Create an Account");

            await Task.CompletedTask;
        }


        [Test, Order(52)]
        public async Task EnterPersonalDetails()
        {
            Set("Email address").To(Create_Account.roger_email);
            Set("Confirm your email address").To(Create_Account.roger_email);

            Set("First name").To(Create_Account.roger_first);
            Set("Last name").To(Create_Account.roger_last);
            Set("Job title").To(Create_Account.roger_job_title);

            Set("Password").To(Create_Account.roger_password);
            Set("Confirm password").To(Create_Account.roger_password);

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in surveys about modern slavery");

            await Task.CompletedTask;
        }

        [Test, Order(54)]

        public async Task ClickingContinueNavigatesToVerification()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //ExpectHeader("Verify your email address");

            await Task.CompletedTask;
        }


        [Test, Order(56)]
        public async Task ExtractVerifyURL()
        {

            Expect(What.Contains, "We have sent a confirmation email to");
            Expect(What.Contains, Create_Account.roger_email);
            Expect(What.Contains, "Follow the instructions in the email to finish creating your account.");


            //get email verification link
            URL = WebDriver.FindElement(By.LinkText(Create_Account.roger_email)).GetAttribute("href");

            await Task.CompletedTask;

        }

        [Test, Order(58)]
        public async Task VerifyEmail()
        {

            //verify email
            Goto(URL);

            Set("Email").To(Create_Account.roger_email);
            Set("Password").To(Create_Account.roger_password);

            Click(The.Bottom, "Sign In");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("You've confirmed your email address");

            Expect("To finish creating your account, select continue.");
            await Task.CompletedTask;

        }

        [Test, Order(60)]
        public async Task ClickingContinueLeadsToConfirmOrgDetails()
        {
            Click("Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader(That.Contains,"Select an organisation");

            await Task.CompletedTask;

        }

    }
}