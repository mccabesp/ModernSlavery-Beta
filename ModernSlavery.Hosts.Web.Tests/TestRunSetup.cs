using Microsoft.Extensions.Hosting;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using ModernSlavery.Hosts.Web.Tests;
using Geeks.Pangolin.Service.DriverService;
using Geeks.Pangolin;
using ModernSlavery.Infrastructure.Hosts;
using NUnit.Framework.Interfaces;
using System.Net;
using System.IO;
using System;
using ModernSlavery.Testing.Helpers.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared.Interfaces;

namespace ModernSlavery.Hosts.Web.Tests
{
    /// <summary>
    /// This class is used by all test fixtures (classes) in this assembly to setup and teardown the websever
    /// This particular class creates and tears down a web host
    /// </summary>
    [SetUpFixture]
    public class TestRunSetup
    {
        public static IHost TestWebHost { get; private set; }
        public static SeleniumWebDriverService WebDriverService { get; private set; }

        private static string LogsFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles");
        private static string ScreenshotsFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");

        [OneTimeSetUp]
        public async Task RunBeforeAnyTestsAsync()
        {
            //Delete all previous log files
            LoggingHelper.DeleteFiles(LogsFilepath);

            //Delete all previous screenshots
            LoggingHelper.DeleteFiles(ScreenshotsFilepath);

            //Create the test host usign the default dependency module and override with a test module
            TestWebHost = HostHelper.CreateTestWebHost<TestDependencyModule>(applicationName: "ModernSlavery.Hosts.Web");

            //Create SQL firewall rule for the build agent
            TestWebHost.OpenSQLFirewall();

            //Start the test host
            await TestWebHost.StartAsync().ConfigureAwait(false);

            //Reset the database - this must be after host start so seed data files are copied
            await TestWebHost.ResetDatabaseAsync();

            //Delete all the draft files
            await TestWebHost.Services.DeleteDraftsAsync();

            //Start the Selenium client
            var baseUrl = TestWebHost.GetHostAddress();
            TestContext.Progress.WriteLine($"Test Host started on endpoint: {baseUrl}");
            WebDriverService = UITest.SetupWebDriverService(baseUrl: baseUrl);
        }
        

        [OneTimeTearDown]
        public async Task RunAfterAnyTestsAsync()
        {
            if (TestWebHost == null) return;

            //Delete SQL firewall rules for the build agent
            AzureHelpers.CloseSQLFirewall();

            //Stop the webhost
            await TestWebHost?.StopAsync();

            //Release the webhost resources
            TestWebHost?.Dispose();

            //Dispose of the webdriver service
            WebDriverService?.DisposeService();

            //NOTE: these dont seem yet to upload so using DevOps task to publish instead till we can get working
            //Publish all log files on failure
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                LoggingHelper.AttachFiles(LogsFilepath,"LOG");

            //Publish all screenshots
            LoggingHelper.AttachFiles(LogsFilepath, "SCREENSHOT");
        }
    }
}
