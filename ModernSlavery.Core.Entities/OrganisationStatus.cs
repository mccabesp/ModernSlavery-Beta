using System;
using ModernSlavery.Entities.Enums;
using ModernSlavery.Extensions;

namespace ModernSlavery.Entities
{
    public class OrganisationStatus
    {
        public long OrganisationStatusId { get; set; }
        public long OrganisationId { get; set; }
        public OrganisationStatuses Status { get; set; }

        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual Organisation Organisation { get; set; }
    }
}