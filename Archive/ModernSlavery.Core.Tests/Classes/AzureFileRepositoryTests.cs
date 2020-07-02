using System;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Classes
{
    [TestFixture]
    public class AzureFileRepositoryTests
    {
        [TestCase("")]
        [TestCase(null)]
        public void AzureFileRepository_Constructor_When_ConnectionString_Is_Not_Set_Throws_Exception(
            string connectionString)
        {
            // Arrange / Act
            var actualException = Assert.Throws<ArgumentNullException>(() =>
            {
                new AzureFileRepository(ConfigHelpers.StorageOptions);
            });

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: connectionString", actualException.Message);
        }

        [TestCase("")]
        [TestCase(null)]
        public void AzureFileRepository_Constructor_When_ShareName_Is_Not_Set_Throws_Exception(string shareName)
        {
            // Arrange / Act
            var actualException =
                Assert.Throws<ArgumentNullException>(() => { new AzureFileRepository(ConfigHelpers.StorageOptions); });

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: shareName", actualException.Message);
        }
    }
}