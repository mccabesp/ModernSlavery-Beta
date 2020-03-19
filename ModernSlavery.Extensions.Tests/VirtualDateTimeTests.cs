using System;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.Extensions.Tests
{
    [TestFixture]
    public class VirtualDateTimeTests
    {
        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(ConfigHelpers.GlobalOptions.DateTimeOffset);
        }

        [Test]
        public void VirtualDateTime_Now_Returns_DateTime_Now_Plus_Three_Days_And_One_Hour_When_Configured()
        {
            // Arrange
            var expectedDateTime = DateTime.Now.AddDays(3).AddHours(1);
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

        [Test]
        public void VirtualDateTime_UtcNow_Returns_DateTime_UtcNow_Plus_Three_Days_And_One_Hour_When_Configured()
        {
            // Arrange
            var expectedDateTime = DateTime.UtcNow.AddDays(3).AddHours(1);
            var upperBound = expectedDateTime.AddSeconds(2);
            var lowerBound = expectedDateTime.AddSeconds(-2);

            // Act
            var actualDateTime = VirtualDateTime.UtcNow;

            // Assert
            var expectedResult = actualDateTime > lowerBound && actualDateTime < upperBound;
            var errorMessage =
                $"The dateTime returned from GpgVirtualDateTime.UtcNow was expected to be between {lowerBound} and {upperBound}, but it was actually {actualDateTime}.";

            Assert.IsTrue(expectedResult, errorMessage);
        }
    }
}