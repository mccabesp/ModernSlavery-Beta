using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Logger.LogEventLogger
{
    [TestFixture]
    public class ConstructorTests
    {
        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Infrastructure.Storage.MessageQueues.AzureQueue>("TestConnectionString", "TestQueueName")
                {CallBase = true};
        }

        private Mock<Infrastructure.Storage.MessageQueues.AzureQueue> mockQueue;

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenApplicationNameIsIllegal(string testAppName)
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() =>
                    new Infrastructure.Logging.EventLoggerProvider(mockQueue.Object, testAppName, null));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: applicationName", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenQueueIsNull()
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Infrastructure.Logging.EventLoggerProvider(null, "TestApplicationName",
                    new LoggerFilterOptions()));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queue", actualExpection.Message);
        }
    }
}