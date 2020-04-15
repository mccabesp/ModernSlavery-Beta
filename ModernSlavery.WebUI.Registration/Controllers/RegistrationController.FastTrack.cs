using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Registration.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    partial class RegistrationController
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

            return View("FastTrack", model);
        }
    }
}
