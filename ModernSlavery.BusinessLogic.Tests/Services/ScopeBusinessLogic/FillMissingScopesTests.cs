using System;
using System.Linq;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using ModernSlavery.Tests.Common.Classes;
using Moq;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel;

using NUnit.Framework;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Abstractions;
using ModernSlavery.Core.Classes;
using ModernSlavery.Tests.Common.TestHelpers;

namespace ModernSlavery.BusinessLogic.Tests.ScopeBusinessLogic
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
            mockCommonBusinessLogic = new CommonBusinessLogic(mockedSnapshotDateHelper, mockedSourceComparer, mockedSendEmailService, mockedNotificationService,mockedFileRepository,mockedDataRepository);

            // sut
            scopeBusinessLogic = new BusinessLogic.ScopeBusinessLogic(
                mockCommonBusinessLogic,
                mockDataRepository.Object,
                null,null);
        }

        private Mock<IDataRepository> mockDataRepository;
        private ICommonBusinessLogic mockCommonBusinessLogic;

        // sut
        private IScopeBusinessLogic scopeBusinessLogic;

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void PresumesOutOfScopeForSnapshotYearsBeforeOrgCreatedDate(SectorTypes testSectorType)
        {
            // setup
            Organisation testOrg = CreateOrgWithNoScopes(1, testSectorType, VirtualDateTime.Now);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            DateTime currentSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testOrg.SectorType);
            int expectedScopeCount = (currentSnapshotDate.Year - Global.FirstReportingYear) + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope before current snapshot year are set to presumed out of scope
            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (var i = 0; i < actualScopesArray.Length - 1; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
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
            DateTime testCreatedDate = mockCommonBusinessLogic.GetAccountingStartDate(testSectorType).AddYears(-1);
            Organisation testOrg = CreateOrgWithNoScopes(1, testSectorType, testCreatedDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change for missing scopes");

            // test the count of scopes set is correct
            DateTime currentSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testOrg.SectorType);
            int expectedScopeCount = (currentSnapshotDate.Year - Global.FirstReportingYear) + 1;
            Assert.AreEqual(expectedScopeCount, testOrg.OrganisationScopes.Count);

            // check each scope after created date is set to presumed in of scope
            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            for (int i = actualScopesArray.Length - 2; i < actualScopesArray.Length; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
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
            DateTime testSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testSectorType, Global.FirstReportingYear);
            Organisation testOrg = CreateOrgWithScopeForAllYears(1, testSectorType, testDeclaredScopeStatus, testSnapshotDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");
            Assert.NotNull(testOrg.LatestScope, "Expected latest scope to be set");
            Assert.AreEqual(expectedPresumedScopeStatus, testOrg.LatestScope.ScopeStatus, "Expected latest scope to be PresumedOutOfScope");
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
            DateTime testCreatedDate = mockCommonBusinessLogic.GetAccountingStartDate(testSectorType, Global.FirstReportingYear);
            Organisation testOrg = CreateOrgWithDeclaredAndPresumedScopes(
                testSectorType,
                testDeclaredScopeStatus,
                testCreatedDate,
                testCreatedDate);

            // act
            bool actualChanged = scopeBusinessLogic.FillMissingScopes(testOrg);

            // assert
            Assert.IsTrue(actualChanged, "Expected change to be true for missing scopes");

            OrganisationScope[] actualScopesArray = testOrg.OrganisationScopes.ToArray();
            Assert.AreEqual(testDeclaredScopeStatus, actualScopesArray[0].ScopeStatus, "Expected first year scope status to match");

            // check that each year is presumed out of scope after first year
            for (var i = 1; i < actualScopesArray.Length; i++)
            {
                OrganisationScope scope = actualScopesArray[i];
                Assert.AreEqual(expectedPresumedScopeStatus, scope.ScopeStatus, "Expected presumed scope statuses to match");
            }
        }

        private Organisation CreateOrgWithNoScopes(int testOrgId, SectorTypes testSector, DateTime testCreated)
        {
            return new Organisation {
                OrganisationId = testOrgId, SectorType = testSector, Status = OrganisationStatuses.Active, Created = testCreated
            };
        }

        private Organisation CreateOrgWithDeclaredAndPresumedScopes(
            SectorTypes testSector,
            ScopeStatuses testDeclaredScopeStatus,
            DateTime testCreated,
            DateTime testSnapshotDate)
        {
            Organisation testOrg = CreateOrgWithNoScopes(1, testSector, testCreated);

            testOrg.OrganisationScopes.Add(
                new OrganisationScope {
                    OrganisationScopeId = 1,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = testSnapshotDate,
                    ScopeStatus = testDeclaredScopeStatus
                });

            testOrg.OrganisationScopes.Add(
                new OrganisationScope {
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
            int firstYear = SectorTypeHelper.SnapshotDateHelper.FirstReportingYear;
            int lastYear = SectorTypeHelper.SnapshotDateHelper.CurrentSnapshotYear;

            Organisation testOrg = CreateOrgWithNoScopes(testOrgId, testSector, VirtualDateTime.Now);

            // for all snapshot years check if scope exists
            for (int year = firstYear; year < lastYear; year++)
            {
                testOrg.OrganisationScopes.Add(
                    new OrganisationScope {
                        OrganisationScopeId = 1,
                        Status = ScopeRowStatuses.Active,
                        SnapshotDate = new DateTime(year, snapshotDate.Month, snapshotDate.Day),
                        ScopeStatus = testScopeStatus
                    });
            }

            return testOrg;
        }

    }

}
