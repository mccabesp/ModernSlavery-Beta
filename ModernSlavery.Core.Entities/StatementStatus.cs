using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementStatus
    {
        public long StatementStatusId { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public ReturnStatuses Status { get; set; }

        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;

        public string StatusDetails { get; set; }

        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
    }
}
