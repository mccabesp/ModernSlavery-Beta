using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Account.Controllers
{
    public class AccountController:BaseController
    {
        public AccountController(
            ILogger<AccountController> logger,
            IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger,webService,sharedBusinessLogic)
        {
        }

        [HttpGet("~/sign-out")]
        public async Task<IActionResult> SignOut()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("SignOut");
            }

            string returnUrl = Url.Action("SignOut", "Account", null, "https");

            return await LogoutUser(returnUrl);
        }

    }
}
