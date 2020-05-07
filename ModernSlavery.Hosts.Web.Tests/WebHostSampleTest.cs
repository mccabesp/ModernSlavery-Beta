using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;

namespace ModernSlavery.Hosts.Web.Tests
{
    [SetUpFixture]
    public class WebHostSampleTest
    {
        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = WebHostSetup.WebTestHost.GetWebAuthority();

            //Get the data repository from the test web host
            _dataRepository = WebHostSetup.WebTestHost.GetDataRepository();

            //Get the file repository from the test web host
            _fileRepository = WebHostSetup.WebTestHost.GetFileRepository();
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
        public async Task WebTestHost_HttpGet_ReturnsValidResponse()
        {
            //Check we get a response from the test web server
            var response=await WebRequestAsync(HttpMethods.Get,_webAuthority);
            Assert.That(!string.IsNullOrWhiteSpace(response));
        }
    }
}