using Microsoft.Extensions.Hosting;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using ModernSlavery.Hosts.Web.Tests;
using System.Diagnostics;

/// <summary>
/// This class is used by all test fixtures (classes) in this assembly to setup and teardown the websever
/// This particular class creates and tears down a web host
/// </summary>
[SetUpFixture]
public class WebHostSetup
{
    public static IHost TestWebHost { get; private set; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        //Create the test host usign the default dependency module and override with a test module
        TestWebHost = HostHelper.CreateTestWebHost<TestDependencyModule>();

        //Start the test host
        await TestWebHost.StartAsync().ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTestsAsync()
    {
        if (TestWebHost == null) return;

        //Stop the webhost
        await TestWebHost?.StopAsync();

        //Release the webhost resources
        TestWebHost?.Dispose();
    }
}
