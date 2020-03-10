using ModernSlavery.Core.Classes;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Azure.Search;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Data;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Classes
{
    [TestFixture]
    public class SicCodeSearchRepositoryTests
    {

        [Test]
        public void SicCodeSearchRepository_Can_Be_Created()
        {
            // Arrange
            string sicCodeSearchServiceName = Config.GetAppSetting("SearchService:ServiceName");
            string sicCodeSearchIndexName = Config.GetAppSetting("SearchService:IndexName");
            string sicCodeSearchAdminApiKey = Config.GetAppSetting("SearchService:AdminApiKey");

            var sicCodeSearchServiceClient = new SearchServiceClient(
                sicCodeSearchServiceName,
                new SearchCredentials(sicCodeSearchAdminApiKey));

            // Act
            var actualSicCodeSearchRepository = new AzureSicCodeSearchRepository(Mock.Of<ILogRecordLogger>(),sicCodeSearchServiceClient, sicCodeSearchIndexName);

            // Assert
            Assert.NotNull(
                actualSicCodeSearchRepository,
                "This test should have been able to create a SicCodeSearchRepository object but seems it was unable to do so");
        }

    }
}
