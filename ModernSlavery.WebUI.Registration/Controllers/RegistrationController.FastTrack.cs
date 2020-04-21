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

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        [Authorize]
        [HttpGet("fast-track")]
        public async Task<IActionResult> FastTrack()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //TODO James - match functionality from GPG, eg lockout/spam protection

            return View("FastTrack", new FastTrackViewModel());
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("fast-track")]
        public async Task<IActionResult> FastTrack(FastTrackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<FastTrackViewModel>();
                return View("FastTrack", model);
            }

            // TODO James - lockout after 3 attempts

            var organisation = await this._registrationService
                .OrganisationBusinessLogic
                .GetOrganisationByEmployerReferenceAndSecurityCodeAsync(model.EmployerReference, model.SecurityCode);

            if (organisation == null)
            {
                // fail - no organisation found
                ModelState.AddModelError(3027); // is this the correct error code?
                return View("FastTrack", model);
            }
            if (organisation.HasSecurityCodeExpired())
            {
                // fail - expired
                ModelState.AddModelError(1144, nameof(FastTrackViewModel.SecurityCode)); // is this the correct error code?
                return View("FastTrack", model);
            }
            // Cant link to org if they are already linked
            if (organisation.UserOrganisations.Any(uo => uo.User == CurrentUser && uo.PINConfirmedDate == null))
            {
                // fail - organisation already registered
                ModelState.AddModelError(3032); // is this the correct error code? it is duplicated in config
                return View("FastTrack", model);
            }

            // success state 
            var vm = await _registrationPresenter.CreateOrganisationViewModelAsync(model, CurrentUser);
            StashModel(vm);
            return RedirectToAction(nameof(ConfirmOrganisation));
        }
    }
}
