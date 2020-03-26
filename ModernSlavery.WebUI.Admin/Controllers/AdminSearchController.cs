using ModernSlavery.WebUI.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.WebUI.Admin.Classes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminSearchController : Controller
    {

        private readonly AdminSearchService adminSearchService;

        public AdminSearchController(AdminSearchService adminSearchService)
        {
            this.adminSearchService = adminSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchGet(string query)
        {
            if (query == null)
            {
                return View("../Admin/Search", new AdminSearchViewModel());
            }

            var viewModel = new AdminSearchViewModel {SearchQuery = query};

            if (string.IsNullOrWhiteSpace(query))
            {
                viewModel.Error = "Search query must not be empty";
            }
            else
            {
                AdminSearchResultsViewModel results = adminSearchService.Search(query);

                viewModel.SearchResults = results;
            }

            return View("../Admin/Search", viewModel);
        }

    }
}
