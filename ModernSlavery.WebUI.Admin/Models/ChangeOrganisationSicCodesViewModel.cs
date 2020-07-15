using System.Collections.Generic;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class ChangeOrganisationSicCodesViewModel : GovUkViewModel
    {
        // "Action" should probably never be set in code
        // It should be mapped from a hidden input and is used to tell the controller what action we want to take
        public ManuallyChangeOrganisationSicCodesActions Action { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "SIC code", NameWithinSentence = "SIC code")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter a SIC code")]
        public int? SicCodeIdToChange { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing =
            "Select whether you want to use this name from Companies House or to enter a name manually")]
        public AcceptCompaniesHouseSicCodes? AcceptCompaniesHouseSicCodes { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        public string Reason { get; set; }

        #region Not mapped, only used for displaying information in the views

        public Organisation Organisation { get; set; }
        public List<SicCode> SicCodesToAdd { get; set; }
        public List<SicCode> SicCodesToKeep { get; set; }
        public List<SicCode> SicCodesToRemove { get; set; }
        public Dictionary<string, SicCode> SicCodesFromCoHo { get; set; }
        public ChangeOrganisationSicCodesConfirmationType? ConfirmationType { get; set; }

        #endregion

        #region Used for hidden inputs - to keep track of the current state

        public List<string> SicCodeIdsFromCoHo { get; set; }
        public List<int> SicCodeIdsToAdd { get; set; }
        public List<int> SicCodeIdsToRemove { get; set; }

        #endregion
    }

    public enum AcceptCompaniesHouseSicCodes : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes, use these SIC codes from Companies House")]
        Accept,

        [GovUkRadioCheckboxLabelText(Text = "No, change SIC codes manually")]
        Reject
    }

    public enum ManuallyChangeOrganisationSicCodesActions : byte
    {
        Unknown,

        OfferCompaniesHouseSicCodesAnswer,

        ManualChangeDoNotAddSicCode,
        ManualChangeAddSicCode,
        ManualChangeRemoveSicCode,
        ManualChangeKeepSicCode,

        ManualChangeContinue,

        MakeMoreManualChanges,

        ConfirmManual,
        ConfirmCoho
    }

    public enum ChangeOrganisationSicCodesConfirmationType : byte
    {
        Manual,
        CoHo
    }
}