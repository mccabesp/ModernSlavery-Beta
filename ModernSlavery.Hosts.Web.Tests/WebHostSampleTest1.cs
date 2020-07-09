using Geeks.Pangolin;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
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
    public class WebHostSampleTest1: UITest
    {
        public WebHostSampleTest1():base(TestRunSetup.WebDriverService)
        {
            
        }

        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;
        private SharedOptions _sharedOptions;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = TestRunSetup.TestWebHost.GetHostAddress();
            TestContext.Progress.WriteLine($"Kestrel authority: {_webAuthority}");

            //Get the data repository from the test web host
            _dataRepository = TestRunSetup.TestWebHost.GetDataRepository();

            //Get the file repository from the test web host
            _fileRepository = TestRunSetup.TestWebHost.GetFileRepository();
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
            //Check we got the data repository from the test web server
            Assert.IsNotNull(_dataRepository);
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
            //Go to the landing page
            Goto(_webAuthority);

            //Check for the landing page header
            Expect(What.Contains,_sharedOptions.ServiceName);

            Expect("It may take up to a week to register your organisation");
        }
    }
}