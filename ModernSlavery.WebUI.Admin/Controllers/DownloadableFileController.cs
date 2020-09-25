using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces.Downloadable;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DownloadableFileController : BaseController
    {
        private readonly IDownloadableFileBusinessLogic _downloadableFileBusinessLogic;

        #region Constructors

        public DownloadableFileController(
            IDownloadableFileBusinessLogic downloadableFileBusinessLogic,
            ILogger<DownloadableFileController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _downloadableFileBusinessLogic = downloadableFileBusinessLogic;
        }

        #endregion

        #region File Download Action

        [HttpGet("admin/downloadfile")]
        [AllowOnlyTrustedDomains]
        [ResponseCache(CacheProfileName = "Download")]
        public async Task<IActionResult> DownloadFile(string p)
        {
            IActionResult result;

            try
            {
                var downloadableFile = await _downloadableFileBusinessLogic.GetFileRemovingSensitiveInformationAsync(p);
                result = File(downloadableFile.ByteArrayContent, downloadableFile.ContentType,
                    downloadableFile.Filename);
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
            var downloadViewModelToReturn = await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.Hosts.Web");
            return View("WebsiteLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/WebjobLogs")]
        public async Task<IActionResult> WebjobLogs(string fp)
        {
            var downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.Hosts.Webjob");
            return View("WebjobLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/IdentityLogs")]
        public async Task<IActionResult> IdentityLogs(string fp)
        {
            var downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "ModernSlavery.Hosts.IdServer");
            return View("IdentityLogs", downloadViewModelToReturn);
        }

        private async Task<IEnumerable<IDownloadableItem>> FetchDownloadablesFromSubfolderAsync(string fp,
            string subfolderName)
        {
            //var logsPathToProcess = string.IsNullOrEmpty(fp)
            //    ? Path.Combine(SharedBusinessLogic.SharedOptions.LogPath, subfolderName)
            //    : fp;

            // Storage explorer, we DO want to change
            var logsPathToProcess = string.IsNullOrWhiteSpace(fp)
                ? Path.Combine(SharedBusinessLogic.SharedOptions.LogPath, subfolderName).Replace("\\", "/")
                : fp;

            var listOfDownloadableItems =
                await _downloadableFileBusinessLogic.GetListOfDownloadableItemsFromPathAsync(logsPathToProcess);
            return listOfDownloadableItems;
        }
    }
}