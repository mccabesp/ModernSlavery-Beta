using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;

namespace ModernSlavery.Hosts.Web.Tests
{
    [SetUpFixture]
    public class WebHostSampleTest
    {
        private string WebAuthority;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            WebAuthority = WebHostSetup.WebTestHost.GetWebAuthority();
        }

        [Test]
        public void WebTestHost_Authority_IsValidUrl()
        {
            //Check we got the url of the test web server
            Assert.That(WebAuthority.IsUrl());
        }

        [Test]
        public async Task WebTestHost_HttpGet_ReturnsValidResponse()
        {
            //Check we get a response from the test web server
            var response=await WebRequestAsync(HttpMethods.Get,WebAuthority);
            Assert.That(!string.IsNullOrWhiteSpace(response));
        }
    }
}