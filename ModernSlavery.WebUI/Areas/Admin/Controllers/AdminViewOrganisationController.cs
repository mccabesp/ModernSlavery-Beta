using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminViewOrganisationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminViewOrganisationController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("organisation/{id}")]
        public IActionResult ViewOrganisation(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("../Admin/ViewOrganisation", organisation);
        }

    }
}
