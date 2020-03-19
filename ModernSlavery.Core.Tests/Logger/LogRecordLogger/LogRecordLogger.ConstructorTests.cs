using System;
using ModernSlavery.Infrastructure.Queue;
using ModernSlavery.Tests.Common.Classes;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.LogRecordLogger
{
    [TestFixture]
    public class ConstrutorTests
    {
        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<LogRecordQueue>("TestConnectionString", "TestQueueName") {CallBase = true};
        }

        private Mock<LogRecordQueue> mockQueue;

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenApplicationNameIsIllegal(string testAppName)
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Infrastructure.Logging.LogRecordLogger(ConfigHelpers.GlobalOptions, mockQueue.Object,
                    testAppName, "TestFilename"));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: applicationName", actualExpection.Message);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenFileNameIsIllegal(string testFilename)
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Infrastructure.Logging.LogRecordLogger(ConfigHelpers.GlobalOptions, mockQueue.Object,
                    "TestAppName", testFilename));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: fileName", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenQueueIsNull()
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() =>
                    new Infrastructure.Logging.LogRecordLogger(ConfigHelpers.GlobalOptions, null, "TestAppName",
                        "TestFilename"));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queue", actualExpection.Message);
        }
    }
}