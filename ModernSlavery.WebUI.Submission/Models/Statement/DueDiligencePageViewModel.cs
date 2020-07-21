﻿using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class DueDiligencePageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public List<DueDiligenceViewModel> DueDiligences { get; set; }

        [Display(Name = "Examples include no formal identification, or who are always dropped off and collected in the same way, often late at night or early in the morning.")]
        public bool HasForceLabour { get; set; }
        [MaxLength(500)]
        public string ForcedLabourDetails { get; set; }

        [Display(Name = "Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?")]
        public bool HasSlaveryInstance { get; set; }
        [MaxLength(500)]
        public string SlaveryInstanceDetails { get; set; }

        public List<StatementRemediation> SlaveryInstanceRemediation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            var validationResults = new List<ValidationResult>();
            var otherDiligence = DueDiligences.Single(x => x.Description.Equals("other"));
            if (otherDiligence.IsSelected && otherDiligence.OtherDiligence.IsNull())
                validationResults.Add(new ValidationResult("Please enter other details"));

            if (HasForceLabour == true & ForcedLabourDetails.IsNull())
                validationResults.Add(new ValidationResult("Please provide the detail"));

            if (HasSlaveryInstance == true & SlaveryInstanceDetails.IsNull())
                validationResults.Add(new ValidationResult("Please provide the detail"));

            //TODO: how to check checkbox here as no isSelected
            //if (HasSlaveryInstance == true & SlaveryInstanceRemediation.None(x => x.IsSelected))
            //    validationResults.Add(new ValidationResult("Please provide the detail"));



            return validationResults;
        }

        public class DueDiligenceViewModel
        {
            // TODO - James Handle "Other" case
            // It seems to only appear once under "Social audits"
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public List<DueDiligenceViewModel> ChildDiligences { get; set; }
            //TODO: set on presenter level
            public string OtherDiligence { get; set; }
        }
    }

    public enum StatementRemediation : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "repayment of recruitment fees")]
        repaymentOfRecruitmentFees,
        [GovUkRadioCheckboxLabelText(Text = "change in policy")]
        changeInPolicy,
        [GovUkRadioCheckboxLabelText(Text = "referring victims into government services")]
        referringVictimsIntoGovernmentServices,
        [GovUkRadioCheckboxLabelText(Text = "supporting victims via NGOs")]
        supportingVictimsViaNGOs,
        [GovUkRadioCheckboxLabelText(Text = "supporting criminal justice against perpetrator")]
        supportingCriminalJusticeAgainstPerpetrator,
        [GovUkRadioCheckboxLabelText(Text = "other")]
        other,
        [GovUkRadioCheckboxLabelText(Text = "none")]
        none

    }
}
