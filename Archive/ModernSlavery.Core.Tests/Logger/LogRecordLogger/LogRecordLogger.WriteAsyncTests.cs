﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Tests.Common.Classes;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Logger.LogAuditLogger
{
    [TestFixture]
    public class WriteAsyncTests
    {
        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Infrastructure.Storage.MessageQueues.AzureQueue>("TestConnectionString", "TestQueueName")
                {CallBase = true};

            mockLogAuditLogger =
                new Mock<Infrastructure.Logging.AuditLogger>(mockQueue.Object, testApplicationName, testFileName)
                    {CallBase = true};
        }

        private Mock<Infrastructure.Storage.MessageQueues.AzureQueue> mockQueue;
        private Mock<Infrastructure.Logging.AuditLogger> mockLogAuditLogger;

        private readonly string testApplicationName = "LogRecordUnitTests";
        private readonly string testFileName = "LogRecordUnitTest.csv";

        [TestCase]
        public async Task WritesNullColumnsToJson()
        {
            // Arrange
            object testNull = null;
            var AddMessageAsyncWasCalled = false;

            var testRecordModel = new {col1 = 123, col2 = testNull, col3 = "test"};

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) =>
                    {
                        // Assert
                        Assert.IsTrue(message.Contains("\"col2\":null"),
                            "Expected null columns to be written to the queue");
                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            var logger = mockLogAuditLogger.Object;
            await logger.WriteAsync(testRecordModel);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

        [TestCase]
        public async Task WritesLogRecordWrapperModelToJson()
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;

            var expectedWrapperModel = new LogRecordWrapperModel
            {
                ApplicationName = testApplicationName, FileName = testFileName, Record = "Some record"
            };

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) =>
                    {
                        var actualWrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(message);

                        // Assert
                        expectedWrapperModel.Compare(actualWrapper);

                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            var logger = mockLogAuditLogger.Object;
            await logger.WriteAsync(expectedWrapperModel.Record);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

        [TestCase("record1", "record2", "record3", "record4", "record5")]
        public async Task WritesListOfLogRecordWrapperModelsToJson(params string[] testRecords)
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;
            var expectedIndex = 0;

            var expectedEnumerableRecords = new List<LogRecordWrapperModel>();
            foreach (var record in testRecords)
                expectedEnumerableRecords.Add(
                    new LogRecordWrapperModel
                        {ApplicationName = testApplicationName, FileName = testFileName, Record = record});

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) =>
                    {
                        var actualWrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(message);

                        // Assert
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].ApplicationName,
                            actualWrapper.ApplicationName);
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].FileName, actualWrapper.FileName);
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].Record, actualWrapper.Record);

                        expectedIndex++;
                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            var logger = mockLogAuditLogger.Object;
            await logger.WriteAsync(testRecords);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }
    }
}