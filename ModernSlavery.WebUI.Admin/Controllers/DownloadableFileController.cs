using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Services;
using ModernSlavery.Core;
using ModernSlavery.Core.Filters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Interfaces.Downloadable;
using ModernSlavery.Core.Models.Downloadable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Abstractions;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Route("~/")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DownloadableFileController : BaseController
    {

        private readonly IDownloadableFileBusinessLogic _downloadableFileBusinessLogic;

        #region Constructors

        public DownloadableFileController(
            ILogger<DownloadableFileController> logger,
            IWebService webService,
            IDownloadableFileBusinessLogic downloadableFileBusinessLogic,
            IDataRepository dataRepository, IFileRepository fileRepository) : base(logger, webService, dataRepository, fileRepository)
        {
            _downloadableFileBusinessLogic = downloadableFileBusinessLogic;
        }

        #endregion

        #region File Download Action

        [Route("download")]
        [AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.EhrcIPRange)]
        [ResponseCache(CacheProfileName = "Download")]
        public async Task<IActionResult> DownloadFile(string p)
        {
            IActionResult result;

            try
            {
                DownloadableFileModel downloadableFile = await _downloadableFileBusinessLogic.GetFileRemovingSensitiveInformationAsync(p);
                result = File(downloadableFile.ByteArrayContent, downloadableFile.ContentType, downloadableFile.Filename);
            }
            catch (ArgumentException argumentException)
            {
                result = BadRequest();
                Logger.LogError(argumentException, argumentException.Message);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                result = NotFound();
                Logger.LogError(fileNotFoundException, fileNotFoundException.Message);
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                result = NotFound();
                Logger.LogError(directoryNotFoundException, directoryNotFoundException.Message);
            }

            return result;
        }

        #endregion

        [HttpGet("admin/WebsiteLogs")]
        public async Task<IActionResult> WebsiteLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn = await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.WebUI");
            return View("WebsiteLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/WebjobLogs")]
        public async Task<IActionResult> WebjobLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.WebJob");
            return View("WebjobLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/IdentityLogs")]
        public async Task<IActionResult> IdentityLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.IdentityServer4");
            return View("IdentityLogs", downloadViewModelToReturn);
        }

        private async Task<IEnumerable<IDownloadableItem>> FetchDownloadablesFromSubfolderAsync(string fp, string subfolderName)
        {
            //var logsPathToProcess = string.IsNullOrEmpty(fp)
            //    ? Path.Combine(Global.LogPath, subfolderName)
            //    : fp;

            // Storage explorer, we DO want to change
            string logsPathToProcess = string.IsNullOrWhiteSpace(fp)
                ? Path.Combine(Global.LogPath, subfolderName).Replace("\\", "/")
                : fp;

            IEnumerable<IDownloadableItem> listOfDownloadableItems =
                await _downloadableFileBusinessLogic.GetListOfDownloadableItemsFromPathAsync(logsPathToProcess);
            return listOfDownloadableItems;
        }

    }
}
