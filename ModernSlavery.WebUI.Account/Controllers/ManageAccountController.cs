﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Account.Models.ManageAccount;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Shared.Resources;

namespace ModernSlavery.WebUI.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ManageAccountController : BaseController
    {

        public ManageAccountController(
            ILogger<ManageAccountController> logger,IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
        }

        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // map the user to the view model
            var model = AutoMapper.Map<ManageAccountViewModel>(VirtualUser);

            // check if we have any successful changes
            if (TempData.ContainsKey(nameof(AccountResources.ChangeDetailsSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangeDetailsSuccessAlert;
            }
            else if (TempData.ContainsKey(nameof(AccountResources.ChangePasswordSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangePasswordSuccessAlert;
            }

            // generate flow urls
            ViewBag.CloseAccountUrl = "";
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (IsImpersonatingUser == false)
            {
                ViewBag.CloseAccountUrl = Url.Action<CloseAccountController>(nameof(CloseAccountController.CloseAccount));
                ViewBag.ChangeEmailUrl = Url.Action<ChangeEmailController>(nameof(ChangeEmailController.ChangeEmail));
                ViewBag.ChangePasswordUrl = Url.Action<ChangePasswordController>(nameof(ChangePasswordController.ChangePassword));
                ViewBag.ChangeDetailsUrl = Url.Action<ChangeDetailsController>(nameof(ChangeDetailsController.ChangeDetails));
            }

            // remove any change updates
            TempData.Remove(nameof(AccountResources.ChangeDetailsSuccessAlert));
            TempData.Remove(nameof(AccountResources.ChangePasswordSuccessAlert));

            return View(model);
        }

    }

}
