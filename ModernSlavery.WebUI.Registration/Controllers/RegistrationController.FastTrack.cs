using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Registration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        [Authorize]
        [HttpGet("fast-track")]
        public async Task<IActionResult> FastTrack()
        {
            // lockout from spam, return custom error
            var remainingTime = await GetRetryLockRemainingTimeAsync("lastFastTrackCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1125, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));

            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            return View("FastTrack", new FastTrackViewModel());
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("fast-track")]
        public async Task<IActionResult> FastTrack(FastTrackViewModel model)
        {
            // lockout from spam, return custom error
            var remainingTime = await GetRetryLockRemainingTimeAsync("lastFastTrackCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1125, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));

            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<FastTrackViewModel>();
                return View("FastTrack", model);
            }

            var vm = await _registrationPresenter.CreateOrganisationViewModelAsync(model, CurrentUser);

            var organisation = await this._registrationService
                .OrganisationBusinessLogic
                .GetOrganisationByEmployerReferenceAndSecurityCodeAsync(model.EmployerReference, model.SecurityCode);

            await IncrementRetryCountAsync("lastFastTrackCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
            if (vm == null)
            {
                // fail - no organisation found
                ModelState.AddModelError(3027);
                return View("FastTrack", model);
            }
            else if (vm.IsSecurityCodeExpired)
            {
                // fail - expired
                ModelState.AddModelError(1144, nameof(FastTrackViewModel.SecurityCode));
                return View("FastTrack", model);
            }
            else if (vm.IsRegistered)
            {
                // fail - cant link to org if they are already linked
                ModelState.AddModelError(3032);
                return View("FastTrack", model);
            }

            // success state 
            await ClearRetryLocksAsync("lastFastTrackCode");

            vm.ConfirmReturnAction = nameof(FastTrack);
            StashModel(vm);
            return RedirectToAction(nameof(ConfirmOrganisation));
        }
    }
}
