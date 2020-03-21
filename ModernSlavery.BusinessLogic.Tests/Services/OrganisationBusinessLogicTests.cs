using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class OrganisationBusinessLogicTests : BaseBusinessLogicTests
    {
        private List<Return> GetFourOrgsWithVariousReturns()
        {
            var result = new List<Return>();

            #region Two organisations with returns that have all their information filled correctly

            for (var i = 0; i < 2; i++)
            {
                var organisationInfoCorrect = OrganisationHelper.GetMockedOrganisation();
                var returnInfoCorrect = ReturnHelper.CreateTestReturn(organisationInfoCorrect);
                result.Add(returnInfoCorrect);
            }

            #endregion

            #region Organisation with return that doesn't contain bonus info

            var tempOrg = new Organisation();
            var tempReturn = new Return();
            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateTestReturnWithNoBonus(tempOrg);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            #region Organisation with return that has a bonus information completed as 0%

            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateBonusTestReturn(
                    tempOrg,
                    0,
                    0,
                    0,
                    0);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            #region Organisation with return that has a bonus information filled with negative numbers

            do
            {
                tempOrg = OrganisationHelper.GetMockedOrganisation();
                tempReturn = ReturnHelper.CreateBonusTestReturn(
                    tempOrg,
                    -15,
                    -34,
                    -56,
                    -78);
            } while (result.Any(x => x.OrganisationId == tempOrg.OrganisationId));

            result.Add(tempReturn);

            #endregion

            return result;
        }

        [Test]
        public async Task OrganisationBusinessLogic_GetCompareData_Leaves_Null_Values_At_The_Bottom_Of_The_ListAsync()
        {
            // Arrange
            var listOfReturns = GetFourOrgsWithVariousReturns();

            var listOfOrgs = listOfReturns.Select(ret => ret.Organisation);

            var mockedDataRepository = MoqHelpers.CreateMockDataRepository();

            mockedDataRepository.SetupGetAll(listOfOrgs, listOfReturns);

            var dataRepository = mockedDataRepository.Object;


            var mockedCommonBusinessLogic = Get<ICommonBusinessLogic>();

            var submissionBusinessLogic =
                new SubmissionBusinessLogic(mockedCommonBusinessLogic, dataRepository, Mock.Of<IRecordLogger>());

            var mockedScopeBusinessLogic = Get<IScopeBusinessLogic>();
            var mockedSecurityCodeBusinessLogic = Get<ISecurityCodeBusinessLogic>();
            var mockedEncryptionHandler = Get<IEncryptionHandler>();
            var mockedObfuscator = Get<IObfuscator>();

            var mockedDnBRepo = Get<IDnBOrgsRepository>();

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                mockedCommonBusinessLogic,
                dataRepository,
                submissionBusinessLogic,
                mockedScopeBusinessLogic,
                mockedEncryptionHandler,
                mockedSecurityCodeBusinessLogic,
                mockedDnBRepo,
                mockedObfuscator);

            var listEncOrgIds = listOfReturns.Select(x => mockedObfuscator.Obfuscate(x.OrganisationId.ToString()));
            var year = 2017;
            var sortColumn = "DiffMedianBonusPercent";
            var sortAscending = true;

            // Act
            var data = await organisationBusinessLogic.GetCompareDataAsync(
                listEncOrgIds,
                year,
                sortColumn,
                sortAscending);

            // Assert
        }
    }
}