using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

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
        [Text] 
        public string Reason { get; set; }

        #region Not mapped, only used for displaying information in the views

        [BindNever] public Organisation Organisation { get; set; }
        [BindNever] public List<SicCode> SicCodesToAdd { get; set; }
        [BindNever] public List<SicCode> SicCodesToKeep { get; set; }
        [BindNever] public List<SicCode> SicCodesToRemove { get; set; }
        [BindNever] public Dictionary<string, SicCode> SicCodesFromCoHo { get; set; }
        [BindNever] public ChangeOrganisationSicCodesConfirmationType? ConfirmationType { get; set; }

        #endregion

        #region Used for hidden inputs - to keep track of the current state

        [Text(Text.NumberChars)]
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