﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
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
        [IPAddressFilter]
        public async Task<IActionResult> DownloadFile([IgnoreText] string p)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

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
    }
}