using System;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.Hosts.Webjob.Tests
{
    [TestFixture]
    public class WebJobConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(ConfigHelpers.GlobalOptions.DateTimeOffset);
        }

        [Test]
        public void WebJobConfiguration_Test_Has_OffsetCurrentDateTimeForSite_Configured_To_Zero_Days_Ahead()
        {
            // Arrange
            var expectedDateTime = DateTime.Now;
            var upperBound = expectedDateTime.AddSeconds(2);
            var lowerBound = expectedDateTime.AddSeconds(-2);

            // Act
            var actualDateTime = VirtualDateTime.Now;

            // Assert
            var expectedResult = actualDateTime > lowerBound && actualDateTime < upperBound;
            var errorMessage =
                $"The dateTime returned from GpgVirtualDateTime.Now was expected to be between {lowerBound} and {upperBound}, but it was actually {actualDateTime}.";

            Assert.IsTrue(expectedResult, errorMessage);
        }
    }
}