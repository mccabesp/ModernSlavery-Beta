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

            var address = obj as Entities.OrganisationAddress;
            if (address == null) return false;

            return AddressId == address.AddressId;
        }

        public bool AddressMatches(Entities.OrganisationAddress other)
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
            if (!string.IsNullOrWhiteSpace(Address1)) list.Add(Text.TrimI(Address1));

            if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Text.TrimI(Address2));

            if (!string.IsNullOrWhiteSpace(Address3)) list.Add(Text.TrimI(Address3));

            if (!string.IsNullOrWhiteSpace(TownCity)) list.Add(Text.TrimI(TownCity));

            if (!string.IsNullOrWhiteSpace(County)) list.Add(Text.TrimI(County));

            if (!string.IsNullOrWhiteSpace(Country)) list.Add(Text.TrimI(Country));

            if (!string.IsNullOrWhiteSpace(PostCode)) list.Add(Text.TrimI(PostCode));

            if (!string.IsNullOrWhiteSpace(PoBox)) list.Add(Text.TrimI(PoBox));

            return list;
        }

        public bool EqualsI(Entities.OrganisationAddress address)
        {
            var add1 = GetAddressString();
            var add2 = address == null ? null : address.GetAddressString();
            return Text.EqualsI(add1, add2);
        }

        public string GetAddressString(string delimiter = ", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }

        public Entities.UserOrganisation GetFirstRegistration()
        {
            return Enumerable.OrderBy<Entities.UserOrganisation, DateTime?>(UserOrganisations, uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINConfirmedDate > Created);
        }

        public DateTime GetFirstRegisteredDate()
        {
            var firstRegistration = GetFirstRegistration();
            return firstRegistration?.PINConfirmedDate ?? Created;
        }

        public void SetStatus(AddressStatuses status, long byUserId, string details = null, DateTime? statusDate = null)
        {
            if (status == Status && details == StatusDetails && statusDate == null) return;

            if (statusDate == null || statusDate == DateTime.MinValue) statusDate = VirtualDateTime.Now;

            AddressStatuses.Add(
                new AddressStatus
                {
                    AddressId = AddressId,
                    Status = status,
                    StatusDate = statusDate.Value,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = statusDate.Value;
            StatusDetails = details;
        }

        #endregion
    }
}