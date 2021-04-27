using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    [Serializable]
    public class ReviewOrganisationViewModel
    {

        public bool IsManualRegistration { get; set; }
        public bool IsManualAuthorised { get; set; }
        public bool IsSelectedAuthorised { get; set; }
        public bool IsFastTrackAuthorised { get; set; }
        public bool IsSecurityCodeExpired { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsManualAddress { get; set; }
        [BindNever] public string RegisteredAddress { get; set; }
        [BindNever] public bool? IsUKAddress { get; set; }
        [BindNever] public string Details { get; set; }

        [Text] public string ReviewCode { get; set; }

        public const string DefaultCancellationReason = "We haven't been able to verify your organisation's identity. So we have declined your application.";
        [Text] public string CancellationReason { get; set; } = DefaultCancellationReason;

        #region Search details

        public bool? IsFastTrack { get; set; }

        public SectorTypes? SectorType { get; set; }

        #endregion

        #region Organisation details

        [BindNever]public string OrganisationName { get; set; }

        [BindNever]public string CompanyNumber { get; set; }

        [BindNever]public string CharityNumber { get; set; }

        [BindNever]public string MutualNumber { get; set; }

        [BindNever] public string DUNSNumber { get; set; }

        [BindNever] public string OtherName { get; set; }

        [BindNever] public string OtherValue { get; set; }

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

        [BindNever]public string Address1 { get; set; }
        [BindNever] public string Address2 { get; set; }
        [BindNever] public string Address3 { get; set; }
        [BindNever] public string City { get; set; }
        [BindNever] public string County { get; set; }
        [BindNever] public string Country { get; set; }
        [BindNever] public string Postcode { get; set; }
        [BindNever] public string PoBox { get; set; }
        [BindNever] public string AddressSource { get; set; }

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

        [BindNever] public string ContactFirstName { get; set; }

        [BindNever]public string ContactLastName { get; set; }

        [BindNever]public string ContactJobTitle { get; set; }

        [BindNever]public string ContactEmailAddress { get; set; }

        [BindNever] public string EmailAddress { get; set; }

        [BindNever]public string ContactPhoneNumber { get; set; }

        #endregion

        #region SIC code details

        [BindNever] public string SicCodeIds { get; set; }

        [BindNever] public List<int> SicCodes { get; set; } = new List<int>();

        #endregion

        #region Manual Organisations

        public int MatchedReferenceCount { get; set; }

        public List<OrganisationRecord> ManualOrganisations { get; set; }
        public int ManualOrganisationIndex { get; set; }

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

        #endregion
    }
}