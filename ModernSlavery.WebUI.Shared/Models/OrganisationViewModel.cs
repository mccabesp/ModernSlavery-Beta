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
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class OrganisationViewModel:BaseViewModel
    {
        public bool PINExpired;
        public bool PINSent;

        public string ConfirmReturnAction { get; set; }
        public string AddressReturnAction { get; set; }

        public bool IsManualRegistration { get; set; }
        [Secured] public bool IsManualAuthorised { get; set; }
        [Secured] public bool IsSelectedAuthorised { get; set; }
        [Secured] public bool IsFastTrackAuthorised { get; set; }
        [Secured] public bool IsSecurityCodeExpired { get; set; }
        [Secured] public bool IsRegistered { get; set; }
        public bool IsManualAddress { get; set; }
        public string RegisteredAddress { get; set; }
        public bool IsWrongAddress { get; set; }

        public string BackAction { get; set; }

        public string ReviewCode { get; set; }
        public string CancellationReason { get; set; }
        
        public SortedSet<int> GetSicCodeIds()
        {
            var separators = ";,: \n\r" + Environment.NewLine;
            var codes = new SortedSet<int>();
            foreach (var sicCode in SicCodeIds.SplitI(separators)) codes.Add(sicCode.ToInt32());

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

        [Required(AllowEmptyStrings = false)]
        public string RegistrationType { get; set; }

        [Required(AllowEmptyStrings = false)] public SectorTypes? SectorType { get; set; }

        [Required]
        [StringLength(
            100,
            ErrorMessage = "You must enter an organisations name or company number between 3 and 100 characters in length",
            MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        public int LastPrivateSearchRemoteTotal { get; set; }

        #endregion

        #region Organisation details

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OrganisationName { get; set; }

        public string NameSource { get; set; }

        public DateTime? DateOfCessation { get; set; }

        public bool NoReference { get; set; }

        [Required(AllowEmptyStrings = false)]
        [CompanyNumber]
        public string CompanyNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string CharityNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string MutualNumber { get; set; }

        [DUNSNumber] public string DUNSNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
        public string OtherName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 3)]
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

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Address1 { get; set; }

        [MaxLength(100)] public string Address2 { get; set; }

        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(20, MinimumLength = 3)]
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

        #endregion

        #region Contact details

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactFirstName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactLastName { get; set; }

        [MaxLength(50)]
        [Required(AllowEmptyStrings = false)]
        public string ContactJobTitle { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.EmailAddress)]
        public string ContactEmailAddress { get; set; }

        public string EmailAddress { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Phone]
        [MaxLength(20)]
        public string ContactPhoneNumber { get; set; }

        #endregion

        #region SIC code details

        [Required(AllowEmptyStrings = false)] public string SicCodeIds { get; set; }

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

        public int OrganisationStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return Organisations.CurrentPage * Organisations.PageSize - Organisations.PageSize + 1;
            }
        }

        public int OrganisationEndIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return OrganisationStartIndex + Organisations.Results.Count - 1;
            }
        }

        public int PagerStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.PageCount <= 5) return 1;

                if (Organisations.CurrentPage < 4) return 1;

                if (Organisations.CurrentPage + 2 > Organisations.PageCount) return Organisations.PageCount - 4;

                return Organisations.CurrentPage - 2;
            }
        }

        public int PagerEndIndex
        {
            get
            {
                if (Organisations == null) return 1;

                if (Organisations.PageCount <= 5) return Organisations.PageCount;

                if (PagerStartIndex + 4 > Organisations.PageCount) return Organisations.PageCount;

                return PagerStartIndex + 4;
            }
        }

        #endregion
    }
}