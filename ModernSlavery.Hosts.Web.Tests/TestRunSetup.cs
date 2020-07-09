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
            if (Directory.Exists(LogsFilepath))
            {
                var logs = Directory.GetFiles(LogsFilepath, "*.*", SearchOption.AllDirectories);
                foreach (var log in logs)
                    File.Delete(log);
            }

            //Delete all previous screenshots
            if (Directory.Exists(ScreenshotsFilepath))
            {
                var screenshots = Directory.GetFiles(ScreenshotsFilepath, "*.*", SearchOption.AllDirectories);
                foreach (var screenshot in screenshots)
                    File.Delete(screenshot);
            }

            //Create the test host usign the default dependency module and override with a test module
            TestWebHost = HostHelper.CreateTestWebHost<TestDependencyModule>();

            //Start the test host
            await TestWebHost.StartAsync().ConfigureAwait(false);

            var baseUrl = TestWebHost.GetHostAddress();
            TestContext.Progress.WriteLine($"Test Host started on endpoint: {baseUrl}");
            WebDriverService = UITest.SetupWebDriverService(baseUrl: baseUrl);
        }

        [OneTimeTearDown]
        public async Task RunAfterAnyTestsAsync()
        {
            if (TestWebHost == null) return;

            //Stop the webhost
            await TestWebHost?.StopAsync();

            //Release the webhost resources
            TestWebHost?.Dispose();

            //Dispose of the webdriver service
            WebDriverService?.DisposeService();

            //NOTE: these dont seem yet to upload so using DevOps task to publish instead till we can get working
            //Publish all log files on failure
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed
               && Directory.Exists(LogsFilepath))
            {
                var logs = Directory.GetFiles(LogsFilepath, "*.*", SearchOption.AllDirectories);
                foreach (var log in logs)
                {
                    var filename = Path.GetFileName(log);
                    TestContext.AddTestAttachment(log, $"[LOG]: {filename}");
                    TestContext.Progress.WriteLine($"Added test attachment: {filename}");
                }
            }

            //Publish all screenshots
            if (Directory.Exists(ScreenshotsFilepath))
            {
                var screenshots = Directory.GetFiles(ScreenshotsFilepath, "*.*", SearchOption.AllDirectories);
                foreach (var screenshot in screenshots)
                {
                    var filename = Path.GetFileName(screenshot);
                    TestContext.AddTestAttachment(screenshot, $"[SCREENSHOT]: {filename}");
                    TestContext.Progress.WriteLine($"Added test attachment: {filename}");
                }
            }
        }
    }
}
