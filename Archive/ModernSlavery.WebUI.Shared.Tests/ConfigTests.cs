using System;
using ModernSlavery.Tests.Common.Classes;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Shared.Tests
{
    [TestFixture]
    public class ConfigTests
    {

        [Test]
        public void Config_DefaultDate_Returns_DateTime_Now_Plus_Three_Days_And_One_Hour_When_Configured()
        {
            // Arrange
            var expectedOffsetCurrentDateTimeForSite = new TimeSpan(4, 2, 0, 0, 0);

            // Act
            TimeSpan actualOffsetCurrentDateTimeForSite = TimeSpan.Parse(ConfigHelpers.SharedOptions.DateTimeOffset);

            // Assert
            Assert.AreEqual(
                expectedOffsetCurrentDateTimeForSite,
                actualOffsetCurrentDateTimeForSite,
                "This value is expected to be configured as 4 days and 2 hours on the 'appsettings.json' file");
        }

    }
}
