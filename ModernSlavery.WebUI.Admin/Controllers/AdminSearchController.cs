using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.WebUI.Admin.Classes;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoleNames.BasicAdmin)]
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
        public async Task<IActionResult> SearchGet(AdminSearchViewModel viewModel)
        {
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            if (!ModelState.IsValid)
                this.SetModelCustomErrors(viewModel);
            else
                viewModel.SearchResults = adminSearchService.Search(viewModel.Search);

            return View("../Admin/Search", viewModel);
        }
    }
}