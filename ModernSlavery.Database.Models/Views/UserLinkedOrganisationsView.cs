using System;
using System.Collections.Generic;

namespace ModernSlavery.Database
{
    public partial class UserLinkedOrganisationsView
    {
        public long UserId { get; set; }
        public long OrganisationId { get; set; }
        public string Dunsnumber { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public string OrganisationName { get; set; }
        public string SectorTypeId { get; set; }
        public string StatusId { get; set; }
    }
}
