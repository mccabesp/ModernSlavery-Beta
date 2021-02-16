using AngleSharp.Text;
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

    public class Manage_Cookies_Website_Usage : BaseUITest
    {
        public Manage_Cookies_Website_Usage() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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

        [Test, Order(11)]
        public async Task BannerTextCheck()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);
            Expect("The Modern slavery statement registry is an online service that uses cookies which are essential for the site to work. We also use non-essential cookies to help us improve government digital services. Any data collected is anonymised.");


            await Task.CompletedTask.ConfigureAwait(false);

        }
        [Test, Order(12)]
        public async Task Clicking_CookieSettings_Leads_To_Cookie_Settings()

        {
            Click("Cookie Settings");
            ExpectHeader("Cookies on Modern slavery statement registry");

            await Task.CompletedTask.ConfigureAwait(false);

        }


        [Test, Order(13)]
        public async Task Check_Initial_Cookies()

        {
            var Cookies = WebDriver.Manage().Cookies.AllCookies;
            //Assert.That(Cookies, Has.No.Member(""));
            //Assert.That(Cookies, Has.No.Member(""));
            //Assert.That(Cookies, Has.No.Member(""));
            //Assert.That(Cookies, Has.No.Member(""));


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(16)]
        public async Task Navigate_To_More_Info()

        {
            ClickLabel(The.Top, "On");

            Click("Confirm");
            Expect("Your cookie settings were saved");
            Goto("/");

            var Cookies = WebDriver.Manage().Cookies.AllCookies;

            string[] WebSiteUsageCookies = { "_ga", "_gid", "_gat", "_gat_govuk_shared" };

            foreach (var cookie in Cookies)
            {
                if (!WebSiteUsageCookies.Contains(cookie.Name))
                {
                    WebDriver.Manage().Cookies.DeleteCookieNamed(cookie.Name);

                }
            }

            RefreshPage();

            Cookies = WebDriver.Manage().Cookies.AllCookies;

            foreach (var Cookie in Cookies)
            {
                Assert.AreNotEqual(Cookie.Name, "_ga");
                Assert.AreNotEqual(Cookie.Name, "_gid");
                Assert.AreNotEqual(Cookie.Name, "_ga");
                Assert.AreNotEqual(Cookie.Name, "_gat");
                Assert.AreNotEqual(Cookie.Name, "_gat_govuk_shared");
            }


            await Task.CompletedTask.ConfigureAwait(false);

        }
    }

}