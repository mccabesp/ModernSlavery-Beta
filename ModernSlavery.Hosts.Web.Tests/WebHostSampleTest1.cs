using Geeks.Pangolin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    [Parallelizable]
    public class WebHostSampleTest1: BaseUITest
    {
        public WebHostSampleTest1():base(TestRunSetup.TestWebHost,TestRunSetup.WebDriverService)
        {
            
        }

        private string _webAuthority;
        private IFileRepository _fileRepository;
        private SharedOptions _sharedOptions;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = _testWebHost.GetHostAddress();
            TestContext.Progress.WriteLine($"Kestrel authority: {_webAuthority}");

            //Get the file repository from the test web host
            _fileRepository = _testWebHost.GetDependency<IFileRepository>();
            TestContext.Progress.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");

            //Get the shared options
            _sharedOptions = TestRunSetup.TestWebHost.GetDependency<SharedOptions>();
        }

        private bool TestRunFailed=false;
        private bool ContinueOnFail = false;

        [SetUp]
        public void SetUp()
        {
            ContinueOnFail = false;
            if (TestRunFailed) 
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {
            if (!ContinueOnFail && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;
            TeardownTest();
        }

        [Test]
        public void WebTestHost_Authority_IsValidUrl()
        {
            //Check we got the url of the test web server
            Assert.That(_webAuthority.IsUrl());
        }

        [Test]
        public void WebTestHost_DataRepository_Exists()
        {
            var datarepo = _testWebHost.Services.GetFileRepository();
            datarepo = ServiceScope.ServiceProvider.GetFileRepository();

            //Check we got the data repository from the test web server
            var dataRepository = ServiceScope.GetDataRepository();

            Assert.IsNotNull(dataRepository);
        }

        [Test]
        public void WebTestHost_FileRepository_Exists()
        {
            //Check we got the file repository from the test web server
            Assert.IsNotNull(_fileRepository);
        }


        [Test]
        public async Task WebTestHost_HttpGet_ReturnsIdentityDiscovery()
        {
            ContinueOnFail = true;

            //Check we get a response from the test web server
            var discoveryAuthority = $"{_webAuthority}/.well-known/openid-configuration";
            TestContext.Progress.WriteLine($"Discovery authority: {discoveryAuthority}");

            var response = await WebRequestAsync(HttpMethods.Get, discoveryAuthority, validateCertificate: false);
            Assert.That(!string.IsNullOrWhiteSpace(response));
        }


        [Test]
        public async Task WebTestHost_HttpGet_ReturnsValidResponse()
        {
            ContinueOnFail = true;

            //Check we get a response from the test web server
            var response =await WebRequestAsync(HttpMethods.Get,_webAuthority, validateCertificate:false);
            Assert.That(!string.IsNullOrWhiteSpace(response));
        }

        [Test]
        public async Task WebTestHost_SeleniumHelper_TestMethods_OK()
        {
            //Check the accessibility of the current page
            await this.CheckAccessibilityAsync();

            //Go to the landing page
            Goto("/manage-organisations");

            //Check the accessibility of the current page
            await this.CheckAccessibilityAsync("manage-organisations");

            //Check for the landing page header
            Expect(What.Contains,_sharedOptions.ServiceName);

            Expect("It may take up to a week to register your organisation");
        }
    }
}