using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.Admin)]
    [Route("admin")]
    [NoCache]
    public class AdminViewUserController : BaseController
    {
        private readonly IAdminService _adminService;
        public AdminViewUserController(
            IAdminService adminService,
            ILogger<AdminViewUserController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
        }

        [HttpGet("user/{id}")]
        public IActionResult ViewUser(long id)
        {
            var user = _adminService.SharedBusinessLogic.DataRepository.Get<User>(id);

            return View("../Admin/ViewUser", user);
        }
    }
}