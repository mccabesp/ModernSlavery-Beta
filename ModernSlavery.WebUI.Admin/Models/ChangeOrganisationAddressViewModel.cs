using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class ChangeOrganisationAddressViewModel : GovUkViewModel
    {
        // Not mapped, only used for displaying information in the views
        [BindNever] 
        public Organisation Organisation { get; set; }

        public ManuallyChangeOrganisationAddressViewModelActions Action { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing =
            "Select whether you want to use this address from Companies House or to enter an address manually")]
        public AcceptCompaniesHouseAddress? AcceptCompaniesHouseAddress { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [Text]
        public string Reason { get; set; }

        #region Used by Manual Change page

        [Text] 
        public string PoBox { get; set; }
        [Text] 
        public string Address1 { get; set; }
        [Text] 
        public string Address2 { get; set; }
        [Text] 
        public string Address3 { get; set; }
        [Text] 
        public string TownCity { get; set; }
        [Text] 
        public string County { get; set; }
        [Text] 
        public string Country { get; set; }
        [Text] 
        public string PostCode { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select whether or not this is a UK address")]
        public ManuallyChangeOrganisationAddressIsUkAddress? IsUkAddress { get; set; }

        public void PopulateFromOrganisationAddress(OrganisationAddress address)
        {
            PoBox = address.PoBox;
            Address1 = address.Address1;
            Address2 = address.Address2;
            Address3 = address.Address3;
            TownCity = address.TownCity;
            County = address.County;
            Country = address.Country;
            PostCode = address.PostCode;

            if (address.IsUkAddress.HasValue)
                IsUkAddress = address.IsUkAddress.Value
                    ? ManuallyChangeOrganisationAddressIsUkAddress.Yes
                    : ManuallyChangeOrganisationAddressIsUkAddress.No;
        }

        #endregion
    }

    public enum ManuallyChangeOrganisationAddressIsUkAddress : byte
    {
        Yes,
        No
    }

    public enum AcceptCompaniesHouseAddress : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes, use this address from Companies House")]
        Accept,

        [GovUkRadioCheckboxLabelText(Text = "No, enter an address manually")]
        Reject
    }

    public enum ManuallyChangeOrganisationAddressViewModelActions : byte
    {
        Unknown = 0,
        OfferNewCompaniesHouseAddress = 1,
        ManualChange = 2,
        CheckChangesManual = 3,
        CheckChangesCoHo = 4
    }
}