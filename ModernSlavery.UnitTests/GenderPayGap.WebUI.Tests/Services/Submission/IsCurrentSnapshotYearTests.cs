﻿using System;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Tests.Services.SubmissionService
{

    public class IsCurrentSnapshotYearTests
    {

        private ICommonBusinessLogic mockCommonBusinessLogic;
        private Mock<IDataRepository> mockDataRepo;
        private Mock<IDraftFileBusinessLogic> mockDraftFileBL;
        private Mock<IFileRepository> mockFileRepo;
        private Mock<IScopeBusinessLogic> mockScopeBL;

        [SetUp]
        public void BeforeEach()
        {
            mockCommonBusinessLogic = new CommonBusinessLogic(Config.Configuration);
            mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
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
            var mockService = new Mock<WebUI.Classes.Services.SubmissionService>(
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
            WebUI.Classes.Services.SubmissionService testService = mockService.Object;
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
            var testService = new WebUI.Classes.Services.SubmissionService(
                mockDataRepo.Object,
                mockScopeBL.Object,
                mockFileRepo.Object,
                mockDraftFileBL.Object,
                null);

            // Assert
            bool actual = testService.IsCurrentSnapshotYear(testSector, testYear);

            Assert.IsTrue(actual, "Expected IsCurrentSnapshotYear to return true");
        }

    }

}