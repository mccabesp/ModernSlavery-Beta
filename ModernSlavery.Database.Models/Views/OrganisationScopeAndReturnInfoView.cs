using System;
using System.Collections.Generic;

namespace ModernSlavery.Entities
{
    public partial class OrganisationScopeAndReturnInfoView
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public string OrganisationStatus { get; set; }
        public string SectorType { get; set; }
        public string ScopeStatus { get; set; }
        public int? SnapshotDate { get; set; }
        public string SicCodeSectionDescription { get; set; }
        public long? ReturnId { get; set; }
        public string OrganisationSize { get; set; }
        public string PublicSectorDescription { get; set; }
    }
}
