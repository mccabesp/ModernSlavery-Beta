using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
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

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = TestRunSetup.TestWebHost.GetHostAddress();
            if (Debugger.IsAttached)Debug.WriteLine($"Kestrel authority: {_webAuthority}");
            Console.WriteLine($"Kestrel authority: {_webAuthority}");

            //Get the data repository from the test web host
            _dataRepository = TestRunSetup.TestWebHost.GetDataRepository();

            //Get the file repository from the test web host
            _fileRepository = TestRunSetup.TestWebHost.GetFileRepository();
            if (Debugger.IsAttached) Debug.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");
            Console.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");
        }

        private bool TestRunFailed=false;

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
            TeardownTest();
        }

        [Test]
        public void WebTestHost_Authority_IsValidUrl()
        {
            TestContext.Out.WriteLine($"Kestrel authority: {_webAuthority}");

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
            TestContext.Out.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");

            //Check we got the file repository from the test web server
            Assert.IsNotNull(_fileRepository);
        }

        [Test]
        public async Task WebTestHost_HttpGet_ReturnsValidResponse()
        {
            //Check we get a response from the test web server
            var response =await WebRequestAsync(HttpMethods.Get,_webAuthority);
            Assert.That(!string.IsNullOrWhiteSpace(response));
        }

        [Test]
        public async Task WebTestHost_SeleniumHelper_TestMethods_OK()
        {
            //Go to the landing page
            Goto(_webAuthority);

            //Check for the landing page header
            ExpectHeader("Search and compare Modern Slavery statements");

            //Check for the landing page header using Xpath
            ClickXPath("//*[@id='NextStep']");

            //Go to the landing page
            Goto(_webAuthority);

            //Check for the landing page header
            ExpectHeader("Search and compare Modern Slavery statements");

            //Check for the landing page header using Xpath
            ClickXPath("//*[@id='NextStep']");
        }
    }
}