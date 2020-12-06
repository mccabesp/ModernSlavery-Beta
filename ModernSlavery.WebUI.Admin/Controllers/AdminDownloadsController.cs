using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    [NoCache]
    public class AdminDownloadsController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminDownloadsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("download-feedback")]
        public FileContentResult DownloadFeedback()
        {
            var feedback = _adminService.SharedBusinessLogic.DataRepository.GetAll<Feedback>().ToList();

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
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