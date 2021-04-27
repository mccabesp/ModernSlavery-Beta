using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Principal;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.BasicAdmin)]
    [Route("admin")]
    public partial class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IAdminHistory _adminHistory;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;

        #region Constructors

        public AdminController(
            IAdminService adminService,
            ISearchBusinessLogic searchBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic,
            IAdminHistory adminHistory,
            ILogger<AdminController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            _adminHistory = adminHistory;
            _searchBusinessLogic = searchBusinessLogic;
            _statementBusinessLogic = statementBusinessLogic;
        }

        #endregion

        #region Home Action

        [HttpGet]
        public async Task<IActionResult> HomeAsync()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            ClearStash();

            var viewModel = new AdminHomepageViewModel {
                FeedbackCount = SharedBusinessLogic.DataRepository.GetAll<Feedback>().Count(),
                LatestFeedbackDate = SharedBusinessLogic.DataRepository.GetAll<Feedback>()
                    .OrderByDescending(feedback => feedback.CreatedDate)
                    .FirstOrDefault()
                    ?.CreatedDate
            };

            return View("Home", viewModel);
        }

        #endregion

        #region History Action

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var vm = await _adminHistory.GetHistoryLogs();

            return View(vm);
        }

        #endregion

        #region Download Action

        [HttpGet("download")]
        public async Task<IActionResult> Download([IgnoreText] string filePath)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Ensure the file exists
            if (string.IsNullOrWhiteSpace(filePath)) return new HttpNotFoundResult("Missing file path");

            if (filePath.StartsWithI("http:", "https:")) return new RedirectResult(filePath);

            if (!await SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                return new HttpNotFoundResult($"File '{filePath}' does not exist");

            var model = new DownloadViewModel.Download();
            model.Filepath = filePath;

            //Setup the http response
            var contentDisposition = new ContentDisposition { FileName = model.Filename, Inline = true };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            //Write the raw file to include the byte order mark
            var content = await SharedBusinessLogic.FileRepository.ReadBytesAsync(filePath);
            return new FileContentResult(content, model.ContentType);
        }

        #endregion

        #region Read Action

        [HttpGet("read")]
        public async Task<IActionResult> Read([IgnoreText] string filePath)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Ensure the file exists
            if (string.IsNullOrWhiteSpace(filePath) ||
                !await SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath)) return new NotFoundResult();

            var model = new DownloadViewModel.Download();
            model.Filepath = filePath;
            var content = await SharedBusinessLogic.FileRepository.ReadAsync(filePath);

            if (model.ContentType.EqualsI("text/csv"))
                model.Datatable = content.ToDataTable();
            else
                model.Content = content;

            return View("Read", model);
        }

        #endregion

        #region PendingRegistration Action

        [HttpGet("pending-registrations")]
        [IPAddressFilter]
        public async Task<IActionResult> PendingRegistrations()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            UnstashModel<ReviewOrganisationViewModel>(true);

            var nonUkAddressUserOrganisations =
                SharedBusinessLogic.DataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(
                        uo => uo.PINConfirmedDate == null
                              && uo.Method == RegistrationMethods.Manual
                              && uo.Address.IsUkAddress.HasValue
                              && uo.Address.IsUkAddress.Value == false)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            var publicSectorUserOrganisations =
                SharedBusinessLogic.DataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(
                        uo => uo.PINConfirmedDate == null
                              && uo.Method == RegistrationMethods.Manual
                              && uo.Organisation.SectorType == SectorTypes.Public)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            var allManuallyRegisteredUserOrganisations =
                SharedBusinessLogic.DataRepository
                    .GetAll<UserOrganisation>()
                    .Where(uo => uo.User.Status == UserStatuses.Active)
                    .Where(uo => uo.PINConfirmedDate == null && uo.Method == RegistrationMethods.Manual)
                    .OrderBy(uo => uo.Modified)
                    .ToList();

            var remainingManuallyRegisteredUserOrganisations =
                allManuallyRegisteredUserOrganisations
                    .Except(publicSectorUserOrganisations)
                    .Except(nonUkAddressUserOrganisations)
                    .ToList();

            var model = new PendingRegistrationsViewModel {
                PublicSectorUserOrganisations = publicSectorUserOrganisations,
                NonUkAddressUserOrganisations = nonUkAddressUserOrganisations,
                ManuallyRegisteredUserOrganisations = remainingManuallyRegisteredUserOrganisations
            };

            return View("PendingRegistrations", model);
        }

        #endregion

        #region Downloads Action

        [HttpGet("downloads")]
        public async Task<IActionResult> Downloads()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            await SharedBusinessLogic.FileRepository.CreateDirectoryAsync(SharedBusinessLogic.SharedOptions.DownloadsPath);

            var model = new DownloadViewModel();

            #region Organisation Downloads

            DownloadViewModel.Download download;
            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                download = new DownloadViewModel.Download {
                    Type = "Organisations",
                    Filepath = file,
                    Webjob = "UpdateOrganisationsAsync",
                    Title = $"All Organisations ({period})",
                    Description = $"A list of all organisations and their statuses for {period}."
                };
                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            #endregion

            #region Orphaned organisations Addresses

            files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.OrphanOrganisations)}*{Path.GetExtension(Filenames.OrphanOrganisations)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                download = new DownloadViewModel.Download {
                    Type = "Orphan Organisations",
                    Filepath = file,
                    Title = $"Orphan Organisations ({period})",
                    Description =
                        "A list of in-scope organisations who no longer who have any registered or registering users."
                };
                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            #endregion

            #region Registration Addresses

            files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.RegistrationAddresses)}*{Path.GetExtension(Filenames.RegistrationAddresses)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                download = new DownloadViewModel.Download {
                    Type = "Registration Addresses",
                    Filepath = file,
                    Webjob = "UpdateRegistrationAddressesAsync",
                    Title = $"Registered Organisation Addresses ({period})",
                    Description =
                        $"A list of registered organisation addresses and their associated contact details for {period}. This includes the DnB contact details and the most recently registered user contact details."
                };
                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            #endregion

            #region Scopes

            files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationScopes)}*{Path.GetExtension(Filenames.OrganisationScopes)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");

                download = new DownloadViewModel.Download {
                    Type = "Scopes",
                    Filepath = file,
                    Webjob = "UpdateScopesAsync",
                    Title = $"Organisation Scopes ({period})",
                    Description = $"The latest organisation scope statuses for each organisation for ({period})."
                };

                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            #endregion

            #region Submissions

            files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.OrganisationSubmissions)}*{Path.GetExtension(Filenames.OrganisationSubmissions)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");

                download = new DownloadViewModel.Download {
                    Type = "Submissions",
                    Filepath = file,
                    Webjob = "UpdateSubmissionsAsync",
                    Title = $"Organisation Submissions ({period})",
                    Description = $"The reported modern slavery data for all organisations for ({period})."
                };

                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            #endregion

            #region Users

            download = new DownloadViewModel.Download {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Users),
                Webjob = "UpdateUsersAsync",
                Title = "All Users Accounts",
                Description = "A list of all user accounts and their statuses."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            #region Registrations

            download = new DownloadViewModel.Download {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Registrations),
                Webjob = "UpdateRegistrationsAsync",
                Title = "User Organisation Registrations",
                Description =
                    "A list of all organisations that have been registered by a user. This includes all users for each organisation."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            download = new DownloadViewModel.Download {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.UnverifiedRegistrations),
                Webjob = "UpdateUnverifiedRegistrationsAsync",
                Title = "Unverified User Organisation Registrations",
                Description = "A list of all unverified organisations pending verification from a user."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            #region Create Consent downloads

            download = new DownloadViewModel.Download {
                Type = "User Consent",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.SendInfo),
                Webjob = "UpdateUsersToSendInfoAsync",
                Title = "Users to send updates and info",
                Description =
                    "Users who answered \"Yes\" to \"I would like to receive information about webinars, events and new guidance\""
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            download = new DownloadViewModel.Download {
                Type = "User Consent",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.AllowFeedback),
                Webjob = "UpdateUsersToContactForFeedbackAsync",
                Title = "Users to contact for feedback",
                Description =
                    "Users who answered \"Yes\" to \"I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys\""
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            //Sort by modified date then by descending date
            model.Downloads.OrderByDescending(d => d.Filename);

            //Get the modified date and record counts
            var isSuperAdministrator = IsSuperAdministrator;
            await model.Downloads.WaitForAllAsync(
                async d => {
                    if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(d.Filepath))
                    {
                        d.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(d.Filepath);
                        var metaData = await SharedBusinessLogic.FileRepository.LoadMetaDataAsync(d.Filepath);

                        d.Count = metaData != null && metaData.ContainsKey("RecordCount")
                            ? metaData["RecordCount"].ToInt32().ToString()
                            : "(unknown)";

                        //Add the shared keys
                        if (isSuperAdministrator) d.EhrcAccessKey = $"../download?p={d.Filepath}";
                    }
                });

            StashModel(model);

            return View("Downloads", model);
        }

        [HttpPost("downloads")]
        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> Downloads([IgnoreText] string command)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = UnstashModel<DownloadViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1138));
            try
            {
                await UpdateFileAsync(command);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $@"Error: {ex.Message}");
            }

            //Return any errors
            if (!ModelState.IsValid) return View("Downloads", model);

            return View("CustomError", WebService.ErrorViewModelFactory.Create(1139));
        }

        #endregion

        #region Uploads Action

        [HttpGet("uploads")]
        public async Task<IActionResult> Uploads()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = new UploadViewModel();

            #region Show SicSection Upload
            var upload = new UploadViewModel.Upload {
                Type = Filenames.SicSections,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.SicSections),
                Title = "SIC Sections",
                Description = "Standard Industrial Classification (SIC) sector titles. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<SicSection>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            #region Show SicCode Upload
            upload = new UploadViewModel.Upload {
                Type = Filenames.SicCodes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.SicCodes),
                Title = "SIC Codes",
                Description = "Standard Industrial Classification (SIC) codes and titles. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<SicCode>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            #region Show StatementSectorTypes Upload
            upload = new UploadViewModel.Upload {
                Type = Filenames.StatementSectorTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.StatementSectorTypes),
                Title = "Statement Sector Types",
                Description = "Sector types used for Modern Slavery Statements. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<StatementSectorType>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            #region Show ImportPrivateOrganisations Upload
            upload = new UploadViewModel.Upload {
                Type = Filenames.ImportPrivateOrganisations,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.ImportPrivateOrganisations),
                Title = "Private Organisations Import",
                Description = "Add only new Private Organisations from external data source.  Import performs Add only.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.CountAsync<Organisation>(r => r.SectorType == SectorTypes.Private)
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }

            model.Uploads.Add(upload);
            #endregion

            #region Show ImportPublicOrganisations Upload
            upload = new UploadViewModel.Upload {
                Type = Filenames.ImportPublicOrganisations,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.ImportPublicOrganisations),
                Title = "Public Organisations Import",
                Description = "Public Organisations from external data source.  Import performs Add only.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.CountAsync<Organisation>(r => r.SectorType == SectorTypes.Public)
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }

            model.Uploads.Add(upload);
            #endregion

            #region Show ShortCodes Upload
            var allShortCodes = await WebService.ShortCodesRepository.GetAllShortCodesAsync();
            upload = new UploadViewModel.Upload {
                Type = Filenames.ShortCodes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.AppDataPath, Filenames.ShortCodes),
                Title = "Short Codes",
                Description = "Short codes for tracking and routing users to specific web pages. Import performs Add/Update/Delete.",
                DatabaseCount = allShortCodes == null ? 0 : allShortCodes.Count
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            StashModel(model);
            return View("Uploads", model);
        }

        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [HttpPost("uploads")]
        [RequestSizeLimit(52428800)]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> Uploads(List<IFormFile> files, [IgnoreText] string command)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = UnstashModel<UploadViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1138));

            string fileName = command.AfterFirst(":");
            command = command.BeforeFirst(":");

            string fileContent = null;
            if (command.EqualsI("Upload"))
            {
                var file = files.FirstOrDefault();
                if (file == null)
                {
                    ModelState.AddModelError("", "No file uploaded");
                    return View("Uploads", model);
                }

                if (!fileName.EqualsI(file.FileName))
                {
                    ModelState.AddModelError("", $@"Invalid filename '{file.FileName}'");
                    return View("Uploads", model);
                }

                if (file.Length == 0)
                {
                    ModelState.AddModelError("", $@"No content found in '{file.FileName}'");
                    return View("Uploads", model);
                }
                try
                {
                    fileContent = file.OpenReadStream().ReadTextWithEncoding();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $@"Error reading file '{fileName}': {ex.Message}");
                    return View("Uploads", model);
                }
            }

            var upload = model.Uploads.FirstOrDefault(u => u.Filename.EqualsI(fileName));
            if (upload == null)
            {
                ModelState.AddModelError("", $@"Invalid filename '{fileName}'");
                return View("Uploads", model);
            }

            if (command.EqualsI("Import"))
            {
                if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
                    fileContent = await SharedBusinessLogic.FileRepository.ReadAsync(upload.Filepath);
                else
                {
                    ModelState.AddModelError("", $@"Cannot find file '{upload.Filepath}'");
                    return View("Uploads", model);
                }
            }

            //Replace any special spaces with normal spaces
            fileContent = fileContent?.ReplaceSpaceSeparators();
            try
            {
                using (var reader = new StringReader(fileContent))
                {
                    var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                    config.ShouldQuote = (field, context) => true;
                    config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;
                    config.MissingFieldFound = null;
                    config.IgnoreQuotes = false;
                    config.HeaderValidated = null;
                    using var csvReader = new CsvReader(reader, config);

                    List<object> records;
                    switch (fileName)
                    {
                        case var f when f.EqualsI(Filenames.ImportPrivateOrganisations):
                            records = csvReader.GetRecords<ImportOrganisationModel>().Cast<object>().ToList();
                            break;
                        case var f when f.EqualsI(Filenames.ImportPublicOrganisations):
                            records = csvReader.GetRecords<ImportOrganisationModel>().Cast<object>().ToList();
                            break;
                        case var f when f.EqualsI(Filenames.SicSections):
                            records = csvReader.GetRecords<SicSection>().Cast<object>().ToList();
                            break;
                        case var f when f.EqualsI(Filenames.SicCodes):
                            records = csvReader.GetRecords<SicCode>().Cast<object>().ToList();
                            break;
                        case var f when f.EqualsI(Filenames.ShortCodes):
                            records = csvReader.GetRecords<ShortCodeModel>().Cast<object>().ToList();
                            break;
                        case var f when f.EqualsI(Filenames.StatementSectorTypes):
                            records = csvReader.GetRecords<StatementSectorType>().Cast<object>().ToList();
                            break;
                        default:
                            throw new Exception($"Invalid upload '{fileName}'");
                    }

                    if (records.Count < 1)
                    {
                        ModelState.AddModelError("", $@"No records found in '{fileName}'");
                        return View("Uploads", model);
                    }

                    //Core.Classes.Extensions
                    if (command.EqualsI("Upload"))
                    {
                        await SharedBusinessLogic.FileRepository.WriteAsync(upload.Filepath, fileContent);
                        var message = $"{records.Count()} records uploaded from {fileName}";
                        AddDisplayMessage(message);
                        Logger.LogInformation(message);
                    }

                    else if (command.EqualsI("Import"))
                    {
                        var importCount = 0;
                        switch (fileName)
                        {
                            case var f when f.EqualsI(Filenames.ImportPrivateOrganisations):
                                importCount = await _adminService.DataImporter.ImportPrivateOrganisationsAsync(VirtualUser.UserId, 0, true, false);
                                break;
                            case var f when f.EqualsI(Filenames.ImportPublicOrganisations):
                                importCount = await _adminService.DataImporter.ImportPublicOrganisationsAsync(VirtualUser.UserId, 0, true, false);
                                break;
                            case var f when f.EqualsI(Filenames.SicSections):
                                importCount = await _adminService.DataImporter.ImportSICSectionsAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.SicCodes):
                                importCount = await _adminService.DataImporter.ImportSICCodesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.ShortCodes):
                                await WebService.ShortCodesRepository.ClearAllShortCodesAsync();
                                importCount = records.Count();
                                break;
                            case var f when f.EqualsI(Filenames.StatementSectorTypes):
                                importCount = await _adminService.DataImporter.ImportStatementSectorTypesAsync(true);
                                break;
                        }
                        var message = $"Imported {importCount} of {records.Count()} records from {fileName}";
                        AddDisplayMessage(message);
                        Logger.LogInformation(message);

                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.SetMultiException(ex, $"Error reading file '{fileName}'");
            }


            //Return any errors
            if (!ModelState.IsValid) return View("Uploads", model);

            return RedirectToAction("Uploads");
        }

        #endregion

        #region Action Impersonate

        [HttpGet("impersonate")]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> Impersonate([EmailAddress] string emailAddress)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!string.IsNullOrWhiteSpace(emailAddress)) return await ImpersonatePost(emailAddress);

            return View("Impersonate");
        }

        [HttpPost("impersonate")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoleNames.SuperOrDatabaseAdmins)]
        public async Task<IActionResult> ImpersonatePost([EmailAddress] string emailAddress)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Ignore case of email address
            emailAddress = emailAddress?.ToLower();

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(emailAddress) || !emailAddress.IsEmailAddress())
            {
                ModelState.AddModelError("", "You must enter a valid email address");
                return View("Impersonate");
            }

            //Ensure we get a valid user from the database
            var currentUser = SharedBusinessLogic.DataRepository.FindUser(User);
            if (currentUser == null || !_adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(currentUser)) throw new IdentityNotMappedException();

            // find the latest active user by email
            var impersonatedUser = await _adminService.UserRepository.FindByEmailAsync(emailAddress, UserStatuses.Active);

            if (impersonatedUser == null)
            {
                ModelState.AddModelError("", "This user does not exist");
                return View("Impersonate");
            }

            if (_adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(impersonatedUser))
            {
                ModelState.AddModelError("", "Impersonating other administrators is not permitted");
                return View("Impersonate");
            }

            ImpersonatedUserId = impersonatedUser.UserId;
            OriginalUser = currentUser;

            //Refresh page to ensure identity is passed in cookie
            return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");
        }

        #endregion
    }
}