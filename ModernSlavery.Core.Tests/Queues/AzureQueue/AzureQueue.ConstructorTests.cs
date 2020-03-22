using System;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Queues.AzureQueue
{
    [TestFixture]
    public class ConstructorTests
    {
        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenConnectionStringIsIllegal(string testConnString)
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() =>
                    new Infrastructure.Storage.MessageQueues.AzureQueue(testConnString, "TestQueueName"));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: connectionString", actualExpection.Message);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenQueueNameIsIllegal(string testQueueName)
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() =>
                    new Infrastructure.Storage.MessageQueues.AzureQueue("TestConnectionString", testQueueName));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queueName", actualExpection.Message);
        }
    }
}