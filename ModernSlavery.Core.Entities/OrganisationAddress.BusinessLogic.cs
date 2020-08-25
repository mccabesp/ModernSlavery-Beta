using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public partial class OrganisationAddress
    {
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var address = obj as OrganisationAddress;
            if (address == null) return false;

            return AddressId == address.AddressId;
        }

        public bool AddressMatches(OrganisationAddress other)
        {
            return string.Equals(Address1, other.Address1, StringComparison.Ordinal)
                   && string.Equals(Address2, other.Address2, StringComparison.Ordinal)
                   && string.Equals(Address3, other.Address3, StringComparison.Ordinal)
                   && string.Equals(TownCity, other.TownCity, StringComparison.Ordinal)
                   && string.Equals(County, other.County, StringComparison.Ordinal)
                   && string.Equals(Country, other.Country, StringComparison.Ordinal)
                   && string.Equals(PostCode, other.PostCode, StringComparison.Ordinal)
                   && string.Equals(PoBox, other.PoBox, StringComparison.Ordinal);
        }

        #region Methods

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

            if (!string.IsNullOrWhiteSpace(PoBox)) list.Add(PoBox.TrimI());

            return list;
        }

        public bool EqualsI(OrganisationAddress address)
        {
            var add1 = GetAddressString();
            var add2 = address == null ? null : address.GetAddressString();
            return add1.EqualsI(add2);
        }

        public string GetAddressString(string delimiter = ", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }

        public UserOrganisation GetFirstRegistration()
        {
            return UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINConfirmedDate > Created);
        }

        public DateTime GetFirstRegisteredDate()
        {
            var firstRegistration = GetFirstRegistration();
            return firstRegistration?.PINConfirmedDate ?? Created;
        }

        public AddressStatus SetStatus(AddressStatuses status, long byUserId, string details = null, DateTime? statusDate = null)
        {
            if (status == Status && details == StatusDetails && statusDate == null) return null;

            if (statusDate == null || statusDate == DateTime.MinValue) statusDate = VirtualDateTime.Now;

            var addressStatus = new AddressStatus
                {
                    AddressId = AddressId,
                    Status = status,
                    StatusDate = statusDate.Value,
                    StatusDetails = details,
                    ByUserId = byUserId
                };

            AddressStatuses.Add(addressStatus);
            Status = status;
            StatusDate = statusDate.Value;
            StatusDetails = details;
            return addressStatus;
        }

        #endregion
    }
}