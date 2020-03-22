using Microsoft.Azure.Search;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Tests.Common.Classes;
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
            var sicCodeSearchServiceClient = new SearchServiceClient(
                ConfigHelpers.SearchOptions.AzureServiceName,
                new SearchCredentials(ConfigHelpers.SearchOptions.AzureApiAdminKey));

            // Act
            var actualSicCodeSearchRepository = new AzureSicCodeSearchRepository(Mock.Of<IRecordLogger>(),
                sicCodeSearchServiceClient, ConfigHelpers.SearchOptions.SicCodeIndexName);

            // Assert
            Assert.NotNull(
                actualSicCodeSearchRepository,
                "This test should have been able to create a SicCodeSearchRepository object but seems it was unable to do so");
        }
    }
}