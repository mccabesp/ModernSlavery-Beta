using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Models.Organisation;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Classes.Options;
using ModernSlavery.WebUI.Presenters;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Services.Submission
{

    public class GetAllEditableReportsAsyncTests
    {

        private ICommonBusinessLogic mockCommonBusinessLogic;
        private Mock<IDataRepository> mockDataRepo;
        private Mock<IDraftFileBusinessLogic> mockDraftFileBL;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [SetUp]
        public void BeforeEach()
        {
            mockCommonBusinessLogic = MoqHelpers.CreateFakeCommonBusinessLogic();
            mockDataRepo = MoqHelpers.CreateMockDataRepository();
            mockScopeBL = new Mock<IScopeBusinessLogic>();
            mockDraftFileBL = new Mock<IDraftFileBusinessLogic>();
        }

        [TestCase(SectorTypes.Public, 3)]
        [TestCase(SectorTypes.Public, 2)]
        [TestCase(SectorTypes.Public, 1)]
        [TestCase(SectorTypes.Private, 3)]
        [TestCase(SectorTypes.Private, 2)]
        [TestCase(SectorTypes.Private, 1)]
        public async Task ReportCountIsControlledBySubmissionOptions(SectorTypes testSector, int testEditableReportCount)
        {
            // Arrange
            var testConfig = new SubmissionOptions {EditableReportCount = testEditableReportCount};
            var testOrg = new Organisation {OrganisationId = 1, SectorType = testSector};
            var testUserOrg = new UserOrganisation {Organisation = testOrg};
            DateTime testSnapshotDate = mockCommonBusinessLogic.GetAccountingStartDate(testOrg.SectorType);

            var mockService = new Mock<SubmissionPresenter>(
                mockDataRepo.Object,
                mockScopeBL.Object,
                null,
                mockDraftFileBL.Object,
                MoqHelpers.CreateIOptionsSnapshotMock(testConfig));

            // Call the real functions unless overridden
            mockService.CallBase = true;

            // Act
            SubmissionPresenter testService = mockService.Object;
            List<ReportInfoModel> actualResults = await testService.GetAllEditableReportsAsync(testUserOrg, testSnapshotDate);

            // Assert
            Assert.AreEqual(
                testEditableReportCount,
                actualResults.Count,
                $"Expected editable report count to be {testEditableReportCount}");
        }

    }

}
