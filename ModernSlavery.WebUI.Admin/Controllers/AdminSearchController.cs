using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminSearchController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly AdminSearchService adminSearchService;

        public AdminSearchController(
            IAdminService adminService, 
            AdminSearchService adminSearchService)
        {
            _adminService = adminService;
            this.adminSearchService = adminSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchGet(string query)
        {
            if (query == null) return View("../Admin/Search", new AdminSearchViewModel());

            var viewModel = new AdminSearchViewModel {SearchQuery = query};

            if (string.IsNullOrWhiteSpace(query))
            {
                viewModel.Error = "Search query must not be empty";
            }
            else
            {
                var results = adminSearchService.Search(query);

                viewModel.SearchResults = results;
            }

            return View("../Admin/Search", viewModel);
        }
    }
}