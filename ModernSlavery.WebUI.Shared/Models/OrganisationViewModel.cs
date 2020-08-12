﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class OrganisationViewModel
    {
        public bool PINExpired;
        public bool PINSent;

        public string ConfirmReturnAction { get; set; }
        public string AddressReturnAction { get; set; }

        public bool ManualRegistration { get; set; }
        public bool ManualAuthorised { get; set; }
        public bool SelectedAuthorised { get; set; }
        public bool ManualAddress { get; set; }
        public string RegisteredAddress { get; set; }
        public bool WrongAddress { get; set; }

        public string BackAction { get; set; }

        public string ReviewCode { get; set; }
        public string CancellationReason { get; set; }
        public bool IsSecurityCodeExpired { get; set; }
        public bool IsFastTrackAuthorised { get; set; }
        public bool IsRegistered { get; set; }

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
            ErrorMessage = "You must enter an employers name or company number between 3 and 100 characters in length",
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
        [StringLength(100, MinimumLength = 3)]
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

        public List<int> SicCodes { get; set; }
        public string SicSource { get; set; }

        #endregion

        #region Manual Employers

        public int MatchedReferenceCount { get; set; }

        public List<OrganisationRecord> ManualEmployers { get; set; }
        public int ManualEmployerIndex { get; set; }

        public OrganisationRecord GetManualEmployer()
        {
            if (ManualEmployerIndex > -1 && ManualEmployers != null && ManualEmployerIndex < ManualEmployers.Count)
                return ManualEmployers[ManualEmployerIndex];

            return null;
        }

        public Dictionary<string, string> GetReferences(int i)
        {
            if (ManualEmployers == null || i >= ManualEmployers.Count) return null;

            var employer = ManualEmployers[i];
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(employer.DUNSNumber)) results["DUNS No"] = employer.DUNSNumber;

            if (!string.IsNullOrWhiteSpace(employer.CompanyNumber)) results["Company No"] = employer.CompanyNumber;

            foreach (var key in employer.References.Keys)
                if (key.EqualsI(nameof(CharityNumber)))
                    results["Charity No"] = employer.References[nameof(CharityNumber)];
                else if (key.EqualsI(nameof(MutualNumber)))
                    results["Mutual No"] = employer.References[nameof(MutualNumber)];
                else
                    results[key] = employer.References[key];

            return results;
        }

        #endregion

        #region Selected Employer details

        public PagedResult<OrganisationRecord> Employers { get; set; }

        public int SelectedEmployerIndex { get; set; }

        public OrganisationRecord GetSelectedEmployer()
        {
            if (SelectedEmployerIndex > -1
                && Employers != null
                && Employers.Results != null
                && SelectedEmployerIndex < Employers.Results.Count)
                return Employers.Results[SelectedEmployerIndex];

            return null;
        }

        public int EmployerStartIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1) return 1;

                return Employers.CurrentPage * Employers.PageSize - Employers.PageSize + 1;
            }
        }

        public int EmployerEndIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1) return 1;

                return EmployerStartIndex + Employers.Results.Count - 1;
            }
        }

        public int PagerStartIndex
        {
            get
            {
                if (Employers == null || Employers.PageCount <= 5) return 1;

                if (Employers.CurrentPage < 4) return 1;

                if (Employers.CurrentPage + 2 > Employers.PageCount) return Employers.PageCount - 4;

                return Employers.CurrentPage - 2;
            }
        }

        public int PagerEndIndex
        {
            get
            {
                if (Employers == null) return 1;

                if (Employers.PageCount <= 5) return Employers.PageCount;

                if (PagerStartIndex + 4 > Employers.PageCount) return Employers.PageCount;

                return PagerStartIndex + 4;
            }
        }

        #endregion
    }
}