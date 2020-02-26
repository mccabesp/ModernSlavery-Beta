using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Database.Models;
using ModernSlavery.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminViewAuditLogsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminViewAuditLogsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("organisation/{id}/audit-logs")]
        public IActionResult ViewOrganisationAuditLogs(long id)
        {
            List<AuditLog> auditLogs = dataRepository.GetAll<AuditLog>()
                .Where(audit => audit.Organisation.OrganisationId == id)
                .OrderByDescending(audit => audit.CreatedDate)
                .ToList();
            var organisation = dataRepository.Get<Organisation>(id);

            var adminViewAuditLogsViewModel = new AdminViewAuditLogsViewModel {AuditLogs = auditLogs, Organisation = organisation};

            return View("../Admin/ViewAuditLogs", adminViewAuditLogsViewModel);
        }


        [HttpGet("user/{id}/audit-logs")]
        public IActionResult ViewUserAuditLogs(long id)
        {
            List<AuditLog> auditLogs = dataRepository.GetAll<AuditLog>()
                .Where(audit => audit.OriginalUser.UserId == id || audit.ImpersonatedUser.UserId == id)
                .OrderByDescending(audit => audit.CreatedDate)
                .ToList();
            var user = dataRepository.Get<User>(id);

            var adminViewAuditLogsViewModel = new AdminViewAuditLogsViewModel { AuditLogs = auditLogs, User = user };

            return View("../Admin/ViewAuditLogs", adminViewAuditLogsViewModel);
        }
    }
}
