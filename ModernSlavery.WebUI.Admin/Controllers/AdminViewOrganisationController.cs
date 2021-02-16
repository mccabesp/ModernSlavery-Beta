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
    public class AdminViewOrganisationController : BaseController
    {
        private readonly IAdminService _adminService;
        public AdminViewOrganisationController(
            IAdminService adminService,
            ILogger<AdminViewOrganisationController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
        }

        [HttpGet("organisation/{id}")]
        public IActionResult ViewOrganisation(long id)
        {
            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            return View("../Admin/ViewOrganisation", organisation);
        }
    }
}