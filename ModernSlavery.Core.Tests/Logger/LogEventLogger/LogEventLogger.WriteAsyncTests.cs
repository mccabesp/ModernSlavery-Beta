using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using ModernSlavery.Tests.Common.Classes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.SharedKernel;
using Moq;

using Newtonsoft.Json;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.LogEventLoggerProvider
{

    [TestFixture]
    public class WriteAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Infrastructure.Queue.AzureQueue>("TestConnectionString", "TestQueueName") {CallBase = true};

            mockLogEventLoggerProvider = new Mock<Infrastructure.Logging.LogEventLoggerProvider>(
                mockQueue.Object,
                testApplicationName,
                Options.Create(new LoggerFilterOptions())) {CallBase = true};
        }

        private Mock<Infrastructure.Queue.AzureQueue> mockQueue;
        private Mock<Infrastructure.Logging.LogEventLoggerProvider> mockLogEventLoggerProvider;

        private readonly string testApplicationName = "LogEventUnitTests";

        [TestCase]
        public async Task WritesNullColumnsToJson()
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;

            var testEntryModel = new LogEntryModel {WebPath = null};

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) => {
                        // Assert
                        Assert.IsTrue(message.Contains("\"WebPath\":null"), "Expected null columns to be written to the queue");
                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            Infrastructure.Logging.LogEventLoggerProvider logger = mockLogEventLoggerProvider.Object;
            await logger.WriteAsync(LogLevel.Information, testEntryModel);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

        [TestCase(LogLevel.Critical)]
        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Error)]
        [TestCase(LogLevel.Information)]
        [TestCase(LogLevel.None)]
        [TestCase(LogLevel.Trace)]
        [TestCase(LogLevel.Warning)]
        public async Task WritesLogEventWrapperModelToJson(LogLevel testLogLevel)
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;

            var testLogEntryModel = new LogEntryModel {Details = "UnitTest"};

            var expectedWrapperModel = new LogEventWrapperModel {
                ApplicationName = testApplicationName, LogLevel = testLogLevel, LogEntry = testLogEntryModel
            };

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) => {
                        var actualWrapper = JsonConvert.DeserializeObject<LogEventWrapperModel>(message);

                        // Assert
                        expectedWrapperModel.Compare(actualWrapper);

                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            Infrastructure.Logging.LogEventLoggerProvider logger = mockLogEventLoggerProvider.Object;
            await logger.WriteAsync(testLogLevel, testLogEntryModel);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

    }


}
