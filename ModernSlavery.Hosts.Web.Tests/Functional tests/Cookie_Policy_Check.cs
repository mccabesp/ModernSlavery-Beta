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

    public class Cookie_Policy_Check : BaseUITest
    {
        public Cookie_Policy_Check() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
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

        [Test, Order(1), NonParallelizable]
        public async Task BannerTextCheck()
        {
            SignOutDeleteCookiesAndReturnToRoot(this);
            Expect("The Modern slavery statement registry is an online service that uses cookies which are essential for the site to work. We also use non-essential cookies to help us improve government digital services. Any data collected is anonymised.");
            

            await Task.CompletedTask;

        }
        [Test, Order(2)]
        public async Task Clicking_CookieSettings_Leads_To_Cookie_Settings()

        {
            Click("Cookie Settings");
            ExpectHeader("Cookies on Modern slavery statement registry");            

            await Task.CompletedTask;

        }


        [Test, Order(3)]
        public async Task Verify_Text()

        {
            Try(
                    () => { Expect("Cookies are files saved on your phone, tablet or computer when you visit a website."); },
                    () => { Expect("We use cookies to store information about how you use our service, such as the pages you visit."); },
                    () => { ExpectHeader("Cookie settings"); },
                    () => { BelowHeader("Cookie settings").Expect("We use 4 types of cookie. You can choose which cookies you're happy for us to use."); },
                    () => { ExpectHeader("Cookies used to improve the Modern slavery statement registry"); },
                    () => { BelowHeader("Cookies used to improve the Modern slavery statement registry").Expect("We use Google Analytics to measure how you use the service so we can improve it based on user needs. Google Analytics sets cookies that store anonymised information about:"); },
                    () => { BelowHeader("Cookies used to improve the Modern slavery statement registry").Expect("how you got to the site"); },
                    () => { BelowHeader("Cookies used to improve the Modern slavery statement registry").Expect("the pages you visit on the service and how long you spend on them"); },
                    () => { BelowHeader("Cookies used to improve the Modern slavery statement registry").Expect("what you click on while you’re visiting the site"); },
                    () => { BelowHeader("Cookies used to improve the Modern slavery statement registry").AboveHeader("Cookies used by the Government Digital Service").ClickLabel("On") ; },
                    () => { ExpectHeader("Cookies measuring system performance"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("We use Microsoft Azure Application Insights to measure system performance to see if there is anything that is not working in the way it should. Application Insights stores anonymised information about:"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("the pages you visit on the service"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("details about the site's performance, for example how long each page takes to load"); },
                    () => { BelowHeader("Cookies measuring system performance").AboveHeader("Cookies that remember your settings").ClickLabel("On"); },
                    () => { ExpectHeader("Cookies that remember your settings"); },
                    () => { BelowHeader("Cookies that remember your settings").Expect("These cookies do things like remember your preferences and the choices you make, to personalise your experience of using the service."); },
                    () => { BelowHeader("Cookies that remember your settings").AboveHeader("Strictly necessary cookies").ClickLabel("On"); },
                    () => { ExpectHeader("Strictly necessary cookies"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("These essential cookies do things like:"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("enable you to login to submit information about your organisation's modern slavery statement"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("remember your progress through the service"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("remember notifications you’ve seen so that we don’t show them again"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("They always need to be on."); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectLink("Find out more about cookies on the Modern slavery statement registry"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("These essential cookies do things like:"); });

            await Task.CompletedTask;

        }

        [Test, Order(6)]
        public async Task Navigate_To_More_Info()

        {
            Click("Find out more about cookies on the Modern slavery statement registry");

            ExpectHeader("Details about cookies on Modern slavery statement registry");
            await Task.CompletedTask;

        }

        [Test, Order(8)]
        public async Task Verify_Cookie_Info_Page_Content()

        {
           Try (() => { Expect("Modern slavery statement registry puts small files (known as ‘cookies’) onto your computer to collect information about how you browse the site. Find out more about the cookies we use, what they’re for and when they expire."); },
                    () => { ExpectHeader("Cookies that measure website usage"); },
                    () => { BelowHeader("Cookies that measure website usage").Expect("We do not allow Google to use or share the data about how you use this site."); },
                    () => { BelowHeader("Cookies that measure website usage").Expect("Google Analytics stores information about:"); },
                    () => { BelowHeader("Cookies that measure website usage").Expect("how you got to the site"); },
                    () => { BelowHeader("Cookies that measure website usage").Expect("the pages you visit on the service and how long you spend on them"); },
                    () => { BelowHeader("Cookies that measure website usage").Expect("what you click on while you’re visiting the site"); },
                    () => { BelowHeader("Cookies that measure website usage").Expect(What.Contains, "Google Analytics sets the following cookies:"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectColumn("Name"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectColumn("Expires"); },
                    () => { BelowHeader("Cookies that measure website usage").AboveHeader("Cookies measuring system performance").ExpectRow(4); },
                    () => { BelowHeader("Cookies that measure website usage").AboveHeader("Cookies measuring system performance").ExpectNoRow(5); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectRowColumns("_ga", "This helps us count how many people visit the service by tracking if you’ve visited before", "2 years"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectRowColumns("_gid", "This helps us count how many people visit the service by tracking if you’ve visited before", "24 hours"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectRowColumns("_gat", "This helps Google to manage the rate at which they track your site interactions (when high traffic occurs)", "1 minute"); },
                    () => { BelowHeader("Cookies that measure website usage").ExpectRowColumns("_gat_govuk_shared", "This helps Google to manage the rate at which they track your site interactions (when high traffic occurs)", "1 minute"); }, 
                    () => { ExpectHeader("Cookies measuring system performance"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("We use Microsoft Azure Application Insights software to measure system performance to see if there is anything that is not working in the way it should."); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("We do not allow Microsoft to use or share the data about how you use this site."); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("Application Insights stores information about:"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("the pages you visit on the service"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("details about the site's performance, for example how long each page takes to load"); },
                    () => { BelowHeader("Cookies measuring system performance").Expect("Application Insights sets the following cookies:"); },
                    () => { BelowHeader("Cookies measuring system performance").ExpectColumn("Name"); },
                    () => { BelowHeader("Cookies measuring system performance").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Cookies measuring system performance").ExpectColumn("Expires"); }, 
                    () => { BelowHeader("Cookies measuring system performance").AboveHeader("Strictly necessary cookies").ExpectRow(2); },
                    () => { BelowHeader("Cookies measuring system performance").AboveHeader("Strictly necessary cookies").ExpectNoRow(3); },
                    () => { BelowHeader("Cookies measuring system performance").ExpectRowColumns("ai_session", "This helps Application Insights understand the performance of the site during a single visit", "30 minutes"); },
                    () => { BelowHeader("Cookies measuring system performance").ExpectRowColumns("ai_user", "This helps Application Insights understand how the performance of the site varies between different users", "1 year"); },
                    () => { ExpectHeader("Cookies that remember your settings"); },
                    () => { BelowHeader("Cookies that remember your settings").Expect("These cookies do things like remember your preferences and the choices you make, to personalise your experience of using the service."); },
                    () => { BelowHeader("Cookies that remember your settings").ExpectColumn("Name"); },
                    () => { BelowHeader("Cookies that remember your settings").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Cookies that remember your settings").ExpectColumn("Expires"); },
                    () => { ExpectHeader("Strictly necessary cookies"); },
                    () => { BelowHeader("Strictly necessary cookies").Expect("Logging into the service"); }, 
                    () => { BelowHeader("Strictly necessary cookies").Expect("To add a modern salavery statement to the registry, you have to log into the service. The following cookies are used as part of this login process."); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectColumn("Name"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectColumn("Expires"); }, 
                    () => { BelowHeader("Strictly necessary cookies").AboveHeader("Your progress when using the service").ExpectRow(5); },
                    () => { BelowHeader("Strictly necessary cookies").AboveHeader("Your progress when using the service").ExpectNoRow(6); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectRowColumns("idsrv.session", "Used during the login process", "When you close your browser"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectRowColumns("idsrv", "Used during the login process", "When you close your browser"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectRowColumns(".AspNetCore.Correlation.*", "Used during the login process", "15 minutes"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectRowColumns(".AspNetCore.OpenIdConnect.*", "Used during the login process", "15 minutes"); },
                    () => { BelowHeader("Strictly necessary cookies").ExpectRowColumns(".AspNetCore.Cookies", "Used to keep you logged in to the section of the site where you enter your statement details", "When you close your browser"); },
                    () => { ExpectHeader("Strictly necessary cookies"); },
                    () => { ExpectHeader("Your progress when using the service"); },
                    () => { BelowHeader("Your progress when using the service").Expect("When you use the service, we’ll set a cookie to remember your progress through the forms. These cookies do not store your personal data."); },
                    () => { BelowHeader("Your progress when using the service").ExpectColumn("Name"); },
                    () => { BelowHeader("Your progress when using the service").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Your progress when using the service").ExpectColumn("Expires"); },
                    () => { BelowHeader("Your progress when using the service").ExpectRowColumns(".AspNetCore.Session", "Saves your progress through the forms on the site.", "When you close your browser"); },
                    () => { BelowHeader("Your progress when using the service").ExpectRowColumns(".AspNetCore.Antiforgery.*", "Helps to keep the site secure - so only users who are logged in can submit statement details", "When you close your browser"); },
                    () => { BelowHeader("Your progress when using the service").AboveHeader("Cookies message").ExpectRow(2); },
                    () => { BelowHeader("Your progress when using the service").AboveHeader("Cookies message").ExpectNoRow(3); },
                    () => { ExpectHeader("Cookies message"); },
                    () => { BelowHeader("Cookies message").Expect("You may see a banner when you visit GOV.UK inviting you to accept cookies or review your settings. We’ll set cookies so that your computer knows you’ve seen it and not to show it again, and also to store your settings."); },
                    () => { BelowHeader("Cookies message").ExpectColumn("Name"); },
                    () => { BelowHeader("Cookies message").ExpectColumn("Purpose"); },
                    () => { BelowHeader("Cookies message").ExpectColumn("Expires"); },
                    () => { BelowHeader("Cookies message").ExpectRow(2); },
                    () => { BelowHeader("Cookies message").ExpectNoRow(3); },
                    () => { BelowHeader("Cookies message").ExpectRowColumns("seen_cookie_message", "Lets us know that you’ve seen our cookie message", "1 year"); },
                    () => { BelowHeader("Cookies message").ExpectRowColumns("cookie_settings", "Saves your cookie consent settings", "1 year"); },
                    () => { ExpectHeader("Change your settings"); },
                    () => { BelowHeader("Change your settings").Expect(What.Contains, "You can "); },
                    () => { BelowHeader("Change your settings").ExpectLink(That.Contains, "change which cookies you’re happy for us to use"); }                    );

            await Task.CompletedTask;

        }

        [Test, Order(10)]
        public async Task ClickingLinkReturnsToCookiesPage()

        {
            Click("change which cookies you’re happy for us to use");
            ExpectHeader("Cookies on Modern slavery statement registry");

            await Task.CompletedTask;

        }

        [Test, Order(12)]
        public async Task SubmitCookieForm()

        {
            Click("Confirm");
            Expect("Your cookie settings were saved");

            await Task.CompletedTask;

        }

    } 
    
}