using ModernSlavery.Entities.Enums;
using ModernSlavery.Extensions;
using ModernSlavery.SharedKernel;
using ModernSlavery.Tests.Common.TestHelpers;
using NUnit.Framework;

namespace ModernSlavery.Database.ReturnTests
{
    [TestFixture]
    public class IsLateSubmissionTests
    {
        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        public void Is_True_When_ModifiedDate_Is_Late_And_InScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                var testYear = VirtualDateTime.Now.Year - yearOffset;
                var snapshotDate = sector.GetAccountingStartDate(testYear);
                var nextSnapshotDate = snapshotDate.AddYears(1);
                var modifiedDate = nextSnapshotDate.AddDays(2);

                var testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                var testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                var testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                var actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(true, actual);
            }
        }

        [TestCase(SectorTypes.Public, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.InScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Not_Late_And_AnyScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;
            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                var testYear = VirtualDateTime.Now.Year - yearOffset;
                var snapshotDate = sector.GetAccountingStartDate(testYear);
                var nextSnapshotDate = snapshotDate.AddYears(1);
                var modifiedDate = nextSnapshotDate.AddDays(-1);

                var testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                var testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                var testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                var actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(false, actual);
            }
        }

        [Test]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.PresumedOutOfScope)]
        public void Is_False_When_ModifiedDate_Is_Late_And_OutOfScope(SectorTypes sector, ScopeStatuses scopeStatus)
        {
            var totalYearOffsets = 4;

            for (var yearOffset = 0; yearOffset < totalYearOffsets; yearOffset++)
            {
                // Arrange 
                var testYear = VirtualDateTime.Now.Year - yearOffset;
                var snapshotDate = sector.GetAccountingStartDate(testYear);
                var nextSnapshotDate = snapshotDate.AddYears(1);
                var modifiedDate = nextSnapshotDate.AddDays(2);

                var testOrganisation = sector == SectorTypes.Private
                    ? OrganisationHelper.GetPrivateOrganisation()
                    : OrganisationHelper.GetPublicOrganisation();

                var testScope = ScopeHelper.CreateScope(scopeStatus, snapshotDate);

                var testReturn = ReturnHelper.CreateLateReturn(testOrganisation, snapshotDate, modifiedDate, testScope);

                // Act
                var actual = testReturn.IsLateSubmission;

                // Assert
                Assert.AreEqual(false, actual);
            }
        }
    }
}