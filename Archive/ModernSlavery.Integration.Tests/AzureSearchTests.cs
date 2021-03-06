using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.BusinessDomain;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Viewing.Models;
using ModernSlavery.WebUI.Viewing.Presenters;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Integration.Tests
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AzureSearchTests
    {
        private readonly AzureEmployerSearchRepository _azureSearchRepo;

        public AzureSearchTests()
        {
            SetupHelpers.SetupMockLogRecordGlobals();

            //_azureSearchRepo = new AzureSearchRepository(azureSearchServiceName, azureSearchAdminApiKey, null, SharedOptions.AppInsightsClient);
            _azureSearchRepo = new AzureEmployerSearchRepository(ConfigHelpers.SharedOptions,
                Mock.Of<IAuditLogger>(), ConfigHelpers.SearchOptions.AzureServiceName,
                ConfigHelpers.SearchOptions.EmployerIndexName, ConfigHelpers.SearchOptions.AzureApiAdminKey);
        }

        /* Tests that fail if PartialNameForSuffixSearches is missing */
        [TestCase("Employment", 17921)] // 24-7 EMPLOYMENT SOLUTIONS LIMITED
        [TestCase("coopers", 268)] // "Pricewaterhousecoopers llp"
        [TestCase("Cleaning", 3131)] // "CHATFIELD CLEANING LIMITED"
        /* Tests that fail if Abbreviations are missing */
        /* Ignored because its position has changed to 12, possibly due to more elements having been added to the index, these tests are too reliant on the data within the index and must be modified (this was already discussed with Stephen and it's a task that will be completed/reviewed at a future date) */
        // [TestCase("B.A.T", 14501)] // Abreviation of "Berlesduna Academy Trust"
        [TestCase(
            "w.u.t.h.n.f.t",
            13852)] // [orgId:13852, orgName:Wirral University Teaching Hospital Nhs Foundation Trust]
        [TestCase("wuthnft", 13852)] // [orgId:13852, orgName:Wirral University Teaching Hospital Nhs Foundation Trust]
        /* These DO return results */
        [TestCase("pricewaterhousecoopers", 268)]
        [TestCase("Pricewaterhousecoopers llp", 268)]
        [TestCase("Pricewaterhousecoopers Services Limited", 267)]

        // Because of prefix analyzer
        [TestCase("pricewater*", 268)]
        [TestCase("pricewa", 268)]

        // Because of the suffix analyzer
        [TestCase("3 way cleaning", 17503)] // "3 way cleaning limited"
        [TestCase("bar", 1794)] // "BAR 2010 LIMITED"
        [TestCase("Bedfordshire Fire & Rescue Service", 1944)]
        [TestCase(
            "snc-lavalan rail & transit limited",
            14734)] // snc-lavalin rail & transit limited - NOTE: misspelling of 'laval_i_n'
        [TestCase("happy", 14663)] // "Happy days south west limited"
        [TestCase("good", 5614)] // "Good Energy limited"
        [TestCase("Cambridgeshire Fire & Rescue Service", 14276)]
        [TestCase("Leicestershire Fire and Rescue Service", 16539)]
        [TestCase("London Fire & Emergency Planning Authority", 7804)]
        [TestCase("Nottinghamshire Fire and Rescue Service", 9183)]
        [TestCase("Staffordshire Fire & Rescue Services", 11630)]
        [TestCase("A.B.M..", 733)] // additional dot at the end
        [TestCase("A.B.M.", 733)]
        [TestCase("ABM", 733)]
        [TestCase(
            "A.B.M catering limited",
            733)] // missing a dot between 'M' and 'Catering' (db entry reads 'A.B.M. CATERING LIMITED')
        [TestCase("A.B.M", 733)]
        [TestCase(
            "coop",
            12302)] // expected to find "the co-operative bank plc" because of the synonymMap added to the index
        [TestCase(
            "the coop",
            12302)] // expected to find "the co-operative bank plc" because of the synonymMap added to the index
        [TestCase(
            "co-operative",
            12302)] // expected to find "the co-operative bank plc" because of the synonymMap added to the index
        [TestCase("BRIGHTSTAR 20:20 UK LIMITED", 2402)]
        [TestCase("a. & b. glass company limited", 729)]
        [TestCase("BBC", 1887)]
        [TestCase("Glaxosmithkline Services", 5533)] // "Glaxosmithkline Services Unlimited"
        [TestCase("Glaxosmithkline Consumer Healthcare(UK) Trading Limited", 5529)]
        [TestCase("yorkshire building society", 14480)]
        [TestCase("24 X 7 Ltd.", 695)] // 24 X 7 Ltd.
        [TestCase("24 x7", 695)]
        [TestCase("24", 695)]
        [TestCase("24x", 695)] // expecting "24 x 7 Ltd."
        [TestCase("24-7 EMPLOYMENT SOLUTIONS LIMITED", 17921)] // 24-7 EMPLOYMENT SOLUTIONS LIMITED
        [TestCase("Employment Solutions", 17921)]
        /* These do NOT return results - in other words, need further investigation */
        // [TestCase("J C B", 733)]

        // [TestCase("ricewater", 99999)] // searching for 'pricewaterhousecoopers' :: middle of the word, 'infix' search in azure search needs further investigation.
        // https://social.msdn.microsoft.com/Forums/en-US/dbbc078d-af02-4d90-b213-944fd77c9213/how-to-use-wildcard-like-contains-search-in-azure-search
        // https://social.msdn.microsoft.com/Forums/azure/en-US/c23446db-17af-4b99-b76f-1a6b006612a3/azure-search-net-sdk-filter-and-startwith
        // https://stackoverflow.com/questions/40056213/behavior-of-asterisk-in-azure-search-service
        // https://stackoverflow.com/questions/40857057/how-to-practially-use-a-keywordanalyzer-in-azure-search
        public async Task ViewingService_Search_Returns_At_Least_One_Result(string searchKeyWords,
            int expectedOrganisationId)
        {
            // Arrange
            var mockDataRepo = MoqHelpers.CreateMockDataRepository();

            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            var mockedSearchParameters =
                Mock.Of<EmployerSearchParameters>(
                    x => x.Keywords == searchKeyWords && x.Page == 1 && x.SearchType == SearchTypes.ByEmployerName);

            // Act
            var result = await testPresenter.SearchAsync(mockedSearchParameters);

            // Assert
            var totalNumberOfRecordsFound = result.Employers.ActualRecordTotal;
            Assert.GreaterOrEqual(
                totalNumberOfRecordsFound,
                1,
                $"Expected term [{searchKeyWords}] to return at least one record.");

            var organisationFound =
                result.Employers.Results.FirstOrDefault(x => x.OrganisationId == expectedOrganisationId.ToString());

            Assert.NotNull(
                organisationFound,
                $"Search term \"{searchKeyWords}\" should have returned organisationId {expectedOrganisationId} within its list of results but it was NOT FOUND");

            var organisationPosition = result.Employers.Results.IndexOf(organisationFound);

            Assert.LessOrEqual(
                organisationPosition,
                10,
                $"Search term \"{searchKeyWords}\" should have returned organisationId {expectedOrganisationId} within the top 10 list of results but it was present in position {organisationPosition}");

            // This last assertion is to make sure that the search term hasn't returned the whole database - some 11k records (so to confirmed that the query was NOT transformed to be a search 'all', search=*)
            Assert.LessOrEqual(totalNumberOfRecordsFound, 9000);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task ViewingService_SearchByEmployerName_Null_Or_Empty_Search_Returns_All(
            string nullWhitespaceOrEmptySearchKeyWords)
        {
            // Arrange
            var mockDataRepo = MoqHelpers.CreateMockDataRepository();

            const SearchTypes searchBy = SearchTypes.ByEmployerName;
            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            var mockedSearchParameters =
                Mock.Of<EmployerSearchParameters>(
                    x => x.Keywords == nullWhitespaceOrEmptySearchKeyWords && x.Page == 1 && x.SearchType == searchBy);

            // Act
            var result = await testPresenter.SearchAsync(mockedSearchParameters);

            // Assert
            var actualNumberOfRecords = result.Employers.ActualRecordTotal;
            Assert.GreaterOrEqual(
                actualNumberOfRecords,
                10000,
                $"Expected term [{nullWhitespaceOrEmptySearchKeyWords}], when searching {searchBy} to return the whole index (10k records or more).");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task ViewingService_BySectorType_Null_Or_Empty_Search_Returns_Zero(
            string nullWhitespaceOrEmptySearchKeyWords)
        {
            // Arrange
            var mockDataRepo = MoqHelpers.CreateMockDataRepository();

            const SearchTypes searchType = SearchTypes.BySectorType;
            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            var mockedSearchParameters =
                Mock.Of<EmployerSearchParameters>(
                    x => x.Keywords == nullWhitespaceOrEmptySearchKeyWords && x.Page == 1 &&
                         x.SearchType == searchType);

            // Act
            var result = await testPresenter.SearchAsync(mockedSearchParameters);

            // Assert
            var actualNumberOfRecords = result.Employers.ActualRecordTotal;
            Assert.AreEqual(
                0,
                actualNumberOfRecords,
                $"Expected term [{nullWhitespaceOrEmptySearchKeyWords}], when searching {searchType} to return no records.");
        }

        [TestCase("Wolseley uk limited", 200)]
        [TestCase("Wolseley uk", 200)]
        [TestCase("Wolseley", 200)]
        public async Task ViewingService_Search_Ignores_Terms_Limited_And_Uk_(string searchKeyWords,
            int maxNumberOfRecordsExpected)
        {
            // Arrange
            var mockDataRepo = MoqHelpers.CreateMockDataRepository();

            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            var mockedSearchParameters =
                Mock.Of<EmployerSearchParameters>(
                    x => x.Keywords == searchKeyWords && x.Page == 1 && x.SearchType == SearchTypes.ByEmployerName);

            // Act
            var result = await testPresenter.SearchAsync(mockedSearchParameters);

            // Assert
            var totalNumberOfRecordsFound = result.Employers.ActualRecordTotal;
            Assert.LessOrEqual(totalNumberOfRecordsFound, maxNumberOfRecordsExpected);
        }

        [TestCase(null, 0)]
        [TestCase("", 0)]
        [TestCase("   ", 0)]
        [TestCase("Accommodation", 9)]
        [TestCase("Supermarket", 2)]
        [TestCase("Bank", 2)]
        [TestCase("acco*", 11)] // accounting, accommodation
        [TestCase("01210", 1)] // [confirm it's been indexed correctly -> zero leading]  01210 - Growing of grapes
        [TestCase("46310", 1)] // 46310 - Wholesale of fruit and vegetables
        [TestCase("591", 8)] // 59111-Motion picture... + 59112-Video... + 59113-Television...
        public async Task ViewingService_GetListOfSicCodeSuggestions(string searchKeyWords,
            int expectedNumberOfSuggestions)
        {
            // Arrange
            var sicCodeSearchServiceClient = new SearchServiceClient(
                ConfigHelpers.SearchOptions.AzureServiceName,
                new SearchCredentials(ConfigHelpers.SearchOptions.AzureApiAdminKey));

            var sicCodeSearchIndexClient = new AzureSicCodeSearchRepository(Mock.Of<IAuditLogger>(),
                sicCodeSearchServiceClient, ConfigHelpers.SearchOptions.SicCodeIndexName);

            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            mockViewingService.Setup(m => m.SearchBusinessLogic.SicCodeSearchRepository)
                .Returns(sicCodeSearchIndexClient);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            // Act
            var result = await testPresenter.GetListOfSicCodeSuggestionsAsync(searchKeyWords);

            // Assert
            var actualNumberOfSuggestions = result.Count;
            Assert.AreEqual(
                expectedNumberOfSuggestions,
                actualNumberOfSuggestions,
                $"Expected term [{searchKeyWords}] to return {expectedNumberOfSuggestions} suggestion(s).");
        }

        [Description(
            "Users searching by several terms expect the search function to select all sector descriptions that contain ALL the terms in the search box. Therefore the result set must contain the list of organisations linked to either of the relevant sicCodeId's."
            + ""
            + "Example: 'mining ore' will currently find two sectors that contain 'mining' and 'ore', which are: '(7290) Mining of other non ~ ferrous metal ores' and '(7100) Mining of iron ores', so the expected result of the search should be all organisations linked to (7290 ~ currently 5 orgs linked) AND those linked to (7100 ~ currently 2 orgs linked), making it a grand total of 7 organisations returned by the search function")]
        [TestCase(
            "perennial crop*",
            "1190~Growing of other non-perennial crops; 1290~Growing of other perennial crops")]
        [TestCase(
            "fruit and vegetable*",
            "46310~Wholesale of fruit and vegetables; 10390~Other processing and preserving of fruit and vegetables; 47210~Retail sale of fruit and vegetables in specialised stores; 46341~Wholesale of fruit and vegetable juices, mineral water and soft drink; 10320~Manufacture of fruit and vegetable juice")]
        [TestCase("bank*", "64191~banks; 64110~central banking; 86900~Other human health activities")]
        public async Task ViewingService_Search_BySectorType_Selects_Sectors_That_Contain_All_Terms(
            string searchKeyWords,
            string csvListOfSicSectionsExpectedToBeSelected)
        {
            // Arrange
            var sicCodeSearchServiceClient = new SearchServiceClient(
                ConfigHelpers.SearchOptions.AzureServiceName,
                new SearchCredentials(ConfigHelpers.SearchOptions.AzureApiAdminKey));

            var sicCodeSearchIndexClient = new AzureSicCodeSearchRepository(Mock.Of<IAuditLogger>(),
                sicCodeSearchServiceClient, ConfigHelpers.SearchOptions.SicCodeIndexName);

            var mockDataRepo = MoqHelpers.CreateMockDataRepository();

            var mockViewingService = new Mock<IViewingService>();
            mockViewingService.Setup(m => m.SearchBusinessLogic.EmployerSearchRepository).Returns(_azureSearchRepo);
            mockViewingService.Setup(m => m.SearchBusinessLogic.SicCodeSearchRepository)
                .Returns(sicCodeSearchIndexClient);
            var testPresenter = new ViewingPresenter(mockViewingService.Object, Mock.Of<ISharedBusinessLogic>());

            #region Calculate the expected number of records

            // Convert the received csv list to a list of EmployerSearchParameters
            var listOfSicSectionDescriptionsToBeSelected = csvListOfSicSectionsExpectedToBeSelected
                .SplitI(";")
                .Select(
                    sicSectionDescriptionFoundInList =>
                    {
                        var sicCodeToSearchBy = sicSectionDescriptionFoundInList.SplitI("~")[1].Trim();

                        return Mock.Of<EmployerSearchParameters>(
                            searchParam =>
                                searchParam.Keywords == sicCodeToSearchBy
                                && searchParam.Page == 1
                                && searchParam.PageSize == 3000
                                && searchParam.SearchFields == $"{nameof(EmployerSearchModel.SicCodeIds)}"
                                && searchParam.SearchType == SearchTypes.ByEmployerName
                                && searchParam.SearchMode == SearchModes.All);
                    });

            var listOfEmployerSearchModel = new List<EmployerSearchModel>();
            foreach (var sicSectionDescriptionSearchParameter in listOfSicSectionDescriptionsToBeSelected)
            {
                var sicSectionDescriptionSearchViewModel =
                    await testPresenter.SearchAsync(sicSectionDescriptionSearchParameter);
                Assert.GreaterOrEqual(
                    sicSectionDescriptionSearchViewModel.EmployerEndIndex,
                    1,
                    $"When searching for {sicSectionDescriptionSearchParameter.Keywords} we expect to find at least one organisation in the index.");
                listOfEmployerSearchModel.AddRange(sicSectionDescriptionSearchViewModel.Employers.Results);
            }

            var expectedListOfEmployerNames =
                listOfEmployerSearchModel.Select(x => x.Name).DistinctI().OrderBy(x => x);

            #endregion

            var mockedSearchParameters =
                Mock.Of<EmployerSearchParameters>(
                    x => x.Keywords == searchKeyWords
                         && x.Page == 1
                         && x.PageSize == 3000
                         && x.SearchFields == $"{nameof(EmployerSearchModel.SicCodeIds)}"
                         && x.SearchType == SearchTypes.ByEmployerName
                         && x.SearchMode == SearchModes.All);

            // Act
            var keywordSearchResultSearchViewModel = await testPresenter.SearchAsync(mockedSearchParameters);

            // Assert
            var actualListOfEmployerNames =
                keywordSearchResultSearchViewModel.Employers.Results.Select(x => x.Name).DistinctI().OrderBy(x => x);

            var foundMoreRecordsThanThoseExpected = actualListOfEmployerNames.Except(expectedListOfEmployerNames);
            Assert.IsEmpty(foundMoreRecordsThanThoseExpected);

            var expectedMoreRecordsThanThoseFound = expectedListOfEmployerNames.Except(actualListOfEmployerNames);
            Assert.IsEmpty(expectedMoreRecordsThanThoseFound);
        }
    }
}