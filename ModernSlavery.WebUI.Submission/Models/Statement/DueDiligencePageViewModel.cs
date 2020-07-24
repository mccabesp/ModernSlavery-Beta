using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.Azure.Management.WebSites.Models;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Models.Statement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class DueDiligencePageViewModelMapperProfile:Profile
    {
        public DueDiligencePageViewModelMapperProfile()
        {
            CreateMap<StatementModel.DiligenceModel, DueDiligencePageViewModel.DueDiligenceViewModel>().ReverseMap();

            CreateMap<StatementModel, DueDiligencePageViewModel>()
                .ForMember(s => s.HasForceLabour, opt => opt.Ignore())
                .ForMember(s => s.HasSlaveryInstance, opt => opt.Ignore())
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore())
                .ForMember(s => s.SelectedRemediation, opt => opt.Ignore())
                .ForMember(s => s.OtherRemediation, opt => opt.Ignore());

            CreateMap<DueDiligencePageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.HasForceLabour, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.HasSlaveryInstance, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SelectedRemediation, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.OtherRemediation, opt => opt.DoNotValidate());
        }
    }

    public class DueDiligencePageViewModel : BaseViewModel
    {
        public DueDiligencePageViewModel(DiligenceTypeIndex diligenceTypes)
        {
            DiligenceTypes = diligenceTypes;
        }

        #region Types
        public class DueDiligenceViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
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
        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 2";

        public DiligenceTypeIndex DiligenceTypes { get; }
        public List<DueDiligenceViewModel> DueDiligences { get; set; }

        [Display(Name = "Examples include no formal identification, or who are always dropped off and collected in the same way, often late at night or early in the morning.")]
        public bool HasForceLabour { get; set; }
        [MaxLength(500)]
        public string ForcedLabourDetails { get; set; }

        [Display(Name = "Have you or anyone else found instances of modern slavery in your operations or supply chain in the last year?")]
        public bool HasSlaveryInstance { get; set; }
        [MaxLength(500)]
        public string SlaveryInstanceDetails { get; set; }

        public StatementRemediation SelectedRemediation { get; set; }
        private string _OtherRemediation;
        public string OtherRemediation 
        { 
            get
            {
                return _OtherRemediation;
            }
            set
            {
                _OtherRemediation = value;
                if (!string.IsNullOrWhiteSpace(_OtherRemediation)) SelectedRemediation = StatementRemediation.other;
            }
        }

        public string SlaveryInstanceRemediation
        {
            get
            {
                if (SelectedRemediation == StatementRemediation.other)
                    return OtherRemediation;
                else
                    return SelectedRemediation.GetAttribute<GovUkRadioCheckboxLabelTextAttribute>().Text;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SelectedRemediation = StatementRemediation.none;
                else
                {
                    var selectedRemediation=Enum.GetValues(typeof(StatementRemediation)).ToList<StatementRemediation>().FirstOrDefault(e => e.GetAttribute<GovUkRadioCheckboxLabelTextAttribute>().Text.EqualsI(value));
                    if (selectedRemediation==null)
                    {
                        selectedRemediation = StatementRemediation.other;
                        OtherRemediation = value;
                    }
                    SelectedRemediation = selectedRemediation;
                }
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var otherId = DiligenceTypes.Single(x => x.Description.Equals("other")).Id;
            var otherDiligence = DueDiligences.FirstOrDefault(x => x.Id==otherId);
            if (otherDiligence!=null && string.IsNullOrWhiteSpace(otherDiligence.Details))
                yield return new ValidationResult("Please enter other details");

            if (HasForceLabour == true & string.IsNullOrWhiteSpace(ForcedLabourDetails))
                yield return new ValidationResult("Please provide the detail");

            if (HasSlaveryInstance == true & string.IsNullOrWhiteSpace(SlaveryInstanceDetails))
                yield return new ValidationResult("Please provide the detail");

            //TODO: how to check checkbox here as no isSelected
            //if (HasSlaveryInstance == true & SlaveryInstanceRemediation.None(x => x.IsSelected))
            //    validationResults.Add(new ValidationResult("Please provide the detail"));
        }
        public override bool IsComplete()
        {
            return base.IsComplete();
        }
    }
}
