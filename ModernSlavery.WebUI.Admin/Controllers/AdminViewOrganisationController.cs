using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    [NoCache]
    public class AdminViewOrganisationController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminViewOrganisationController(
            IAdminService adminService)
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