using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using ModernSlavery.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminDownloadsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminDownloadsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("download-feedback")]
        public FileContentResult DownloadFeedback()
        {
            List<Feedback> feedback = dataRepository.GetAll<Feedback>().ToList();

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            {
                using (var csv = new CsvWriter(writer,CultureInfo.CurrentCulture))
                {
                    csv.WriteRecords(feedback);
                }
            }

            var fileContentResult = new FileContentResult(memoryStream.GetBuffer(), "text/csv") {FileDownloadName = "Feedback.csv"};

            return fileContentResult;
        }

    }
}
