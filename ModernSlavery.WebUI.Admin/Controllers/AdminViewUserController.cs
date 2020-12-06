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
    public class AdminViewUserController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminViewUserController(
            IAdminService adminService)
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