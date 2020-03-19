using System;
using ModernSlavery.Extensions;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.WebJob.Tests
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
            DateTime expectedDateTime = DateTime.Now;
            DateTime upperBound = expectedDateTime.AddSeconds(2);
            DateTime lowerBound = expectedDateTime.AddSeconds(-2);

            // Act
            DateTime actualDateTime = VirtualDateTime.Now;

            // Assert
            bool expectedResult = actualDateTime > lowerBound && actualDateTime < upperBound;
            string errorMessage =
                $"The dateTime returned from GpgVirtualDateTime.Now was expected to be between {lowerBound} and {upperBound}, but it was actually {actualDateTime}.";

            Assert.IsTrue(expectedResult, errorMessage);
        }

    }
}
