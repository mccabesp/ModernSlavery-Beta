using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Entities;
using ModernSlavery.Tests.Common.TestHelpers;
using NUnit.Framework;

namespace ModernSlavery.Database.Tests
{
    [TestFixture]
    public class OrganisationTests
    {
        [SetUp]
        public void BeforeEach()
        {
            _testOrganisation = OrganisationHelper.GetPublicOrganisation();

            var mocked2017Return = ReturnHelper.CreateTestReturn(_testOrganisation);
            OrganisationHelper.LinkOrganisationAndReturn(_testOrganisation, mocked2017Return);

            var mocked2018Return = ReturnHelper.CreateTestReturn(_testOrganisation, 2018);
            OrganisationHelper.LinkOrganisationAndReturn(_testOrganisation, mocked2018Return);
        }

        private Organisation _testOrganisation;

        [TestCase(2, 2)]
        [TestCase(1, 1)]
        public void OrganisationDB_GetRecentReportingYears_Returns_The_Correct_Years(int countOfYears, int expected)
        {
            // Arrange - Act
            var returnedDates = _testOrganisation.GetRecentReportingYears(countOfYears);
            var actualCountOfReturnedDates = returnedDates.Count();

            // Assert
            Assert.AreEqual(expected, actualCountOfReturnedDates);
        }

        [TestCase(2, 2)]
        [TestCase(1, 1)]
        public void OrganisationDB_GetRecentReports_Returns_The_Correct_Years(int countOfYears, int expected)
        {
            // Arrange - Act
            var returnsAvailableInPreviousYears = _testOrganisation.GetRecentReports(countOfYears);
            var actualCount = returnsAvailableInPreviousYears.Count();

            // Assert
            Assert.AreEqual(expected, actualCount);
        }

        [Test]
        public void OrganisationDB_GetRecentReports_Returns_New_Return_If_No_Data_Available()
        {
            // todo: the logic under test here should be changed, since it's not correct that a 'new return' - filled with zeroes!! - is created if the system doesn't find one a return on the database for a particular year.

            // Arrange
            _testOrganisation.Returns = new List<Return>(); // remove returns

            // Act
            var returnsAvailableForTheLastTwoYears = _testOrganisation.GetRecentReports(2);
            var actualCount = returnsAvailableForTheLastTwoYears.Count();
            var returnForCurrentYear = returnsAvailableForTheLastTwoYears.ElementAt(0);
            var returnForPreviousYear = returnsAvailableForTheLastTwoYears.ElementAt(1);

            // Assert
            Assert.AreEqual(2, actualCount);
            Assert.AreEqual(0, returnForCurrentYear.ReturnId);
            Assert.AreEqual(0, returnForPreviousYear.ReturnId);
        }
    }
}