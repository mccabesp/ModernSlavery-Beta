using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebJob.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using Moq;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel;

using NUnit.Framework;
using OrganisationHelper = ModernSlavery.WebJob.Tests.TestHelpers.OrganisationHelper;

namespace ModernSlavery.WebJob.Tests.Functions
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateDownloadsFileTests
    {

        [SetUp]
        public void BeforeEach()
        {
            //Create 10 test orgs with returns
            IEnumerable<Organisation> orgs = OrganisationHelper.CreateTestOrganisations(10);
            IEnumerable<Return> returns = ReturnHelper.CreateTestReturns(orgs);

            //Instantiate the dependencies
            _functions = WebJobTestHelper.SetUp(orgs, returns);
        }

        private WebJob.Functions _functions;

        [Test]
        [Description("Check download file populated OK")]
        public async Task FunctionsUpdateFile_UpdateDownloads_AddAllReturns()
        {
            //ARRANGE
            var log = new Mock<ILogger>();

            int year = SectorTypes.Private.GetAccountingStartDate(2017).Year;
            IEnumerable<Return> returns = await _functions.CommonBusinessLogic.DataRepository
                .ToListAsync<Return>(
                    r => r.AccountingDate.Year == year
                         && r.Status == ReturnStatuses.Submitted
                         && r.Organisation.Status == OrganisationStatuses.Active);

            //ACT
            await _functions.UpdateDownloadFilesAsync(log.Object, "testadmin@user.com", true);

            //ASSERT
            //Check each return is in the download file
            string downloadFilePath =
                _functions.CommonBusinessLogic.FileRepository.GetFullPath(Path.Combine(ConfigHelpers.GlobalOptions.DownloadsLocation, $"GPGData_{year}-{year + 1}.csv"));

            //Check the file exists
            Assert.That(await _functions.CommonBusinessLogic.FileRepository.GetFileExistsAsync(downloadFilePath), $"File '{downloadFilePath}' should exist");

            //Get the actual results
            List<DownloadResult> actualResults = await _functions.CommonBusinessLogic.FileRepository.ReadCSVAsync<DownloadResult>(downloadFilePath);

            //Generate the expected results
            List<DownloadResult> expectedResults = returns.Select(r => DownloadResult.Create(r)).OrderBy(d => d.EmployerName).ToList();

            //Check the results
            expectedResults.Compare(actualResults);
        }

    }
}
