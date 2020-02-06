using ModernSlavery.WebUI.Models.Admin;
using ModernSlavery.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Controllers
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
