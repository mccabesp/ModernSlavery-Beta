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
    [TestFixture]

    public class Footer_Check : BaseUITest
    {
        public Footer_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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

        [Test, Order(2)]
        public async Task ReturnToRootPage()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(4)]
        public async Task CheckLinkVisibility()
        {
            ExpectLink("Contact us");
            ExpectLink("Cookies");
            ExpectLink("Privacy Policy");



            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(6)]
        public async Task CheckLinkNavigation()
        {
            ClickLink("Contact us");
            ExpectHeader("Contact us");
            ClickLink("Cookies");
            ExpectHeader("Cookies on the Modern slavery statement registry");

            ClickLink("Privacy Policy");
            ExpectHeader("Privacy Policy");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

}