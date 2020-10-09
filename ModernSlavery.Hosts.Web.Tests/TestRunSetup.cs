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

        [OneTimeSetUp]
        public async Task RunBeforeAnyTestsAsync()
        {
            //Delete all previous log files, screenshots and accessibility results
            LoggingHelper.ClearLogs();
            LoggingHelper.ClearScreenshots();
            LoggingHelper.ClearAccessibilityResults();

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

            //Create the Accessibility results summary
            AxeHelper.SaveResultSummary();

            //Attach log files, screenshots and accessibility results as pipeline build artifacts
            LoggingHelper.AttachLogs();
            LoggingHelper.AttachScreenshots();
            LoggingHelper.AttachAccessibilityResults();

            //Delete SQL firewall rules for the build agent
            AzureHelpers.CloseSQLFirewall();

            //Stop the webhost
            await TestWebHost?.StopAsync();

            //Release the webhost resources
            TestWebHost?.Dispose();

            //Dispose of the webdriver service
            WebDriverService?.DisposeService();
        }
    }
}
