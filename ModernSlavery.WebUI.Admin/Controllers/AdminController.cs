using System;
using System.Collections.Generic;
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
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public partial class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        #region Constructors

        public AdminController(
            IAdminService adminService,
            ILogger<AdminController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
        }

        #endregion


        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            if (!SharedBusinessLogic.SharedOptions.IsProduction())
                Logger.LogInformation("Admin Controller Initialised");

            return new EmptyResult();
        }

        #endregion

        #region Home Action

        [HttpGet]
        public IActionResult Home()
        {
            var viewModel = new AdminHomepageViewModel
            {
                IsSuperAdministrator = _adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(CurrentUser),
                IsDatabaseAdministrator = _adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(CurrentUser),
                IsDowngradedDueToIpRestrictions =
                    !IsTrustedIP && (_adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(CurrentUser) || _adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(CurrentUser)),
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
            var model = new DownloadViewModel();
            var downloads = new List<DownloadViewModel.Download>();
            DownloadViewModel.Download download;

            #region Create Registration History

            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.LogPath, "RegistrationLog*.csv", true);
            if (!files.Any())
            {
                //Create the first log file
                var logRecords = new List<RegisterLogModel>();
                foreach (var userOrg in SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                    .Where(uo => uo.PINConfirmedDate != null)
                    .OrderBy(uo => uo.PINConfirmedDate))
                {
                    //Dont log test registrations
                    if (userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix)) continue;

                    var status = await SharedBusinessLogic.DataRepository.GetAll<OrganisationStatus>()
                        .FirstOrDefaultAsync(
                            os => os.OrganisationId == userOrg.OrganisationId
                                  && os.Status == userOrg.Organisation.Status
                                  && os.StatusDate == userOrg.Organisation.StatusDate);
                    if (status == null)
                        Logger.LogError(
                            $"Could not find status '{userOrg.Organisation.Status}' for organisation '{userOrg.OrganisationId}' at '{userOrg.Organisation.StatusDate}' while creating registration history");
                    else
                        logRecords.Add(
                            new RegisterLogModel
                            {
                                StatusDate = status.StatusDate,
                                Status = status.StatusDetails,
                                ActionBy = status.ByUser.EmailAddress,
                                Details = "",
                                Sector = userOrg.Organisation.SectorType,
                                Organisation = userOrg.Organisation.OrganisationName,
                                CompanyNo = userOrg.Organisation.CompanyNumber,
                                Address = userOrg?.Address.GetAddressString(),
                                SicCodes = userOrg.Organisation.GetLatestSicCodeIdsString(),
                                UserFirstname = userOrg.User.Firstname,
                                UserLastname = userOrg.User.Lastname,
                                UserJobtitle = userOrg.User.JobTitle,
                                UserEmail = userOrg.User.EmailAddress,
                                ContactFirstName = userOrg.User.ContactFirstName,
                                ContactLastName = userOrg.User.ContactLastName,
                                ContactJobTitle = userOrg.User.ContactJobTitle,
                                ContactOrganisation = userOrg.User.ContactOrganisation,
                                ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                            });
                }

                if (logRecords.Count > 0)
                    await _adminService.RegistrationLog.WriteAsync(logRecords.OrderBy(l => l.StatusDate));

                //Get the files again
                files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                    SharedBusinessLogic.SharedOptions.LogPath, "RegistrationLog*.csv", true);
            }

            foreach (var filePath in files)
            {
                download = new DownloadViewModel.Download
                {
                    Type = "Registration History",
                    Filepath = filePath,
                    Title = "Registration History",
                    Description = "Audit history of approved and rejected registrations."
                };
                if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(download.Filepath))
                    download.Modified =
                        await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(download.Filepath);

                downloads.Add(download);
            }

            model.Downloads.OrderByDescending(d => d.Modified).ThenByDescending(d => d.Filename);
            model.Downloads.AddRange(downloads);

            #endregion

            #region Create Submission History

            downloads = new List<DownloadViewModel.Download>();

            files = await SharedBusinessLogic.FileRepository.GetFilesAsync(SharedBusinessLogic.SharedOptions.LogPath,
                "SubmissionLog*.csv", true);
            if (!files.Any())
            {
                var logRecords = new List<SubmissionLogModel>();

                //Create the first log file
                foreach (var statement in SharedBusinessLogic.DataRepository.GetAll<Statement>().OrderBy(r => r.StatusDate))
                {
                    //Dont log return for test organisations
                    if (statement.Organisation.OrganisationName.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                        continue;

                    var status = await SharedBusinessLogic.DataRepository.GetAll<StatementStatus>()
                        .FirstOrDefaultAsync(
                            rs => rs.StatementId == statement.StatementId && rs.Status == statement.Status &&
                                  rs.StatusDate == statement.StatusDate);

                    //Log the submission
                    if (status == null)
                        Logger.LogError(
                            $"Could not find status '{statement.Status}' for return '{statement.StatementId}' at '{statement.StatusDate}' while creating return history");
                    else
                        logRecords.Add(
                            new SubmissionLogModel
                            {
                                StatusDate = statement.Created,
                                Status = StatementStatuses.Submitted,
                                Details = "",
                                Sector = statement.Organisation.SectorType,
                                StatementId = statement.StatementId,
                                AccountingDate = statement.SubmissionDeadline.ToShortDateString(),
                                OrganisationId = statement.OrganisationId,
                                EmployerName = statement.Organisation.OrganisationName,
                                Address = statement.Organisation.LatestAddress?.GetAddressString(
                                    "," + Environment.NewLine),
                                CompanyNumber = statement.Organisation.CompanyNumber,
                                SicCodes = statement.Organisation.GetSicCodeIdsString(statement.StatusDate,
                                    "," + Environment.NewLine),
                                StatementUrl = statement.StatementUrl,
                                ApprovingPerson = statement.ApprovingPerson,
                                UserFirstname = status.ByUser.Firstname,
                                UserLastname = status.ByUser.Lastname,
                                UserJobtitle = status.ByUser.JobTitle,
                                UserEmail = status.ByUser.EmailAddress,
                                ContactFirstName = status.ByUser.ContactFirstName,
                                ContactLastName = status.ByUser.ContactLastName,
                                ContactJobTitle = status.ByUser.ContactJobTitle,
                                ContactOrganisation = status.ByUser.ContactOrganisation,
                                ContactPhoneNumber = status.ByUser.ContactPhoneNumber
                            });
                }

                if (logRecords.Count > 0) await _adminService.LogSubmission(logRecords.OrderBy(l => l.StatusDate));

                //Get the files again
                files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                    SharedBusinessLogic.SharedOptions.LogPath, "SubmissionLog*.csv", true);
            }

            foreach (var filePath in files)
            {
                download = new DownloadViewModel.Download
                {
                    Type = "Submission History",
                    Filepath = filePath,
                    Title = "Submission History",
                    Description = "Audit history of approved and rejected registrations."
                };
                if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(download.Filepath))
                    download.Modified =
                        await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(download.Filepath);

                downloads.Add(download);
            }

            model.Downloads.OrderByDescending(d => d.Modified).ThenByDescending(d => d.Filename);
            model.Downloads.AddRange(downloads);

            #endregion

            return View("History", model);
        }

        #endregion

        #region Download Action

        [HttpGet("download")]
        public async Task<IActionResult> Download(string filePath)
        {
            //Ensure the file exists
            if (string.IsNullOrWhiteSpace(filePath)) return new HttpNotFoundResult("Missing file path");

            if (filePath.StartsWithI("http:", "https:")) return new RedirectResult(filePath);

            if (!await SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                return new HttpNotFoundResult($"File '{filePath}' does not exist");

            var model = new DownloadViewModel.Download();
            model.Filepath = filePath;

            //Setup the http response
            var contentDisposition = new ContentDisposition {FileName = model.Filename, Inline = true};
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            /* No Longer required as AspNetCore has response buffering on by default
            // Buffer response so that page is sent after processing is complete.
            Response.BufferOutput = true;
            */

            return Content(await SharedBusinessLogic.FileRepository.ReadAsync(filePath), model.ContentType);
        }

        #endregion

        #region Read Action

        [HttpGet("read")]
        public async Task<IActionResult> Read(string filePath)
        {
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
        public async Task<IActionResult> PendingRegistrations()
        {
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

            var model = new PendingRegistrationsViewModel
            {
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
            await SharedBusinessLogic.FileRepository.CreateDirectoryAsync(SharedBusinessLogic.SharedOptions
                .DownloadsPath);

            var model = new DownloadViewModel();

            #region Organisation Downloads

            DownloadViewModel.Download download;
            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsPath,
                $"{Path.GetFileNameWithoutExtension(Filenames.Organisations)}*{Path.GetExtension(Filenames.Organisations)}");
            foreach (var file in files.OrderByDescending(file => file))
            {
                var period = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                download = new DownloadViewModel.Download
                {
                    Type = "Organisations",
                    Filepath = file,
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
                download = new DownloadViewModel.Download
                {
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
                download = new DownloadViewModel.Download
                {
                    Type = "Registration Addresses",
                    Filepath = file,
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

                download = new DownloadViewModel.Download
                {
                    Type = "Scopes",
                    Filepath = file,
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

                download = new DownloadViewModel.Download
                {
                    Type = "Submissions",
                    Filepath = file,
                    Title = $"Organisation Submissions ({period})",
                    Description = $"The reported GPG data for all organisations for ({period})."
                };

                download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
                model.Downloads.Add(download);
            }

            download = new DownloadViewModel.Download
            {
                Type = "Late Submissions",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.OrganisationLateSubmissions),
                Title = "Organisation Late Submissions",
                Description = "Organisations who reported or changed their submission late the previous snapshot date."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            #region Users

            download = new DownloadViewModel.Download
            {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Users),
                Title = "All Users Accounts",
                Description = "A list of all user accounts and their statuses."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            #region Registrations

            download = new DownloadViewModel.Download
            {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Registrations),
                Title = "User Organisation Registrations",
                Description =
                    "A list of all organisations that have been registered by a user. This includes all users for each organisation."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            download = new DownloadViewModel.Download
            {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.UnverifiedRegistrations),
                Title = "Unverified User Organisation Registrations",
                Description = "A list of all unverified organisations pending verification from a user."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            download = new DownloadViewModel.Download
            {
                Type = "Users",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.UnverifiedRegistrations),
                Title = "Unverified User Organisation Registrations",
                Description = "A list of all unverified organisations pending verification from a user."
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            #endregion

            #region Create Consent downloads

            download = new DownloadViewModel.Download
            {
                Type = "User Consent",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.SendInfo),
                Title = "Users to send updates and info",
                Description =
                    "Users who answered \"Yes\" to \"I would like to receive information about webinars, events and new guidance\""
            };
            download.ShowUpdateButton = !await GetFileUpdatingAsync(download.Filepath);
            model.Downloads.Add(download);

            download = new DownloadViewModel.Download
            {
                Type = "User Consent",
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.AllowFeedback),
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
                async d =>
                {
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
        public async Task<IActionResult> Downloads(string command)
        {
            //Throw error if the user is not a super administrator
            if (!IsSuperAdministrator)
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a super administrator");

            var model = UnstashModel<DownloadViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1138));

            var filepath = command.AfterFirst(":");
            command = command.BeforeFirst(":");
            try
            {
                await UpdateFileAsync(filepath, command);
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
            var model = new UploadViewModel();

            #region Show SicSection Upload
            var upload = new UploadViewModel.Upload
            {
                Type = Filenames.SicSections,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.SicSections),
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
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.SicCodes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.SicCodes),
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

            #region Show StatementDiligenceTypes Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.StatementDiligenceTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.StatementDiligenceTypes),
                Title = "Statement Due Diligence Types",
                Description = "Due-diligence types used for Modern Slavery Statements. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<StatementDiligenceType>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            
            #endregion

            #region Show StatementPolicyTypes Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.StatementPolicyTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.StatementPolicyTypes),
                Title = "Statement Policy Types",
                Description = "Policy types used for Modern Slavery Statements. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<StatementPolicyType>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            #region Show StatementRiskTypes Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.StatementRiskTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.StatementRiskTypes),
                Title = "Statement Risk Types",
                Description = "Risk types used for Modern Slavery Statements. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<StatementRiskType>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            model.Uploads.Add(upload);
            #endregion

            #region Show StatementSectorTypes Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.StatementSectorTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.StatementSectorTypes),
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

            #region Show StatementTrainingTypes Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.StatementTrainingTypes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.StatementTrainingTypes),
                Title = "Statement Due Training Types",
                Description = "Due-Training types used for Modern Slavery Statements. Import performs Add/Update/Delete.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.GetAll<StatementTrainingType>().CountAsync()
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }
            
            model.Uploads.Add(upload);
            #endregion

            #region Show ImportPrivateOrganisations Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.ImportPrivateOrganisations,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.ImportPrivateOrganisations),
                Title = "Private Organisations Import",
                Description = "Add only new Private Organisations from external data source.  Import performs Add only.",
                DatabaseCount = await SharedBusinessLogic.DataRepository.CountAsync<Organisation>(r=>r.SectorType== SectorTypes.Private)
            };
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(upload.Filepath))
            {
                upload.Modified = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(upload.Filepath);
                upload.FileExists = true;
            }

            model.Uploads.Add(upload);
            #endregion

            #region Show ImportPublicOrganisations Upload
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.ImportPublicOrganisations,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.ImportPublicOrganisations),
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
            upload = new UploadViewModel.Upload
            {
                Type = Filenames.ShortCodes,
                Filepath = Path.Combine(SharedBusinessLogic.SharedOptions.DataPath, Filenames.ShortCodes),
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
        public async Task<IActionResult> Uploads(List<IFormFile> files, string command)
        {
            string fileName = command.AfterFirst(":");
            command = command.BeforeFirst(":");

            //Throw error if the user is not a super administrator
            if (!IsSuperAdministrator)
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a super administrator");

            var model = UnstashModel<UploadViewModel>();
            if (model == null) return View("CustomError", WebService.ErrorViewModelFactory.Create(1138));

            var filepath = command.AfterFirst(":");
            command = command.BeforeFirst(":");

            if (command.EqualsI("Send", "Pause", "Update"))
            {
                try
                {
                    await UpdateFileAsync(filepath, command);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $@"Error: {ex.Message}");
                }
            }
            else if (command.EqualsI("Recheck"))
            {
                await RecheckCompaniesAsync();
            }

            else if (command.EqualsI("Upload","Import"))
            {
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
                    using (var reader = new StreamReader(file.OpenReadStream()))
                        fileContent =await reader.ReadToEndAsync();
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

                try
                {
                    using (var reader = new StringReader(fileContent))
                    {
                        var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                        config.ShouldQuote = (field, context) => true;
                        config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;
                        config.MissingFieldFound = null;
                        config.IgnoreQuotes = false;
                        config.HeaderValidated =  null;
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
                            case var f when f.EqualsI(Filenames.StatementDiligenceTypes):
                                records = csvReader.GetRecords<StatementDiligenceType>().Cast<object>().ToList();
                                break;
                            case var f when f.EqualsI(Filenames.StatementPolicyTypes):
                                records = csvReader.GetRecords<StatementPolicyType>().Cast<object>().ToList();
                                break;
                            case var f when f.EqualsI(Filenames.StatementRiskTypes):
                                records = csvReader.GetRecords<StatementRiskType>().Cast<object>().ToList();
                                break;
                            case var f when f.EqualsI(Filenames.StatementSectorTypes):
                                records = csvReader.GetRecords<StatementSectorType>().Cast<object>().ToList();
                                break;
                            case var f when f.EqualsI(Filenames.StatementTrainingTypes):
                                records = csvReader.GetRecords<StatementTrainingType>().Cast<object>().ToList();
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
                        if (command.EqualsI("Upload"))await SharedBusinessLogic.FileRepository.SaveCSVAsync(records, fileName);

                        var updateTime = VirtualDateTime.Now.AddMinutes(-2);
                        switch (fileName)
                        {
                            case var f when f.EqualsI(Filenames.ImportPrivateOrganisations):
                                await _adminService.DataImporter.ImportPrivateOrganisationsAsync(VirtualUser.UserId,true);
                                break;
                            case var f when f.EqualsI(Filenames.ImportPublicOrganisations):
                                await _adminService.DataImporter.ImportPublicOrganisationsAsync(VirtualUser.UserId,true);
                                break;
                            case var f when f.EqualsI(Filenames.SicSections):
                                await _adminService.DataImporter.ImportSICSectionsAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.SicCodes):
                                await _adminService.DataImporter.ImportSICCodesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.ShortCodes):
                                await WebService.ShortCodesRepository.ClearAllShortCodesAsync();
                                break;
                            case var f when f.EqualsI(Filenames.StatementDiligenceTypes):
                                await _adminService.DataImporter.ImportStatementDiligenceTypesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.StatementPolicyTypes):
                                await _adminService.DataImporter.ImportStatementPolicyTypesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.StatementRiskTypes):
                                await _adminService.DataImporter.ImportStatementRiskTypesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.StatementSectorTypes):
                                await _adminService.DataImporter.ImportStatementSectorTypesAsync(true);
                                break;
                            case var f when f.EqualsI(Filenames.StatementTrainingTypes):
                                await _adminService.DataImporter.ImportStatementTrainingTypesAsync(true);
                                break;
                        }
                    }
                }
                catch (AggregateException aex)
                {
                    ModelState.AddModelError("", $@"Error reading file '{fileName}'");
                    foreach (var ex in aex.InnerExceptions)
                        ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $@"Error reading file '{fileName}': {ex.Message}");
                }
            }

            //Return any errors
            if (!ModelState.IsValid) return View("Uploads", model);

            return RedirectToAction("Uploads");
        }

        private async Task UpdateCompanySicCodesAsync(DateTime updateTime)
        {
            //Get all the bad sic records
            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.LogPath, "BadSicLog*.csv", true);
            var fileRecords = new Dictionary<string, List<BadSicLogModel>>();
            var changedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                var records = await SharedBusinessLogic.FileRepository.ReadCSVAsync<BadSicLogModel>(file);
                fileRecords[file] = records;
            }

            //Get all the new Sic Codes
            var newSicCodes = SharedBusinessLogic.DataRepository.GetAll<SicCode>().Where(s => s.Created >= updateTime)
                .Select(s => s.SicCodeId);

            foreach (var newSicCode in newSicCodes)
            foreach (var key in fileRecords.Keys)
            {
                var file = fileRecords[key];
                var records = file.Where(r => r.SicCode == newSicCode);
                foreach (var record in records)
                {
                    var orgSics = SharedBusinessLogic.DataRepository.GetAll<OrganisationSicCode>()
                        .Where(o => o.OrganisationId == record.OrganisationId);
                    if (await orgSics.AnyAsync())
                    {
                        if (await orgSics.AnyAsync(o => o.SicCodeId == newSicCode)) continue;

                        SharedBusinessLogic.DataRepository.Insert(new OrganisationSicCode
                            {OrganisationId = record.OrganisationId, SicCodeId = newSicCode});
                    }

                    file.Remove(record);
                    changedFiles.Add(key);
                }
            }

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Update or delete the changed log files
            foreach (var changedFile in changedFiles)
            {
                var records = fileRecords[changedFile];
                if (records.Any())
                    await SharedBusinessLogic.FileRepository.SaveCSVAsync(fileRecords[changedFile], changedFile);
                else
                    await SharedBusinessLogic.FileRepository.DeleteFileAsync(changedFile);
            }
        }

        private async Task RecheckCompaniesAsync()
        {
            //Get all the bad sic records
            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.LogPath, "BadSicLog*.csv", true);
            var badSicCodes = new HashSet<string>();
            foreach (var file in files)
            {
                var records = await SharedBusinessLogic.FileRepository.ReadCSVAsync<BadSicLogModel>(file);
                badSicCodes.AddRange(records.ToList().Select(s => $"{s.OrganisationId}:{s.SicCode}").Distinct());
            }

            //Get all the private organisations with no sic codes
            var orgs = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(
                    o => o.SectorType == SectorTypes.Private
                         && o.CompanyNumber != null
                         && !o.OrganisationSicCodes.Where(s => s.Retired == null).Any());
            var allSicCodes = await SharedBusinessLogic.DataRepository.GetAll<SicCode>().ToListAsync();
            foreach (var org in orgs)
                try
                {
                    //Lookup the sic codes from companies house
                    var sicCodeResults = await _adminService.PrivateSectorRepository.GetSicCodesAsync(org.CompanyNumber);
                    var sicCodes = sicCodeResults.SplitI().Select(s => s.ToInt32());
                    foreach (var code in sicCodes)
                    {
                        if (code <= 0) continue;

                        var sicCode = allSicCodes.FirstOrDefault(sic => sic.SicCodeId == code);
                        if (sicCode != null)
                        {
                            org.OrganisationSicCodes.Add(
                                new OrganisationSicCode {Organisation = org, SicCode = sicCode});
                            continue;
                        }

                        if (badSicCodes.Contains($"{org.OrganisationId}:{code}")) continue;

                        await _adminService.BadSicLog.WriteAsync(
                            new BadSicLogModel
                            {
                                OrganisationId = org.OrganisationId, OrganisationName = org.OrganisationName,
                                SicCode = code
                            });
                    }
                }
                catch (HttpException hex)
                {
                    var httpCode = hex.StatusCode;
                    if (httpCode.IsAny(429, (int) HttpStatusCode.NotFound)) Logger.LogError(hex, hex.Message);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
        }

        #endregion

        #region Action Impersonate

        [HttpGet("impersonate")]
        public async Task<IActionResult> Impersonate(string emailAddress)
        {
            if (!string.IsNullOrWhiteSpace(emailAddress)) return await ImpersonatePost(emailAddress);

            return View("Impersonate");
        }

        [HttpPost("impersonate")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImpersonatePost(string emailAddress)
        {
            //Ignore case of email address
            emailAddress = emailAddress?.ToLower();

            //Throw error if the user is not a super administrator of a test admin
            if (!IsSuperAdministrator && (!IsAdministrator || !IsTestUser ||
                                          !emailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix)))
                return new HttpUnauthorizedResult($"User {CurrentUser?.EmailAddress} is not a super administrator");

            if (string.IsNullOrWhiteSpace(emailAddress) || !emailAddress.IsEmailAddress())
            {
                ModelState.AddModelError("", "You must enter a valid email address");
                return View("Impersonate");
            }

            //Ensure we get a valid user from the database
            var currentUser = SharedBusinessLogic.DataRepository.FindUser(User);
            if (currentUser == null || !_adminService.SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(currentUser)) throw new IdentityNotMappedException();

            if (currentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) &&
                !emailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                ModelState.AddModelError(
                    "",
                    "Test administrators are only permitted to impersonate other test users");
                return View("Impersonate");
            }

            // find the latest active user by email
            var impersonatedUser =
                await _adminService.UserRepository.FindByEmailAsync(emailAddress, UserStatuses.Active);
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