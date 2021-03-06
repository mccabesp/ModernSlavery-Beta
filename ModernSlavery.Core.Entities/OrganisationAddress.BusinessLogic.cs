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

        private List<string> GetAddressList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1)) list.Add(Address1.TrimNonLettersOrDigits());

            if (!string.IsNullOrWhiteSpace(Address2)) list.Add(Address2.TrimNonLettersOrDigits());

            if (!string.IsNullOrWhiteSpace(Address3)) list.Add(Address3.TrimNonLettersOrDigits());

            if (!string.IsNullOrWhiteSpace(TownCity)) list.Add(TownCity.TrimNonLettersOrDigits());

            if (!string.IsNullOrWhiteSpace(County)) list.Add(County.TrimNonLettersOrDigits());

            if (!string.IsNullOrWhiteSpace(Country)) list.Add(Country.TrimNonLettersOrDigits());

            return list;
        }


        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Address1)
                   && string.IsNullOrWhiteSpace(Address2)
                   && string.IsNullOrWhiteSpace(Address3)
                   && string.IsNullOrWhiteSpace(TownCity)
                   && string.IsNullOrWhiteSpace(County)
                   && string.IsNullOrWhiteSpace(Country)
                   && string.IsNullOrWhiteSpace(PostCode)
                   && string.IsNullOrWhiteSpace(PoBox);
        }

        public bool HasAnyAddress() => !IsEmpty();

        public bool EqualsI(OrganisationAddress address)
        {
            var add1 = GetAddressString();
            var add2 = address?.GetAddressString();
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
            Modified = statusDate.Value;
            StatusDate = statusDate.Value;
            StatusDetails = details;
            return addressStatus;
        }

        public void Trim()
        {
            var addresses = GetAddressList();

            var breakChars = ",.!;:/\\".ToCharArray();
            var splitIndex = -1;

            var index = addresses.Count - 1;
            const int maxChars = 100;
            while (index >= 0 && addresses.Count < 6 && addresses.Any(address => address.Length > maxChars))
            {
                var address = addresses[index];
                if (address.Length > maxChars)
                {
                    var foundIndex = address.LastIndexOfAny(breakChars);
                    if (foundIndex > -1)
                    {
                        var afterPart = address.Substring(foundIndex + 1).TrimNonLettersOrDigits();
                        var beforePart = address.Substring(0, foundIndex).TrimNonLettersOrDigits();

                        addresses.Insert(index + 1, afterPart);
                        addresses[index] = beforePart;
                        splitIndex = index;
                        index++;
                    }
                }

                index--;
            }

            if (splitIndex > -1)
            {

                if (splitIndex < 1) Address1 = addresses.Count > 0 ? addresses[0] : null;
                if (splitIndex < 2) Address2 = addresses.Count > 1 ? addresses[1] : null;
                if (splitIndex < 3) Address3 = addresses.Count > 2 ? addresses[2] : null;
                if (splitIndex < 4) TownCity = addresses.Count > 3 ? addresses[3] : null;
                if (splitIndex < 5) County = addresses.Count > 4 ? addresses[4] : null;
                if (splitIndex < 6) Country = addresses.Count > 5 ? addresses[5] : null;
            }

            Address1 = Address1?.TrimNonLettersOrDigits().Left(maxChars * 2);
            Address2 = Address2?.TrimNonLettersOrDigits().Left(maxChars);
            Address3 = Address3?.TrimNonLettersOrDigits().Left(maxChars);
            TownCity = TownCity?.TrimNonLettersOrDigits().Left(maxChars);
            County = County?.TrimNonLettersOrDigits().Left(maxChars);
            Country = Country?.TrimNonLettersOrDigits().Left(maxChars);
            PostCode = PostCode?.TrimNonLettersOrDigits().Left(20);
            PoBox = PoBox?.TrimNonLettersOrDigits().Left(30);
        }

        #endregion
    }
}