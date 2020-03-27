using System.Collections.Generic;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminViewAuditLogsViewModel : GovUkViewModel
    {
        public IEnumerable<AuditLog> AuditLogs { get; set; }
        public Organisation Organisation { get; set; }
        public User User { get; set; }
    }
}