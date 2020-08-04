using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Core.Models
{
    public class ImportOrganisationModel
    {
        public string OrganisationName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string Sector { get; set; }
        public string CompanyNumber { get; set; }
        public string SICCode { get; set; }
    }
}
