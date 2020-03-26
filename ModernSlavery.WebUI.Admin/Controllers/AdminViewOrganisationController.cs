using ModernSlavery.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.WebUI.Admin.Controllers
{
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
