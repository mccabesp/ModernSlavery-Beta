using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class OrganisationRecord: AddressModel
    {
        public Dictionary<string, string> References = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public long OrganisationId { get; set; }
        public string DUNSNumber { get; set; }
        public string OrganisationReference { get; set; }
        public string CompanyNumber { get; set; }
        public DateTime? DateOfCessation { get; set; }

        public string OrganisationName { get; set; }
        public SectorTypes SectorType { get; set; }

        public string NameSource { get; set; }
        public long ActiveAddressId { get; set; }

        public string AddressSource { get; set; }

        public string SicCodeIds { get; set; }
        public string SicSource { get; set; }

        //Public Sector
        public string EmailDomains { get; set; }
        public string RegistrationStatus { get; set; }

        public string SicSectors { get; set; }

        public SortedSet<int> GetSicCodes()
        {
            var codes = new SortedSet<int>();
            foreach (var sicCode in SicCodeIds.SplitI()) codes.Add(sicCode.ToInt32());

            return codes;
        }

        public bool IsAuthorised(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress()) throw new ArgumentException("Bad email address");

            if (string.IsNullOrWhiteSpace(EmailDomains)) return false;

            var emailDomains = EmailDomains.SplitI(";")
                .Select(ep => ep.ContainsI("*@") ? ep : ep.Contains('@') ? "*" + ep : "*@" + ep)
                .ToList();
            return emailDomains.Count > 0 && emailAddress.LikeAny(emailDomains);
        }

        public AddressModel ToAddressModel()
        {
            return new AddressModel
            {
                Address1 = Address1,
                Address2 = Address2,
                Address3 = Address3,
                City = City,
                County = County,
                Country = County,
                PostCode = PostCode,
                PoBox = PoBox,
                IsUkAddress = IsUkAddress
            };
        }

    }
}