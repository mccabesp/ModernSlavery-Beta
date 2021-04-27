using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.BasicAdmin)]
    [Route("admin")]
    [NoCache]
    public class AdminDownloadsController : BaseController
    {
        private readonly IAdminService _adminService;
        public AdminDownloadsController(IAdminService adminService, ILogger<AdminDownloadsController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
        }

        [HttpGet("download-feedback")]
        public async Task<IActionResult> DownloadFeedbackAsync()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var feedback = _adminService.SharedBusinessLogic.DataRepository.GetAll<Feedback>().ToList();

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                config.ShouldQuote = (field, context) => true;
                config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;
                config.MissingFieldFound = null;
                config.IgnoreReferences = true; //Otherwise virtual properties are set with weird values

                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecords(feedback);
                }
            }

            var fileContentResult = new FileContentResult(memoryStream.GetBuffer(), "text/csv")
                {FileDownloadName = "Feedback.csv"};

            return fileContentResult;
        }
    }
}