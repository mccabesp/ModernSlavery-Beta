using System;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Submit;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Presenters;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Services.Submission
{

    public class IsCurrentSnapshotYearTests
    {

        private ISubmissionService mockSubmissionService;
        private ICommonBusinessLogic mockCommonBusinessLogic;
        private Mock<IDataRepository> mockDataRepo;
        private Mock<IDraftFileBusinessLogic> mockDraftFileBL;
        private Mock<IFileRepository> mockFileRepo;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [SetUp]
        public void BeforeEach()
        {
            mockCommonBusinessLogic = MoqHelpers.CreateFakeCommonBusinessLogic();
            mockDataRepo = MoqHelpers.CreateMockDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
            mockFileRepo = new Mock<IFileRepository>();
        }

        [TestCase(SectorTypes.Private, 2017)]
        [TestCase(SectorTypes.Public, 2017)]
        [TestCase(SectorTypes.Private, 2018)]
        [TestCase(SectorTypes.Public, 2018)]
        public void ReturnsFalseWhenNotCurrentYear(SectorTypes testSector, int testYear)
        {
            // Arrange
            DateTime testSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testSector);
            var expectCalledGetSnapshotDate = false;

            // Mocks
            var mockService = new Mock<SubmissionPresenter>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockFileRepo.Object,
                mockDraftFileBL.Object,
                null);
            mockService.CallBase = true;

            // Override GetPreviousReportingStartDate and return expectedYear
            mockService.Setup(ss => ss.GetSnapshotDate(It.IsIn(testSector), It.IsAny<int>()))
                .Returns(
                    () => {
                        expectCalledGetSnapshotDate = true;
                        return testSnapshotDate;
                    });

            // Assert
            SubmissionPresenter testService = mockService.Object;
            bool actual = testService.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsTrue(expectCalledGetSnapshotDate, "Expected to call GetSnapshotDate");
            Assert.IsFalse(actual, "Expected IsCurrentSnapshotYear to return true");
        }

        [TestCase(SectorTypes.Private)]
        [TestCase(SectorTypes.Public)]
        public void ReturnsTrueForCurrentYear(SectorTypes testSector)
        {
            // Arrange
            DateTime testSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testSector);
            int testYear = testSnapshotDate.Year;

            // Mocks
            var testSubmissionService = new SubmissionService(mockCommonBusinessLogic, Mock.Of<ISubmissionBusinessLogic>(), Mock.Of<IScopeBusinessLogic>(), Mock.Of<IDraftFileBusinessLogic>());
            var testPresenter = new SubmissionPresenter(testSubmissionService, null);

            // Assert
            bool actual = testPresenter.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsTrue(actual, "Expected IsCurrentSnapshotYear to return true");
        }

    }

}
