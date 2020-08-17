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

        public List<string> GetList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1)) list.Add(Address1.TrimI());

            if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Address2.TrimI());

            if (!string.IsNullOrWhiteSpace(Address3)) list.Add(Address3.TrimI());

            if (!string.IsNullOrWhiteSpace(TownCity)) list.Add(TownCity.TrimI());

            if (!string.IsNullOrWhiteSpace(County)) list.Add(County.TrimI());

            if (!string.IsNullOrWhiteSpace(Country)) list.Add(Country.TrimI());

            if (!string.IsNullOrWhiteSpace(PostCode)) list.Add(PostCode.TrimI());

            return list;
        }

        public string GetAddressString(string delimiter = ", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }


    }
}
