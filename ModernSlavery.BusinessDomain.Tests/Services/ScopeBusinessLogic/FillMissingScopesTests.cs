using System;
using System.Linq;
using ModernSlavery.BusinessLogic.Submission;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Services.ScopeBusinessLogic
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class FillMissingScopesTests : BaseBusinessLogicTests
    {
        [SetUp]
        public void BeforeEach()
        {
            // setup mocks
            mockDataRepository = MoqHelpers.CreateMockDataRepository();

            var mockedSnapshotDateHelper = Get<ISnapshotDateHelper>();
            var mockedSourceComparer = Get<ISourceComparer>();
            var mockedSendEmailService = Get<ISendEmailService>();
            var mockedNotificationService = Get<INotificationService>();
            var mockedFileRepository = Get<IFileRepository>();
            var mockedDataRepository = Get<IDataRepository>();
            mockSharedBusinessLogic = new SharedBusinessLogic(mockedSnapshotDateHelper, mockedSourceComparer,
                mockedSendEmailService, mockedNotificationService, mockedFileRepository, mockedDataRepository);

            // sut
            scopeBusinessLogic = new Submission.ScopeBusinessLogic(
                mockSharedBusinessLogic,
                mockDataRepository.Object,
                null, null);
        }

        private Mock<IDataRepository> mockDataRepository;
        private ISharedBusinessLogic mockSharedBusinessLogic;

        // sut
        private IScopeBusinessLogic scopeBusinessLogic;

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void PresumesOutOfScopeForSnapshotYearsBeforeOrgCreatedDate(SectorTypes testSectorType)
        {
            // setup
            var testOrg = CreateOrgWithNoScopes(1, testSectorType, VirtualDateTime.Now);

            // act
            var actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            var currentSnapshotDate = mockSharedBusinessLogic.GetAccountingStartDate(testOrg.SectorType);
            var expectedScopeCount = currentSnapshotDate.Year - ConfigHelpers.SharedOptions.FirstReportingYear + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope before current snapshot year are set to presumed out of scope
            var actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (var i = 0; i < actualScopesArray.Length - 1; i++)
            {
                var scope = actualScopesArray[i];
                Assert.AreEqual(ScopeStatuses.PresumedOutOfScope, scope.ScopeStatus);
            }

            // assert current year is presumed in scope
            Assert.AreEqual(ScopeStatuses.PresumedInScope, actualScopesArray[actualScopesArray.Length - 1].ScopeStatus);
            Assert.NotNull(testOrg.LatestScope, "Expected latest scope to be set");
        }

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void PresumesInScopeForSnapshotYearsDuringAndAfterOrgCreatedDate(SectorTypes testSectorType)
        {
            // setup
            var testCreatedDate = mockSharedBusinessLogic.GetAccountingStartDate(testSectorType).AddYears(-1);
            var testOrg = CreateOrgWithNoScopes(1, testSectorType, testCreatedDate);

            // act
            var actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            var currentSnapshotDate = mockSharedBusinessLogic.GetAccountingStartDate(testOrg.SectorType);
            var expectedScopeCount = currentSnapshotDate.Year - ConfigHelpers.SharedOptions.FirstReportingYear + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope after created date is set to presumed in of scope
            var actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (var i = actualScopesArray.Length - 2; i < actualScopesArray.Length; i++)
            {
                var scope = actualScopesArray[i];
                Assert.AreEqual(ScopeStatuses.PresumedInScope, scope.ScopeStatus);
            }

            Assert.NotNull(testOrg.LatestScope, "Expected latest scope to be set");
        }

        [TestCase(SectorTypes.Private, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        public void PreservesPresumedScopesFromPreviousYear(SectorTypes testSectorType,
            ScopeStatuses testDeclaredScopeStatus,
            ScopeStatuses expectedPresumedScopeStatus)
        {
            // setup
            var testSnapshotDate =
                mockSharedBusinessLogic.GetAccountingStartDate(testSectorType,
                    ConfigHelpers.SharedOptions.FirstReportingYear);
            var testOrg = CreateOrgWithScopeForAllYears(1, testSectorType, testDeclaredScopeStatus, testSnapshotDate);

            // act
            var actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");
            Assert.NotNull(testOrg.LatestScope, "Expected latest scope to be set");
            Assert.AreEqual(expectedPresumedScopeStatus, testOrg.LatestScope.ScopeStatus,
                "Expected latest scope to be PresumedOutOfScope");
        }

        [TestCase(SectorTypes.Private, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.InScope, ScopeStatuses.PresumedInScope)]
        [TestCase(SectorTypes.Private, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        [TestCase(SectorTypes.Public, ScopeStatuses.OutOfScope, ScopeStatuses.PresumedOutOfScope)]
        public void PreservesDeclaredScopes(SectorTypes testSectorType,
            ScopeStatuses testDeclaredScopeStatus,
            ScopeStatuses expectedPresumedScopeStatus)
        {
            // setup
            var testCreatedDate =
                mockSharedBusinessLogic.GetAccountingStartDate(testSectorType,
                    ConfigHelpers.SharedOptions.FirstReportingYear);
            var testOrg = CreateOrgWithDeclaredAndPresumedScopes(
                testSectorType,
                testDeclaredScopeStatus,
                testCreatedDate,
                testCreatedDate);

            // act
            var actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");

            var actualScopesArray = testOrg.OrganisationScopes.ToArray();
            Assert.AreEqual(testDeclaredScopeStatus, actualScopesArray[0].ScopeStatus,
                "Expected first year scope status to match");

            // check that each year is presumed out of scope after first year
            for (var i = 1; i < actualScopesArray.Length; i++)
            {
                var scope = actualScopesArray[i];
                Assert.AreEqual(expectedPresumedScopeStatus, scope.ScopeStatus,
                    "Expected presumed scope statuses to match");
            }
        }

        private Organisation CreateOrgWithNoScopes(int testOrgId, SectorTypes testSector, DateTime testCreated)
        {
            return new Organisation
            {
                OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active,
                Created = testCreated
            };
        }

        private Organisation CreateOrgWithDeclaredAndPresumedScopes(
            SectorTypes testSector,
            ScopeStatuses testDeclaredScopeStatus,
            DateTime testCreated,
            DateTime testSnapshotDate)
        {
            var testOrg = CreateOrgWithNoScopes(1, testSector, testCreated);

            testOrg.OrganisationScopes.Add(
                new OrganisationScope
                {
                    OrganisationScopeId = 1,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = testSnapshotDate,
                    ScopeStatus = testDeclaredScopeStatus
                });

            testOrg.OrganisationScopes.Add(
                new OrganisationScope
                {
                    OrganisationScopeId = 2,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = testSnapshotDate.AddYears(1),
                    ScopeStatus = testDeclaredScopeStatus == ScopeStatuses.InScope
                        ? ScopeStatuses.PresumedInScope
                        : ScopeStatuses.PresumedOutOfScope
                });

            return testOrg;
        }

        private Organisation CreateOrgWithScopeForAllYears(int testOrgId,
            SectorTypes testSector,
            ScopeStatuses testScopeStatus,
            DateTime snapshotDate)
        {
            var firstYear = SectorTypeHelper.SnapshotDateHelper.FirstReportingYear;
            var lastYear = SectorTypeHelper.SnapshotDateHelper.CurrentSnapshotYear;

            var testOrg = CreateOrgWithNoScopes(testOrgId, testSector, VirtualDateTime.Now);

            // for all snapshot years check if scope exists
            for (var year = firstYear; year < lastYear; year++)
                testOrg.OrganisationScopes.Add(
                    new OrganisationScope
                    {
                        OrganisationScopeId = 1,
                        Status = ScopeRowStatuses.Active,
                        SnapshotDate = new DateTime(year, snapshotDate.Month, snapshotDate.Day),
                        ScopeStatus = testScopeStatus
                    });

            return testOrg;
        }
    }
}