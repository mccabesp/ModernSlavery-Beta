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
using ModernSlavery.Core.Options;
using System.Linq;
using ModernSlavery.Core.Extensions;
using System.IO;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

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
        private readonly IDisasterRecoveryBusinessLogic _disasterRecoveryBusinessLogic;
        private readonly DevOpsOptions _devopsOptions;
        public DevOpsController(
            ITestBusinessLogic testBusinessLogic,
            IDisasterRecoveryBusinessLogic disasterRecoveryBusinessLogic,
            ILogger<DevOpsController> logger, 
            IWebService webService, 
            ISharedBusinessLogic sharedBusinessLogic,
            DevOpsOptions devopsOptions) : base(logger, webService, sharedBusinessLogic)
        {
            _testBusinessLogic = testBusinessLogic;
            _disasterRecoveryBusinessLogic = disasterRecoveryBusinessLogic;
            _devopsOptions = devopsOptions;
        }

        #endregion

        #region Home Action

        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            ClearStash();
            ClearAllStashes();
            var viewModel = new HomeViewModel
            {
            };

            return View(viewModel);
        }

        #endregion

        #region Trigger Webjobs Actions
        [HttpGet("trigger-webjobs")]
        public async Task<IActionResult> TriggerWebjobs()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            return View();
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("trigger-webjobs")]
        public async Task<IActionResult> TriggerWebjobs([Text] string webjobname)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!_devopsOptions.AllowTriggerWebjobs)
            {
                ModelState.AddModelError("", $"Action '{nameof(TriggerWebjobs)}:{webjobname}' is not allowed");
                return View();
            }

            if (!string.IsNullOrWhiteSpace(webjobname))
            {
                await _testBusinessLogic.QueueWebjob(webjobname);
                AddDisplayMessage($"Webjob {webjobname} successfully queued", true);
            }

            return RedirectToAction("TriggerWebjobs");
        }

        #endregion

        #region Load testing Actions
        [HttpGet("load-testing")]
        public async Task<IActionResult> LoadTesting()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var viewModel = new LoadTestingViewModel
            {
            };

            return View(viewModel);
        }

        [HttpPost("load-testing")]
        public async Task<IActionResult> LoadTestingAsync(LoadTestingViewModel viewModel, [IgnoreText] string action)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var resetDatabase =false;
            var deleteDownloadFiles = false;
            var deleteDraftFiles = false;
            var deleteAuditLogFiles = false;
            var clearQueues = false;
            var resetSearch = false;
            var clearAppInsights = false;
            var clearCache = false;
            var clearLocalLogs = false;
            var clearLocalSettingsDump = false;

            switch (action)
            {
                case "ResetDatabase":
                    if (!_devopsOptions.AllowDatabaseReset)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    resetDatabase = true;
                    break;
                case "DeleteDownloadFiles":
                    if (!_devopsOptions.AllowDeleteDownloadFiles)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    deleteDownloadFiles = true;
                    break;
                case "DeleteDraftFiles":
                    if (!_devopsOptions.AllowDeleteDraftFiles)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    deleteDraftFiles = true;
                    break;
                case "DeleteAuditLogFiles":
                    if (!_devopsOptions.AllowDeleteAuditLogFiles)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    deleteAuditLogFiles = true;
                    break;
                case "ClearQueues":
                    if (!_devopsOptions.AllowClearQueues)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    clearQueues = true;
                    break;
                case "ResetSearch":
                    if (!_devopsOptions.AllowResetSearch)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    resetSearch = true;
                    break;
                case "ClearAppInsights":
                    if (!_devopsOptions.AllowClearAppInsights)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    clearAppInsights = true;
                    break;
                case "ClearCache":
                    if (!_devopsOptions.AllowClearCache)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    clearCache = true;
                    break;
                case "DeleteLocalLogs":
                    if (!_devopsOptions.AllowDeleteLocalLogs)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    clearLocalLogs = true;
                    break;
                case "DeleteSettingsDump":
                    if (!_devopsOptions.AllowDeleteSettingsDump)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
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

            Task deleteDownloadFilesTask = null;
            if (deleteDownloadFiles)
            {
                deleteDownloadFilesTask = _testBusinessLogic.DeleteDownloadFilesAsync();
                tasks.Add(deleteDownloadFilesTask);
            }

            Task deleteDraftFilesTask = null;
            if (deleteDraftFiles)
            {
                deleteDraftFilesTask = _testBusinessLogic.DeleteDraftFilesAsync();
                tasks.Add(deleteDraftFilesTask);
            }

            Task deleteAuditLogFilesTask = null;
            if (deleteAuditLogFiles)
            {
                deleteAuditLogFilesTask = _testBusinessLogic.DeleteAuditLogFilesAsync();
                tasks.Add(deleteAuditLogFilesTask);
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
                    ModelState.SetMultiException(resetDatabaseTask.Exception, "Reset Database");
                else
                    AddDisplayMessage("Database successfully cleared, reseeded and reset", true);
            }

            if (deleteDownloadFilesTask != null)
            {
                if (deleteDownloadFilesTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(deleteDownloadFilesTask.Exception, "Delete Download Files");
                else
                    AddDisplayMessage("Download files successfully deleted", true);
            }

            if (deleteDraftFilesTask != null)
            {
                if (deleteDraftFilesTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(deleteDraftFilesTask.Exception, "Delete Draft Files");
                else
                    AddDisplayMessage("Draft files successfully deleted", true);
            }

            if (deleteAuditLogFilesTask != null)
            {
                if (deleteAuditLogFilesTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(deleteAuditLogFilesTask.Exception, "Delete Audit Log Files");
                else
                    AddDisplayMessage("Audit Log files successfully deleted", true);
            }

            if (clearQueuesTask != null)
            {
                if (clearQueuesTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(clearQueuesTask.Exception, "Clear Queues");
                else
                    AddDisplayMessage("Queues successfully cleared", true);
            }

            if (resetSearchTask != null)
            {
                if (resetSearchTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(resetSearchTask.Exception, "Reset Search");
                else
                    AddDisplayMessage("Search indexes successfully deleted and recreated", true);
            }

            if (clearAppInsightsTask != null)
            {
                if (clearAppInsightsTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(clearAppInsightsTask.Exception, "Clear AppInsights");
                else
                    AddDisplayMessage("App Insights logs successfully queued for clearing and may take up to 72 hours", true);
            }
            

            if (clearCacheTask != null)
            {
                if (clearCacheTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(clearCacheTask.Exception, "Clear Cache");
                else
                    AddDisplayMessage("Cache and session successfully cleared", true);
            }

            if (clearLocalLogsTask != null)
            {
                if (clearLocalLogsTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(clearLocalLogsTask.Exception, "Delete local logs");
                else
                    AddDisplayMessage("Local log files successfully deleted", true);
            }

            if (clearLocalSettingsDumpTask != null)
            {
                if (clearLocalSettingsDumpTask.Status == TaskStatus.Faulted)
                    ModelState.SetMultiException(clearLocalSettingsDumpTask.Exception, "Delete local settings dump");
                else
                    AddDisplayMessage("Local file containing dump of app settings successfully deleted",true);
            }

            if (!ModelState.IsValid)return View(viewModel);

            return RedirectToAction("LoadTesting");
        }


        #endregion


        #region Environments Actions

        [HttpGet("environments")]
        public async Task<IActionResult> Environments()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Ensure cannot be run on production
            if (SharedBusinessLogic.TestOptions.IsProduction()) return new ForbidResult();

            var viewModel = new EnvironmentsViewModel {
            };

            return View(viewModel);
        }

        #endregion

        #region Disaster Recovery Actions

        [HttpGet("disaster-recovery")]
        public async Task<IActionResult> DisasterRecovery()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var viewModel = UnstashModel<DisasterRecoveryViewModel>();
            if (viewModel == null)
            {
                viewModel = new DisasterRecoveryViewModel();
                try
                {
                    viewModel.SqlServerName = _disasterRecoveryBusinessLogic.GetSqlServerName();
                    viewModel.Databases = await _disasterRecoveryBusinessLogic.ListDatabasesAsync().ToListAsync();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Cannot load SQL databases: {ex.Message}");
                }

                try
                {
                    viewModel.Backups = await _disasterRecoveryBusinessLogic.ListDatabaseBackupsAsync().ToListAsync();
                    viewModel.Backups?.Sort();
                    viewModel.Backups?.Reverse();
                    StashModel(viewModel);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Cannot load backups: {ex.Message}");
                }
            };

            return View(viewModel);
        }

        [HttpPost("disaster-recovery")]
        public async Task<IActionResult> DisasterRecovery([IgnoreText] string action, int databaseIndex = -1, int backupIndex=-1)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var viewModel = UnstashModel<DisasterRecoveryViewModel>();
            if (viewModel == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1138));

            switch (action)
            {
                case "CreateDac":
                    //Check the action is allowed
                    if (!_devopsOptions.AllowDatabaseBackup)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    try
                    {
                        if (databaseIndex < 0) 
                            ModelState.AddModelError("", $"You must select a source database when creating a new backup");
                        else if (viewModel.Backups.Any() && backupIndex > -1)
                            ModelState.AddModelError("", $"You must not select a backup when creating a new backup");
                        else
                        {
                            var database = viewModel.Databases[databaseIndex];
                            var backup = await _disasterRecoveryBusinessLogic.CreateDatabaseDacPacAsync(database);
                            AddDisplayMessage($"Database '{database}' successfully backed up to '{Path.GetFileName(backup)}'",true);
                            databaseIndex = -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        ModelState.AddModelError("", $"Create database backup failed: {ex.Message}");
                    }
                    break;
                case "CreateBac":
                    //Check the action is allowed
                    if (!_devopsOptions.AllowDatabaseBackup)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    try
                    {
                        if (databaseIndex < 0)
                            ModelState.AddModelError("", $"You must select a source database when creating a new backup");
                        else if (viewModel.Backups.Any() && backupIndex > -1)
                            ModelState.AddModelError("", $"You must not select a backup when creating a new backup");
                        else
                        {
                            var database = viewModel.Databases[databaseIndex];
                            var backup = await _disasterRecoveryBusinessLogic.CreateDatabaseBacPacAsync(database);
                            AddDisplayMessage($"Database '{database}' successfully backed up to '{Path.GetFileName(backup)}'", true);
                            databaseIndex = -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        ModelState.AddModelError("", $"Create database backup failed: {ex.Message}");
                    }
                    break;
                case "Restore":
                    //Check the action is allowed
                    if (!_devopsOptions.AllowDatabaseRestore)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    try
                    {
                        if (databaseIndex < 0)
                            ModelState.AddModelError("", $"You must select a target database to restore");
                        else if (backupIndex < 0)
                            ModelState.AddModelError("", $"You must select a backup to restore");
                        else
                        {
                            var database = viewModel.Databases[databaseIndex];
                            var backup = viewModel.Backups[backupIndex];
                            await _disasterRecoveryBusinessLogic.RestoreDatabaseAsync(backup, database);
                            AddDisplayMessage($"Database '{database}' successfully restored from '{Path.GetFileName(backup)}'", true);
                            databaseIndex = -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        ModelState.AddModelError("", $"Restore database backup failed: {ex.Message}");
                    }
                    break;
                case "Delete":
                    //Check the action is allowed
                    if (!_devopsOptions.AllowBackupDelete)
                    {
                        ModelState.AddModelError("", $"Action '{action}' is not allowed");
                        return View(viewModel);
                    }
                    try
                    {
                        if (backupIndex < 0 )
                            ModelState.AddModelError("", $"You must select a backup to delete");
                        else
                        {
                            var backup = viewModel.Backups[backupIndex];
                            await _disasterRecoveryBusinessLogic.DeleteDatabaseBackupAsync(backup);
                            AddDisplayMessage($"Database backup '{Path.GetFileNameWithoutExtension(backup)}' successfully deleted", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, ex.Message);
                        ModelState.AddModelError("", $"Delete database backup failed: {ex.Message}");
                    }
                    break;
                case "None":
                    ModelState.AddModelError("", "You must make a selection");
                    return View(viewModel);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), $"Invalid {nameof(action)}='{action}'");
            }
            viewModel.SelectedDatabaseIndex = databaseIndex;

            if (!ModelState.IsValid) return View(viewModel);

            ClearStash();
            return RedirectToAction(nameof(DisasterRecovery));
        }

        [HttpGet("download-backup/{backupIndex}")]
        public async Task<IActionResult> DownloadBackup(int backupIndex)
        {
            //Check the action is allowed
            if (!_devopsOptions.AllowBackupDownload)return new ForbidResult();

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var viewModel = UnstashModel<DisasterRecoveryViewModel>();
            if (viewModel == null) return new BadRequestResult();

            if (backupIndex < 0 || backupIndex >= viewModel.Backups.Count)
                return new NotFoundResult();

            var backup = viewModel.Backups[backupIndex];
            var stream = await _disasterRecoveryBusinessLogic.GetBackDownloadAsync(backup);

            var fileName = Path.GetFileName(backup);
            
            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition {
                FileName = Path.GetFileName(backup),
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return new FileStreamResult(stream, "application/octet-stream") {
                FileDownloadName = fileName
            };
        }
        #endregion
    }
}
