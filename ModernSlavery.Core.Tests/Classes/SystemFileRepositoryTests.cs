using System;
using ModernSlavery.Infrastructure.Storage.Classes;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Classes
{
    [TestFixture]
    public class SystemFileRepositoryTests
    {
        [Test]
        public void ReadDataTableAsync_When_FilePath_Is_Null_Throws_ArgumentNullException()
        {
            // Arrange
            var testSystemFileRepository = new SystemFileRepository(new StorageOptions());

            // Act
            var actualException =
                Assert.ThrowsAsync<ArgumentNullException>(async () =>
                {
                    await testSystemFileRepository.ReadDataTableAsync(null);
                });

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: filePath", actualException.Message);
        }
    }
}