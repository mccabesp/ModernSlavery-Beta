using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.DevOps.Testing;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core;
using ModernSlavery.WebUI.DevOps.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.DevOps.Controllers
{
    [Area("DevOps")]
    [Authorize(Roles = UserRoleNames.DevOpsAdmin)]
    [Route("devops")]
    [IPAddressFilter]
    public partial class DevOpsController : BaseController
    {
        #region Constructors
        private readonly ITestBusinessLogic _testBusinessLogic;
        public DevOpsController(
            ITestBusinessLogic testBusinessLogic,
            ILogger<DevOpsController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _testBusinessLogic = testBusinessLogic;
        }

        #endregion

        #region Home Action

        [HttpGet]
        public IActionResult Home()
        {
            var viewModel = new HomeViewModel
            {
            };

            return View(viewModel);
        }

        #endregion

        
        #region Trigger Webjobs Actions
        [HttpGet("trigger-webjobs")]
        public IActionResult TriggerWebjobs()
        {
            var viewModel = new TriggerWebjobsViewModel {
            };

            return View(viewModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("trigger-webjobs")]
        public async Task<IActionResult> TriggerWebjobs([Text] string webjobname)
        {
            if (!string.IsNullOrWhiteSpace(webjobname))
            {
                await _testBusinessLogic.QueueWebjob(webjobname);
                AddDisplayMessage($"Webjob {webjobname} successfully queued");
            }

            return RedirectToAction("TriggerWebjobs");
        }

        #endregion

        #region Load testing Actions
        [HttpGet("load-testing")]
        public IActionResult LoadTesting()
        {
            //Ensure cannot be run on production
            if (SharedBusinessLogic.TestOptions.IsProduction()) return new ForbidResult();

            var viewModel = new LoadTestingViewModel
            {
            };

            return View(viewModel);
        }

        [HttpPost("load-testing")]
        public async Task<IActionResult> LoadTestingAsync(LoadTestingViewModel viewModel, [IgnoreText] string action)
        {
            //Ensure cannot be run on production
            if (SharedBusinessLogic.TestOptions.IsProduction()) return new ForbidResult();

            var resetDatabase =false;
            var setUKAddresses = false;
            var deleteFiles = false;
            var deleteDrafts = false;
            var clearQueues = false;
            var resetSearch = false;
            var clearAppInsights = false;
            var clearCache = false;
            var clearLocalLogs = false;
            var clearLocalSettingsDump = false;

            switch (action)
            {
                case "FullReset":
                    resetDatabase = true;
                    deleteFiles = true;
                    clearQueues = true;
                    resetSearch = true;
                    clearAppInsights = true;
                    clearCache = true;
                    clearLocalLogs = true;
                    clearLocalSettingsDump = true;
                    break;
                case "ResetDatabase":
                    resetDatabase = true;
                    break;
                case "SetUKAddresses":
                    setUKAddresses = true;
                    break;
                case "DeleteFiles":
                    deleteFiles = true;
                    break;
                case "DeleteDrafts":
                    deleteDrafts = true;
                    break;
                case "ClearQueues":
                    clearQueues = true;
                    break;
                case "ResetSearch":
                    resetSearch = true;
                    break;
                case "ClearAppInsights":
                    clearAppInsights = true;
                    break;
                case "ClearCache":
                    clearCache = true;
                    break;
                case "DeleteLocalLogs":
                    clearLocalLogs = true;
                    break;
                case "DeleteSettingsDump":
                    clearLocalSettingsDump = true;
                    break;
                case "None":
                    ModelState.AddModelError("", "You must make a selection");
                    return View(viewModel);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action),$"Invalid {nameof(action)}='{action}'");
            }

            var tasks = new List<Task>();
            Task resetDatabaseTask = null;
            if (resetDatabase)
            {
                resetDatabaseTask = _testBusinessLogic.ResetDatabaseAsync(true);
                tasks.Add(resetDatabaseTask); 
            }

            Task setUKAddressesTask = null;
            if (setUKAddresses)
            {
                setUKAddressesTask = _testBusinessLogic.SetIsUkAddressesAsync();
                tasks.Add(setUKAddressesTask);
            }

            Task deleteFilesTask = null;
            if (deleteFiles)
            {
                deleteFilesTask = _testBusinessLogic.DeleteFilesAsync();
                tasks.Add(deleteFilesTask);
            }

            Task deleteDraftsTask = null;
            if (deleteDrafts)
            {
                deleteDraftsTask = _testBusinessLogic.DeleteDraftFilesAsync();
                tasks.Add(deleteDraftsTask);
            }

            Task clearQueuesTask = null;
            if (clearQueues) 
            {
                clearQueuesTask = _testBusinessLogic.ClearQueuesAsync();
                tasks.Add(clearQueuesTask); 
            }

            Task resetSearchTask = null;
            if (resetSearch)
            {
                resetSearchTask = _testBusinessLogic.ResetSearchIndexesAsync();
                tasks.Add(resetSearchTask);
            }

            Task clearAppInsightsTask = null;
            if (clearAppInsights)
            {
                clearAppInsightsTask=_testBusinessLogic.ClearAppInsightsAsync();
                tasks.Add(clearAppInsightsTask);
            }

            Task clearCacheTask = null;
            if (clearCache)
            {
                clearCacheTask= _testBusinessLogic.ClearCacheAsync();
                tasks.Add(clearCacheTask);
            }

            Task clearLocalLogsTask = null;
            if (clearLocalLogs)
            {
                clearLocalLogsTask = Task.Factory.StartNew(()=>_testBusinessLogic.ClearLocalLogs());
                tasks.Add(clearLocalLogsTask); 
            }

            Task clearLocalSettingsDumpTask = null;
            if (clearLocalSettingsDump)
            {
                clearLocalSettingsDumpTask= Task.Factory.StartNew(() => _testBusinessLogic.ClearLocalSettingsDump());
                tasks.Add(clearLocalSettingsDumpTask);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                    Logger.LogError(ex, ex.Message);
            }
            catch (Exception ex) 
            {
                Logger.LogError(ex,ex.Message);
            }

            if (resetDatabaseTask != null)
            {
                if (resetDatabaseTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Reset Database: {resetDatabaseTask.Exception.Message}");
                else
                    AddDisplayMessage("Database successfully cleared, reseeded and reset");
            }

            if (setUKAddressesTask != null)
            {
                if (setUKAddressesTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Set UK Addresses: {setUKAddressesTask.Exception.Message}");
                else
                    AddDisplayMessage("UK Addresses successfully set");
            }

            if (deleteFilesTask != null)
            {
                if (deleteFilesTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Delete Files: {deleteFilesTask.Exception.Message}");
                else
                    AddDisplayMessage("Files successfully deleted");
            }

            if (deleteDraftsTask != null)
            {
                if (deleteDraftsTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Delete Drafts: {deleteDraftsTask.Exception.Message}");
                else
                    AddDisplayMessage("Drafts successfully deleted");
            }

            if (clearQueuesTask != null)
            {
                if (clearQueuesTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Clear Queues: {clearQueuesTask.Exception.Message}");
                else
                    AddDisplayMessage("Queues successfully cleared");
            }

            if (resetSearchTask != null)
            {
                if (resetSearchTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Reset Search: {resetSearchTask.Exception.Message}");
                else
                    AddDisplayMessage("Search indexes successfully deleted and recreated");
            }

            if (clearAppInsightsTask != null)
            {
                if (clearAppInsightsTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Clear AppInsights: {clearAppInsightsTask.Exception.Message}");
                else
                    AddDisplayMessage("App Insights logs successfully queued for clearing and may take up to 72 hours");
            }
            

            if (clearCacheTask != null)
            {
                if (clearCacheTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Clear Cache: {clearCacheTask.Exception.Message}");
                else
                    AddDisplayMessage("Cache and session successfully cleared");
            }

            if (clearLocalLogsTask != null)
            {
                if (clearLocalLogsTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Delete local logs: {clearLocalLogsTask.Exception.Message}");
                else
                    AddDisplayMessage("Local log files successfully deleted");
            }

            if (clearLocalSettingsDumpTask != null)
            {
                if (clearLocalSettingsDumpTask.Status == TaskStatus.Faulted)
                    ModelState.AddModelError("", $"Delete local settings dump: {clearLocalSettingsDumpTask.Exception.Message}");
                else
                    AddDisplayMessage("Local file containing dump of app settings successfully deleted");
            }

            if (!ModelState.IsValid)return View(viewModel);

            return RedirectToAction("LoadTesting");
        }

        #endregion

        #region Environments Actions

        [HttpGet("environments")]
        public IActionResult Environments()
        {
            //Ensure cannot be run on production
            if (SharedBusinessLogic.TestOptions.IsProduction()) return new ForbidResult();

            var viewModel = new EnvironmentsViewModel {
            };

            return View(viewModel);
        }

        #endregion

        #region Disaster Recovery Actions

        [HttpGet("disaster-recovery")]
        public IActionResult DisasterRecovery()
        {
            //Ensure cannot be run on production
            if (SharedBusinessLogic.TestOptions.IsProduction()) return new ForbidResult();

            var viewModel = new DisasterRecoveryViewModel {
            };

            return View(viewModel);
        }

        #endregion
    }
}
