﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.BusinessDomain;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;
using Moq;

namespace ModernSlavery.Tests.Common.Classes
{
    public static class MoqHelpers
    {
        public static ISharedBusinessLogic CreateFakeSharedBusinessLogic()
        {
            var mockedSnapshotDateHelper = new Mock<ISnapshotDateHelper>();
            var mockedSourceComparer = new Mock<ISourceComparer>();
            var mockedSendEmailService = new Mock<ISendEmailService>();
            var mockedNotificationService = new Mock<INotificationService>();
            var mockedFileRepository = new Mock<IFileRepository>();
            var mockedDataRepository = new Mock<IDataRepository>();
            var mockSharedBusinessLogic = new SharedBusinessLogic(mockedSnapshotDateHelper.Object,
                mockedSourceComparer.Object, mockedSendEmailService.Object, mockedNotificationService.Object,
                mockedFileRepository.Object, mockedDataRepository.Object);

            return mockSharedBusinessLogic;
        }

        public static ISubmissionBusinessLogic CreateFakeSubmissionBusinessLogic()
        {
            var fakeSharedBusinessLogic = CreateFakeSharedBusinessLogic();
            var fakeSubmissionBusinessLogic = new SubmissionBusinessLogic(fakeSharedBusinessLogic,
                fakeSharedBusinessLogic.DataRepository, Mock.Of<IAuditLogger>());
            return fakeSubmissionBusinessLogic;
        }

        public static ISearchBusinessLogic CreateFakeSearchBusinessLogic()
        {
            var fakeSearchBusinessLogic = new SearchBusinessLogic(Mock.Of<ISearchRepository<EmployerSearchModel>>(),
                Mock.Of<ISearchRepository<SicCodeSearchModel>>(), Mock.Of<IAuditLogger>());
            return fakeSearchBusinessLogic;
        }

        public static IScopeBusinessLogic CreateFakeScopeBusinessLogic()
        {
            var fakeSharedBusinessLogic = CreateFakeSharedBusinessLogic();
            var fakeSearchBusinessLogic = CreateFakeSearchBusinessLogic();
            var fakeScopeBusinessLogic = new ScopeBusinessLogic(fakeSharedBusinessLogic,
                fakeSharedBusinessLogic.DataRepository, fakeSearchBusinessLogic, null);
            return fakeScopeBusinessLogic;
        }

        public static IDraftFileBusinessLogic CreateFakeDraftBusinessLogic()
        {
            var fakeSharedBusinessLogic = CreateFakeSharedBusinessLogic();
            var fakeDraftBusinessLogic = new DraftFileBusinessLogic(null, fakeSharedBusinessLogic.FileRepository);
            return fakeDraftBusinessLogic;
        }

        public static ISubmissionService CreateFakeSubmissionService()
        {
            var fakeSharedBusinessLogic = CreateFakeSharedBusinessLogic();
            var fakeSubmissionBusinessLogic = CreateFakeSubmissionBusinessLogic();
            var fakeScopeBusinessLogic = CreateFakeScopeBusinessLogic();
            var fakeDraftBusinessLogic = CreateFakeDraftBusinessLogic();

            var fakeSubmissionService = new SubmissionService(fakeSharedBusinessLogic, fakeSubmissionBusinessLogic,
                fakeScopeBusinessLogic, fakeDraftBusinessLogic);
            return fakeSubmissionService;
        }

        public static Mock<IDataRepository> CreateMockDataRepository()
        {
            var mockDataRepo = new Mock<IDataRepository>();
            mockDataRepo.SetupEmptyDataRepository();
            return mockDataRepo;
        }

        public static void SetupEmptyDataRepository(this Mock<IDataRepository> mockDataRepo)
        {
            mockDataRepo.SetupGetAllItemsOfType(new AddressStatus[] { });
            mockDataRepo.SetupGetAllItemsOfType(new Organisation[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationAddress[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationName[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationReference[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationScope[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationSicCode[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationStatus[] { });
            mockDataRepo.SetupGetAllItemsOfType(new Return[] { });
            mockDataRepo.SetupGetAllItemsOfType(new ReturnStatus[] { });
            mockDataRepo.SetupGetAllItemsOfType(new SicCode[] { });
            mockDataRepo.SetupGetAllItemsOfType(new SicSection[] { });
            mockDataRepo.SetupGetAllItemsOfType(new User[] { });
            mockDataRepo.SetupGetAllItemsOfType(new UserOrganisation[] { });
            mockDataRepo.SetupGetAllItemsOfType(new UserSetting[] { });
            mockDataRepo.SetupGetAllItemsOfType(new UserStatus[] { });
        }

        public static Mock<IDataRepository> SetupGetAll(this Mock<IDataRepository> mockDataRepo, params object[] items)
        {
            var objects = new List<object>();
            foreach (var item in items)
            {
                var enumerable = item as IEnumerable<object>;
                if (enumerable == null)
                    objects.Add(item);
                else
                    objects.AddRange(enumerable);
            }

            mockDataRepo.SetupGetAll(objects);

            return mockDataRepo;
        }

        public static IOptionsSnapshot<T> CreateIOptionsSnapshotMock<T>(T value) where T : class, new()
        {
            var mock = new Mock<IOptionsSnapshot<T>>();
            mock.Setup(m => m.Value).Returns(value);
            return mock.Object;
        }

        private static void SetupGetAll(this Mock<IDataRepository> mockDataRepo, IEnumerable<object> items)
        {
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<AddressStatus>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<Organisation>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationAddress>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationName>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationReference>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationScope>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationSicCode>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationStatus>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<Return>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<SicCode>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<SicSection>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<User>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<UserOrganisation>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<UserSetting>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<UserStatus>());
        }

        private static void SetupGetAllItemsOfType<T>(this Mock<IDataRepository> mockDataRepo, IEnumerable<T> items)
            where T : class
        {
            var list = items.OfType<T>().ToList();
            if (!list.Any()) list = new List<T>(new T[] { });

            var mockItems = list.AsQueryable().BuildMock();
            mockDataRepo.Setup(x => x.GetAll<T>()).Returns(mockItems.Object);
        }
    }
}