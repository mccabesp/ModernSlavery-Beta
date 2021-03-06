﻿using Geeks.Pangolin;
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
    public abstract class CreateAccount : BaseUITest
    {
        public CreateAccount() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }

        string _firstname; string _lastname; string _title; string _email; string _password;
        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;
        private string URL;
        public string UniqueEmail;
        protected readonly OrganisationTestData TestData;
        public CreateAccount(string firstname, string lastname, string title, string email, string password) : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {
            UniqueEmail = email + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            _firstname = firstname; _lastname = lastname; _title = title; _email = UniqueEmail; _password = password;

            TestData = new OrganisationTestData(this);
        }

        private bool TestRunFailed = false;

        [SetUp]
        public void SetUp()
        {
            if (TestRunFailed)
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        }

        [Test, Order(1)]
        public async Task GoToCreateAccountPage()
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

        [Test, Order(2)]
        public async Task EnterPersonalDetails()
        {
            Set("Email address").To(_email);
            Set("Confirm your email address").To(_email);

            Set("First name").To(_firstname);
            Set("Last name").To(_lastname);
            Set("Job title").To(_title);

            Set("Password").To(_password);
            Set("Confirm password").To(_password);

            ClickLabel("I would like to receive information on resources and guidance relating to modern slavery");
            ClickLabel("I am happy to be contacted for feedback on this service");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(3)]
        public async Task ClickingContinueNavigatesToVerification()
        {
            Click("Continue");
            ExpectHeader("Verify your email address");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(4)]
        public async Task ExtractVerifyURL()
        {

            Expect(What.Contains, "We have sent a confirmation email to");
            Expect(What.Contains, _email);
            Expect(What.Contains, "Follow the instructions in the email to finish creating your account.");


            //get email verification link
            URL = WebDriver.FindElement(By.LinkText(_email)).GetAttribute("href");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(5)]
        public async Task VerifyEmail()
        {
            //verify email
            Goto(URL);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Set("Email").To(_email);
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