using System;
using System.Collections.Generic;

namespace ModernSlavery.Database
{
    public partial class OrganisationInfoView
    {
        public long OrganisationId { get; set; }
        public string EmployerReference { get; set; }
        public string Dunsnumber { get; set; }
        public string CompanyNumber { get; set; }
        public string OrganisationName { get; set; }
        public string SectorType { get; set; }
        public string OrganisationStatus { get; set; }
        public string SecurityCode { get; set; }
        public DateTime? SecurityCodeExpiryDateTime { get; set; }
        public DateTime? SecurityCodeCreatedDateTime { get; set; }
    }
}
