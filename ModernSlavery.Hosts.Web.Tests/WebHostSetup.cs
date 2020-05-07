using Autofac;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.BusinessDomain.Account;
using ModernSlavery.Infrastructure.Configuration;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// This class is used by all test fixtures (classes) in this assembly to setup and teardown the websever
/// This particular class creates and tears down a web host
/// </summary>
[SetUpFixture]
public class WebHostSetup
{
    public static IHost WebTestHost { get; private set; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        //Build the web host using the default dependencies
        var webHostBuilder = ModernSlavery.Hosts.Web.Program.CreateHostBuilder();

        webHostBuilder.ConfigureServices((context, serviceCollection) => { 
            //Override any dependency services here
        });
        webHostBuilder.ConfigureContainer<ContainerBuilder>((context, builder) => {
            //Override any autofac services here
        });

        //Build and start the host
        WebTestHost = await webHostBuilder.StartAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTestsAsync()
    {
        //Stop the webhost
        await WebTestHost?.StopAsync();

        //Release the webhost resources
        WebTestHost?.Dispose();
    }
}
