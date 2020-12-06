using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    [NoCache]
    public class AdminViewAuditLogsController : Controller
    {
        private readonly IAdminService _adminService;
        public AdminViewAuditLogsController(
            IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("organisation/{id}/audit-logs")]
        public IActionResult ViewOrganisationAuditLogs(long id)
        {
            var auditLogs = _adminService.SharedBusinessLogic.DataRepository.GetAll<AuditLog>()
                .Where(audit => audit.Organisation.OrganisationId == id)
                .OrderByDescending(audit => audit.CreatedDate)
                .ToList();
            var organisation = _adminService.SharedBusinessLogic.DataRepository.Get<Organisation>(id);

            var adminViewAuditLogsViewModel = new AdminViewAuditLogsViewModel
                {AuditLogs = auditLogs, Organisation = organisation};

            return View("../Admin/ViewAuditLogs", adminViewAuditLogsViewModel);
        }


        [HttpGet("user/{id}/audit-logs")]
        public IActionResult ViewUserAuditLogs(long id)
        {
            var auditLogs = _adminService.SharedBusinessLogic.DataRepository.GetAll<AuditLog>()
                .Where(audit => audit.OriginalUser.UserId == id || audit.ImpersonatedUser.UserId == id)
                .OrderByDescending(audit => audit.CreatedDate)
                .ToList();
            var user = _adminService.SharedBusinessLogic.DataRepository.Get<User>(id);

            var adminViewAuditLogsViewModel = new AdminViewAuditLogsViewModel {AuditLogs = auditLogs, User = user};

            return View("../Admin/ViewAuditLogs", adminViewAuditLogsViewModel);
        }
    }
}