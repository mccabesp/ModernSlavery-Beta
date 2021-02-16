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
    public class Create_Account_Password_Rules : BaseUITest
    {
        public Create_Account_Password_Rules() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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
            SignOutDeleteCookiesAndReturnToRoot(this);

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);


            Click("Sign in");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Click("Create an account");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Create an Account");
            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(2)]
        public async Task FillOutAccountCreationInformation()
        {
            Set("Email address").To("roger@test.co");
            Set("Confirm your email address").To("roger@test.co");

            Set("First name").To("Roger");
            Set("Last Name").To("Reporter");

            Set("Job title").To("Company Reporter");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(3)]
        public async Task ShortPassowrd()
        {

            Set("Password").To("test");
            Set("Confirm password").To("test");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("The following errors were detected");

            BelowLabel("Password").Expect("The Password must be at least 8 characters long.");

            await Task.CompletedTask.ConfigureAwait(false);

        }


        [Test, Order(4)]
        public async Task NoNumberOrCapital()
        {
            Set("Password").To("testtest");
            Set("Confirm password").To("testtest");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("The following errors were detected");

            BelowLabel("Password").Expect("Password must contain at least one upper case, 1 lower case character and 1 digit");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(5)]
        public async Task NoMatch()
        {
            Set("Password").To("Testtest1");
            Set("Confirm password").To("testtest");

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("The following errors were detected");

            BelowLabel("Password").Expect("The password and confirmation password do not match.");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}