﻿using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Models
{
    public class UnregisteredOrganisationsFileModel
    {
        public long OrganisationId { get; set; }
        public string DUNSNumber { get; set; }
        public string OrganisationReference { get; set; }
        public SectorTypes Sector { get; set; }
        public string Company { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public long CreatedByUserId { get; set; }
        public string Expires { get; set; }
        public ScopeStatuses? ScopeStatus { get; set; }
        public string HasSubmitted { get; set; }
    }
}