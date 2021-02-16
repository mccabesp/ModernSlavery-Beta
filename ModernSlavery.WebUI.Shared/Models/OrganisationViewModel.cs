using System;
using System.Collections.Generic;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class OrganisationViewModel
    {
        public bool PINExpired;
        public bool PINSent;

        public string ConfirmReturnAction { get; set; }
        public string AddressReturnAction { get; set; }

        public bool IsManualRegistration { get; set; }
        public bool IsManualAuthorised { get; set; }
        public bool IsFastTrackAuthorised { get; set; }
        public bool IsSecurityCodeExpired { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsManualAddress { get; set; }
        public string RegisteredAddress { get; set; }
        public bool IsWrongAddress { get; set; }

        public string BackAction { get; set; }

        public string ReviewCode { get; set; }
        public string CancellationReason { get; set; }
        
        public SortedSet<int> GetSicCodeIds()
        {
            var codes = new SortedSet<int>();
            foreach (var sicCode in SicCodeIds.SplitI(";,: \n\r".ToCharArray())) codes.Add(sicCode.ToInt32());

            return codes;
        }

        public AddressModel GetAddressModel()
        {
            return new AddressModel
            {
                Address1 = Address1,
                Address2 = Address2,
                Address3 = Address3,
                City = City,
                County = County,
                Country = Country,
                PostCode = Postcode,
                PoBox = PoBox
            };
        }

        #region Search details

        public bool? IsFastTrack { get; set; }

        public SectorTypes? SectorType { get; set; }

        public string SearchText { get; set; }

        public int LastPrivateSearchRemoteTotal { get; set; }

        #endregion

        #region Organisation details

        public string OrganisationName { get; set; }

        public string NameSource { get; set; }

        public DateTime? DateOfCessation { get; set; }

        public bool NoReference { get; set; }

        public string CompanyNumber { get; set; }

        public string CharityNumber { get; set; }

        public string MutualNumber { get; set; }

        public string DUNSNumber { get; set; }

        public string OtherName { get; set; }

        public string OtherValue { get; set; }

        public bool IsDUNS =>
            OtherName.EqualsI(
                "DUNS",
                "D-U-N-S",
                "DUNS no",
                "D-U-N-S no",
                "DUNS number",
                "D-U-N-S number",
                "DUNS reference",
                "D-U-N-S reference",
                "DUNS ref",
                "D-U-N-S ref");

        #endregion

        #region Address details
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string PoBox { get; set; }
        public string AddressSource { get; set; }
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
            list.Add(Postcode);
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

            if (!string.IsNullOrWhiteSpace(Postcode)) list.Add(Postcode);

            if (!string.IsNullOrWhiteSpace(PoBox)) list.Add(PoBox);

            return list;
        }
        #endregion

        #region Contact details
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactEmailAddress { get; set; }
        public string EmailAddress { get; set; }
        public string ContactPhoneNumber { get; set; }
        #endregion

        #region SIC code details
        public string SicCodeIds { get; set; }
        public List<int> SicCodes { get; set; } = new List<int>();
        public string SicSource { get; set; }
        #endregion

        #region Manual Organisations
        public int MatchedReferenceCount { get; set; }

        public List<OrganisationRecord> ManualOrganisations { get; set; }
        public int ManualOrganisationIndex { get; set; }

        public OrganisationRecord GetManualOrganisation()
        {
            if (ManualOrganisationIndex > -1 && ManualOrganisations != null && ManualOrganisationIndex < ManualOrganisations.Count)
                return ManualOrganisations[ManualOrganisationIndex];

            return null;
        }

        public Dictionary<string, string> GetReferences(int i)
        {
            if (ManualOrganisations == null || i >= ManualOrganisations.Count) return null;

            var organisation = ManualOrganisations[i];
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(organisation.DUNSNumber)) results["DUNS No"] = organisation.DUNSNumber;

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber)) results["Company No"] = organisation.CompanyNumber;

            foreach (var key in organisation.References.Keys)
                if (key.EqualsI(nameof(CharityNumber)))
                    results["Charity No"] = organisation.References[nameof(CharityNumber)];
                else if (key.EqualsI(nameof(MutualNumber)))
                    results["Mutual No"] = organisation.References[nameof(MutualNumber)];
                else
                    results[key] = organisation.References[key];

            return results;
        }

        #endregion

        #region Selected Organisation details

        public PagedResult<OrganisationRecord> Organisations { get; set; }

        public int SelectedOrganisationIndex { get; set; }

        public OrganisationRecord GetSelectedOrganisation()
        {
            if (SelectedOrganisationIndex > -1
                && Organisations != null
                && Organisations.Results != null
                && SelectedOrganisationIndex < Organisations.Results.Count)
                return Organisations.Results[SelectedOrganisationIndex];

            return null;
        }

        #endregion
    }
}