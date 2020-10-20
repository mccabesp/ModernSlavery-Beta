using Geeks.Pangolin;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class Create_Account_Content_Check : BaseUITest
    {
        public Create_Account_Content_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

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
            DeleteCookiesAndReturnToRoot(this);

            Click("Sign in");

            await AxeHelper.CheckAccessibilityAsync(this);

            BelowHeader("No account yet?");
            Click("Create an account");
            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Create an Account");

            await Task.CompletedTask;

        }
        [Test, Order(2)]
        public async Task Expect_Email_Fields()

        {
            ExpectHeader("Email address");
            BelowHeader("Email address").Expect("Enter an email address that you can access. The service will send you an email to verify your identity.");
            ExpectField("Email address");
            ExpectField("Confirm your email address");

            await Task.CompletedTask;

        }


        [Test, Order(3)]
        public async Task Expect_Your_Details_Fields()

        {
            ExpectHeader("Your details");
            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            await Task.CompletedTask;

        }

        [Test, Order(4)]
        public async Task Expect_Create_Password_Validation_Rules()

        {
            ExpectHeader("Create a password");
            Expect(What.Contains, "Your password must be at least 8 characters long.");
            //splitting due to underlined text
            Expect(What.Contains, "It must also have at least one of ");
            Expect(What.Contains, "each");
            Expect(What.Contains," of the following:");

            Expect(What.Contains, "lower-case letter");
            Expect(What.Contains, "capital letter");
            Expect(What.Contains, "number");
          


            ExpectField("Password");
            ExpectField("Confirm password");

            await Task.CompletedTask;
        }

        [Test, Order(6)]
        public async Task Expect_Create_Account_Terms_And_Conditions()

        {
            ExpectLabel("I would like to receive information about webinars, events and new guidance");
            ExpectLabel("I'm happy to be contacted for feedback on this service and take part in surveys about modern slavery");

            await Task.CompletedTask;
        }

        [Test, Order(8)]
        public async Task Expect_Continue_Creation_Button()
        {
            ExpectButton("Continue");
            await Task.CompletedTask;
        }
    } 
    
}