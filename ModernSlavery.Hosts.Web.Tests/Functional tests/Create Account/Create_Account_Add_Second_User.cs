using Geeks.Pangolin;
using Geeks.Pangolin.Service.DriverService;
using NUnit.Framework;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ModernSlavery.Infrastructure.Hosts;
using System.Diagnostics;
using System;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Core.Interfaces;
using NUnit.Framework.Interfaces;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Create_Account_Add_Second_User : CreateAccount
    {

        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private string SecondUrl;


        public Create_Account_Add_Second_User() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

         public string SecondEmail = Create_Account.second_email + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        [Test, Order(11)]
        public async Task ReturnToCreateAccountPage()
        {

            AppSettingHelper.SetShowEmailVerifyLink(TestRunSetup.TestWebHost, true);
            SignOutDeleteCookiesAndReturnToRoot(this);

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            RefreshPage();
            Click("Sign in");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Sign in or create an account");

            BelowHeader("No account yet?");
            Click("Create an account");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Create an Account");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(12)]
        public async Task EnterPersonalDetails_Second_User()
        {
            Set("Email address").To(SecondEmail);
            Set("Confirm your email address").To(SecondEmail);

            Set("First name").To(Create_Account.second_first);
            Set("Last name").To(Create_Account.second_last);
            Set("Job title").To(Create_Account.second_job_title);

            Set("Password").To(_password);
            Set("Confirm password").To(_password);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(13)]

        public async Task ClickingContinueNavigatesToVerification_Second_User()
        {
            Click("Continue");
            ExpectHeader("Verify your email address");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(14)]
        public async Task ExtractVerifyURL_Second_User()
        {

            Expect(What.Contains, "We have sent a confirmation email to");
            Expect(What.Contains, SecondEmail);
            Expect(What.Contains, "Follow the instructions in the email to finish creating your account.");


            //get email verification link
            SecondUrl = WebDriver.FindElement(By.LinkText(SecondEmail)).GetAttribute("href");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(15)]
        public async Task VerifyEmail_SecondUser()
        {

            //verify email
            Goto(SecondUrl);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Set("Email").To(SecondEmail);
            Set("Password").To(_password);

            Click(The.Bottom, "Sign In");


            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("You've confirmed your email address");

            Expect("To finish creating your account, select continue.");
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);


            ExpectHeader("Privacy Policy");

            Click("Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Register or select organisations you want to add statements for");


            await Task.CompletedTask.ConfigureAwait(false);

        }

    }
}