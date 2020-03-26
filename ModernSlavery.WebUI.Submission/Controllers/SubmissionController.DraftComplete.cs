﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    public partial class SubmissionController : BaseController
    {

        #region public methods

        [HttpGet("draft-complete")]
        public async Task<IActionResult> DraftComplete()
        {
            #region Check user, then retrieve model from Session

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            var stashedReturnViewModel = this.UnstashModel<ReturnViewModel>();

            #endregion

            stashedReturnViewModel = await LoadReturnViewModelFromDBorFromDraftFileAsync(stashedReturnViewModel, VirtualUser.UserId);

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ReturnViewModel>();
                return View("DraftComplete", stashedReturnViewModel);
            }

            await _SubmissionPresenter.UpdateDraftFileAsync(VirtualUser.UserId, stashedReturnViewModel);
            await _SubmissionPresenter.CommitDraftFileAsync(stashedReturnViewModel);

            return View("DraftComplete", stashedReturnViewModel);
        }

        [HttpPost("draft-complete")]
        [PreventDuplicatePost]
        public IActionResult DraftCompletePost(string command)
        {
            string doneUrl = SharedBusinessLogic.SharedOptions.DoneUrl ?? Url.Action("Index", "Viewing", null, "https");

            return Redirect(doneUrl);
        }

        #endregion

    }
}
