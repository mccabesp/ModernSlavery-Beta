using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminViewUserController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminViewUserController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("user/{id}")]
        public IActionResult ViewUser(long id)
        {
            var user = dataRepository.Get<User>(id);

            return View("../Admin/ViewUser", user);
        }

    }
}
