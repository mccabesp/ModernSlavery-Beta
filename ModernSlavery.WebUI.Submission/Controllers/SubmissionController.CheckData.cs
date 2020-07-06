using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {
        #region private methods

        private async Task TryToReloadDraftContent(ReturnViewModel stashedReturnViewModel)
        {
            var availableDraft = await _SubmissionPresenter.GetDraftIfAvailableAsync(
                stashedReturnViewModel.OrganisationId,
                stashedReturnViewModel.AccountingDate.Year);

            if (availableDraft != null && availableDraft.HasContent())
                stashedReturnViewModel.ReportInfo.Draft.ReturnViewModelContent = availableDraft.ReturnViewModelContent;
        }

        private string GetReportLink(Return postedReturn)
        {
            return Url.Action(
                "Report",
                "Viewing",
                new
                {
                    employerIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(postedReturn.Organisation.OrganisationId),
                    year = postedReturn.AccountingDate.Year
                },
                "https");
        }

        private string GetSubmittedOrUpdated(Return postedReturn)
        {
            var otherReturns =
                postedReturn.Organisation.Returns
                    .Except(new[] {postedReturn})
                    .Where(r => r.AccountingDate == postedReturn.AccountingDate)
                    .ToList();

            return otherReturns.Count > 0 ? "updated" : "submitted";
        }

        #endregion

        #region public methods

        [HttpGet("check-data")]
        public async Task<IActionResult> CheckData()
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            stashedReturnViewModel =
                await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (stashedReturnViewModel.ReportInfo.Draft != null &&
                !stashedReturnViewModel.ReportInfo.Draft.IsUserAllowedAccess)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CustomError", WebService.ErrorViewModelFactory.Create(3040));
            }


            if (!stashedReturnViewModel.HasDraftWithContent()) await TryToReloadDraftContent(stashedReturnViewModel);

            if (stashedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession
                || stashedReturnViewModel.HasDraftWithContent())
            {
                var databaseReturn = await _SubmissionPresenter.GetSubmissionByIdAsync(stashedReturnViewModel.ReturnId);

                if (databaseReturn != null)
                {
                    var stashedReturn = _SubmissionPresenter.CreateDraftSubmissionFromViewModel(stashedReturnViewModel);
                    var changeSummary = _SubmissionPresenter.GetSubmissionChangeSummary(stashedReturn, databaseReturn);
                    stashedReturnViewModel.IsDifferentFromDatabase = changeSummary.HasChanged;
                    stashedReturnViewModel.ShouldProvideLateReason = changeSummary.ShouldProvideLateReason;
                }
                else
                {
                    // We have some draft info and no DB record, therefore is definitely different
                    stashedReturnViewModel.IsDifferentFromDatabase = true;
                    // Recalculate to know if they're submitting late. This is because it is possible that a draft was created BEFORE the cut-off date ("should provide late reason" would have been marked as 'false') but are completing the submission process AFTER which is when we need them to provide a late reason and the flag is expected to be 'true'.
                    stashedReturnViewModel.ShouldProvideLateReason = _SubmissionPresenter.IsHistoricSnapshotYear(
                        stashedReturnViewModel.SectorType,
                        ReportingOrganisationStartYear.Value);
                }
            }

            if (!_SubmissionPresenter.IsValidSnapshotYear(ReportingOrganisationStartYear.Value))
                return new HttpBadRequestResult($"Invalid snapshot year {ReportingOrganisationStartYear.Value}");

            StashModel(stashedReturnViewModel);
            return View("CheckData", stashedReturnViewModel);
        }

        [HttpPost("check-data")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckData(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            postedReturnViewModel.ReportInfo.Draft = stashedReturnViewModel.ReportInfo.Draft;

            if (postedReturnViewModel.SectorType == SectorTypes.Public)
                ModelState.Exclude(
                    nameof(postedReturnViewModel.FirstName),
                    nameof(postedReturnViewModel.LastName),
                    nameof(postedReturnViewModel.JobTitle));

            var postedReturn = _SubmissionPresenter.CreateDraftSubmissionFromViewModel(postedReturnViewModel);

            SubmissionChangeSummary changeSummary = null;
            var databaseReturn = await _SubmissionPresenter.GetSubmissionByIdAsync(postedReturnViewModel.ReturnId);
            if (databaseReturn != null)
            {
                changeSummary = _SubmissionPresenter.GetSubmissionChangeSummary(postedReturn, databaseReturn);

                if (!changeSummary.HasChanged) return new HttpBadRequestResult("Submission has no changes");

                if (!changeSummary.FiguresChanged)
                    // If the figure have not changed
                    //   e.g. if the only change was to the URL / person reporting
                    // Then don't apply a NEW late flag
                    // But DO continue to apply an EXISTING late flag
                    // So, in summary, "If the figure have not changed, copy the old late flag"
                    postedReturn.IsLateSubmission = databaseReturn.IsLateSubmission;

                postedReturn.Modifications = changeSummary.Modifications;

                databaseReturn.SetStatus(ReturnStatuses.Retired,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId);
            }

            if (databaseReturn == null || !changeSummary.ShouldProvideLateReason)
            {
                ModelState.Remove(nameof(postedReturnViewModel.LateReason));
                ModelState.Remove(nameof(postedReturnViewModel.EHRCResponse));
            }

            ModelState.Remove("ReportInfo.Draft");

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("CheckData", postedReturnViewModel);
            }

            if (databaseReturn == null || databaseReturn.Status == ReturnStatuses.Retired)
                SharedBusinessLogic.DataRepository.Insert(postedReturn);

            postedReturn.SetStatus(ReturnStatuses.Submitted, OriginalUser?.UserId ?? VirtualUser.UserId);

            var organisationFromDatabase =
                await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                    o.OrganisationId == postedReturnViewModel.OrganisationId);

            organisationFromDatabase.Returns.Add(postedReturn);

            if (_SubmissionPresenter.ShouldUpdateLatestReturn(organisationFromDatabase,
                ReportingOrganisationStartYear.Value)) organisationFromDatabase.LatestReturn = postedReturn;

            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            if (!VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                await _SubmissionPresenter.SubmissionService.SubmissionBusinessLogic.SubmissionLog.WriteAsync(
                    new SubmissionLogModel
                    {
                        StatusDate = VirtualDateTime.Now,
                        Status = postedReturn.Status,
                        Details = "",
                        Sector = postedReturn.Organisation.SectorType,
                        ReturnId = postedReturn.ReturnId,
                        AccountingDate = postedReturn.AccountingDate.ToShortDateString(),
                        OrganisationId = postedReturn.OrganisationId,
                        EmployerName = postedReturn.Organisation.OrganisationName,
                        Address = postedReturn.Organisation.LatestAddress?.GetAddressString("," + Environment.NewLine),
                        CompanyNumber = postedReturn.Organisation.CompanyNumber,
                        SicCodes = postedReturn.Organisation.GetSicCodeIdsString(postedReturn.StatusDate,
                            "," + Environment.NewLine),
                        DiffMeanHourlyPayPercent = postedReturn.DiffMeanHourlyPayPercent,
                        DiffMedianHourlyPercent = postedReturn.DiffMedianHourlyPercent,
                        DiffMeanBonusPercent = postedReturn.DiffMeanBonusPercent,
                        DiffMedianBonusPercent = postedReturn.DiffMedianBonusPercent,
                        MaleMedianBonusPayPercent = postedReturn.MaleMedianBonusPayPercent,
                        FemaleMedianBonusPayPercent = postedReturn.FemaleMedianBonusPayPercent,
                        MaleLowerPayBand = postedReturn.MaleLowerPayBand,
                        FemaleLowerPayBand = postedReturn.FemaleLowerPayBand,
                        MaleMiddlePayBand = postedReturn.MaleMiddlePayBand,
                        FemaleMiddlePayBand = postedReturn.FemaleMiddlePayBand,
                        MaleUpperPayBand = postedReturn.MaleUpperPayBand,
                        FemaleUpperPayBand = postedReturn.FemaleUpperPayBand,
                        MaleUpperQuartilePayBand = postedReturn.MaleUpperQuartilePayBand,
                        FemaleUpperQuartilePayBand = postedReturn.FemaleUpperQuartilePayBand,
                        CompanyLinkToGPGInfo = postedReturn.CompanyLinkToGPGInfo,
                        ResponsiblePerson = postedReturn.ResponsiblePerson,
                        UserFirstname = VirtualUser.Firstname,
                        UserLastname = VirtualUser.Lastname,
                        UserJobtitle = VirtualUser.JobTitle,
                        UserEmail = VirtualUser.EmailAddress,
                        ContactFirstName = VirtualUser.ContactFirstName,
                        ContactLastName = VirtualUser.ContactLastName,
                        ContactJobTitle = VirtualUser.ContactJobTitle,
                        ContactOrganisation = VirtualUser.ContactOrganisation,
                        ContactPhoneNumber = VirtualUser.ContactPhoneNumber,
                        Created = postedReturn.Created,
                        Modified = postedReturn.Modified,
                        Browser = HttpContext.GetBrowser() ?? "No browser in the request",
                        SessionId = Session.SessionID
                    });

            //This is required for the submission complete page
            postedReturnViewModel.EncryptedOrganisationId = SharedBusinessLogic.Obfuscator.Obfuscate(postedReturn.Organisation.OrganisationId);
            StashModel(postedReturnViewModel);

            if (SharedBusinessLogic.SharedOptions.EnableSubmitAlerts
                && postedReturn.Organisation.Returns.Count(r => r.AccountingDate == postedReturn.AccountingDate) == 1
                && !VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
                await _SubmissionService.SharedBusinessLogic.SendEmailService.SendGeoMessageAsync(
                    "GPG Data Submission Notification",
                    $"GPG data was submitted for first time in {postedReturn.AccountingDate.Year} by '{postedReturn.Organisation.OrganisationName}' on {postedReturn.StatusDate.ToShortDateString()}\n\n See {Url.ActionArea("Report", "Viewing", "Viewing", new {employerIdentifier = postedReturnViewModel.EncryptedOrganisationId, year = postedReturn.AccountingDate.Year})}",
                    VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix));

            _SubmissionService.SharedBusinessLogic.NotificationService.SendSuccessfulSubmissionEmailToRegisteredUsers(
                postedReturn,
                GetReportLink(postedReturn),
                GetSubmittedOrUpdated(postedReturn));

            await _SubmissionPresenter.DiscardDraftFileAsync(postedReturnViewModel);

            return RedirectToAction("SubmissionComplete");
        }

        [HttpPost("cancel-check-data")]
        public async Task<IActionResult> CancelCheckData(ReturnViewModel postedReturnViewModel)
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var stashedReturnViewModel = UnstashModel<ReturnViewModel>();

            #endregion

            if (stashedReturnViewModel == null) return SessionExpiredView();

            postedReturnViewModel.ReportInfo.Draft = stashedReturnViewModel.ReportInfo.Draft;

            postedReturnViewModel.OriginatingAction = "CheckData";
            var hasDraftChanged = postedReturnViewModel.ReportInfo.Draft.HasDraftBeenModifiedDuringThisSession;
            return await PresentUserTheOptionOfSaveDraftOrIgnoreAsync(postedReturnViewModel, hasDraftChanged);
        }

        #endregion
    }
}