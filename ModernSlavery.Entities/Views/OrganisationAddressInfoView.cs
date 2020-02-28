using System;
using System.Collections.Generic;

namespace ModernSlavery.Entities
{
    public partial class OrganisationAddressInfoView
    {
        public long OrganisationId { get; set; }
        public string AddressStatus { get; set; }
        public string FullAddress { get; set; }
        public string AddressStatusDetails { get; set; }
        public DateTime AddressStatusDate { get; set; }
        public string AddressSource { get; set; }
        public DateTime AddressCreated { get; set; }
        public DateTime AddressModified { get; set; }
    }
}
