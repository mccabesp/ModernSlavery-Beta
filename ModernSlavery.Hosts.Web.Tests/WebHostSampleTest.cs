using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Interfaces;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class WebHostSampleTest: UITest(WebHostSetup.)
    {
        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = WebHostSetup.TestWebHost.GetHostAddresses().FirstOrDefault();
            if (Debugger.IsAttached)Debug.WriteLine($"Kestrel authority: {_webAuthority}");
            Console.WriteLine($"Kestrel authority: {_webAuthority}");

            //Get the data repository from the test web host
            _dataRepository = WebHostSetup.TestWebHost.GetDataRepository();

            //Get the file repository from the test web host
            _fileRepository = WebHostSetup.TestWebHost.GetFileRepository();
            if (Debugger.IsAttached) Debug.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");
            Console.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");
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
    }
}