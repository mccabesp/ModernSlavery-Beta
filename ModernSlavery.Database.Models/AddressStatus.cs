using System;
using ModernSlavery.Entities.Enums;
using ModernSlavery.Extensions;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.Entities
{
    public class AddressStatus
    {

        public long AddressStatusId { get; set; }
        public long AddressId { get; set; }
        public AddressStatuses Status { get; set; }
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public long ByUserId { get; set; }

        public virtual OrganisationAddress Address { get; set; }
        public virtual User ByUser { get; set; }

    }
}
