using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class AddressModel
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }

        public bool? IsUkAddress { get; set; }

        public string GetFullAddress()
        {
            var list = new List<string>();
            list.Add(Address1);
            list.Add(Address2);
            list.Add(Address3);
            list.Add(City);
            list.Add(County);
            list.Add(Country);
            list.Add(PostCode);
            list.Add(PoBox);
            return list.ToDelimitedString(", ");
        }

        public List<string> GetAddressList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1)) list.Add(Address1);

            if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Address2);

            if (!string.IsNullOrWhiteSpace(Address3)) list.Add(Address3);

            if (!string.IsNullOrWhiteSpace(City)) list.Add(City);

            if (!string.IsNullOrWhiteSpace(County)) list.Add(County);

            if (!string.IsNullOrWhiteSpace(Country)) list.Add(Country);

            if (!string.IsNullOrWhiteSpace(PoBox)) list.Add(PoBox);

            return list;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Address1)
                   && string.IsNullOrWhiteSpace(Address2)
                   && string.IsNullOrWhiteSpace(Address3)
                   && string.IsNullOrWhiteSpace(City)
                   && string.IsNullOrWhiteSpace(County)
                   && string.IsNullOrWhiteSpace(Country)
                   && string.IsNullOrWhiteSpace(PostCode)
                   && string.IsNullOrWhiteSpace(PoBox);
        }

        public static AddressModel Create(OrganisationAddress address)
        {
            return address == null
                ? null
                : new AddressModel
                {
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    Address3 = address.Address3,
                    City = address.TownCity,
                    County = address.County,
                    Country = address.Country,
                    PostCode = address.PostCode,
                    PoBox = address.PoBox
                };
        }

        public override bool Equals(object obj)
        {
            if (obj is OrganisationAddress) obj = Create((OrganisationAddress) obj);

            var address = obj as AddressModel;
            if (address == null) return false;

            if (obj is OrganisationAddress)
            {
            }

            if ((!string.IsNullOrWhiteSpace(Address1) || !string.IsNullOrWhiteSpace(address.Address1))
                && Address1?.Trim() != address.Address1?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(Address2) || !string.IsNullOrWhiteSpace(address.Address2))
                && Address2?.Trim() != address.Address2?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(Address3) || !string.IsNullOrWhiteSpace(address.Address3))
                && Address3?.Trim() != address.Address3?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(City) || !string.IsNullOrWhiteSpace(address.City)) &&
                City?.Trim() != address.City?.Trim()) return false;

            if ((!string.IsNullOrWhiteSpace(County) || !string.IsNullOrWhiteSpace(address.County))
                && County?.Trim() != address.County?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(Country) || !string.IsNullOrWhiteSpace(address.Country))
                && Country?.Trim() != address.Country?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(PostCode) || !string.IsNullOrWhiteSpace(address.PostCode))
                && PostCode?.Trim() != address.PostCode?.Trim())
                return false;

            if ((!string.IsNullOrWhiteSpace(PoBox) || !string.IsNullOrWhiteSpace(address.PoBox)) &&
                PoBox?.Trim() != address.PoBox?.Trim()) return false;

            return true;
        }

        public OrganisationAddress FindAddress(Organisation org, AddressStatuses status = AddressStatuses.Active)
        {
            return org.OrganisationAddresses.FirstOrDefault(a => a.Status == status && Equals(a));
        }
    }
}