using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.Admin)]
    [Route("admin")]
    [NoCache]
    public class AdminSearchController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly AdminSearchService adminSearchService;

        public AdminSearchController(
            IAdminService adminService, 
            AdminSearchService adminSearchService,
            ILogger<AdminSearchController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _adminService = adminService;
            this.adminSearchService = adminSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchGet([Text]string query)
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