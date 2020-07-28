using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.Submission.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System.ComponentModel;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class DueDiligencePageViewModelMapperProfile : Profile
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
                .ForMember(d => d.DueDiligences, opt => opt.MapFrom(s => s.DueDiligences.Where(r => r.Id > 0)))
                .ForSourceMember(s => s.DiligenceTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.DueDiligences, opt => opt.DoNotValidate())
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
        [IgnoreMap]
        public DiligenceTypeIndex DiligenceTypes;
        public DueDiligencePageViewModel(DiligenceTypeIndex diligenceTypes)
        {
            DiligenceTypes = diligenceTypes;
        }

        public DueDiligencePageViewModel()
        {

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

        public List<DueDiligenceViewModel> DueDiligences { get; set; } = new List<DueDiligenceViewModel>();

        public bool? HasForceLabour { get; set; }

        [MaxLength(500)]
        public string ForcedLabourDetails { get; set; }

        public bool? HasSlaveryInstance { get; set; }

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
                    var selectedRemediation = Enum.GetValues(typeof(StatementRemediation)).ToList<StatementRemediation>().FirstOrDefault(e => e.GetAttribute<GovUkRadioCheckboxLabelTextAttribute>().Text.EqualsI(value));
                    if (selectedRemediation == null)
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
            //Remove all the empty due diligences
            DueDiligences.RemoveAll(r => r.Id == 0);

            //Get the diligence types
            var DiligenceTypes = validationContext.GetService<DiligenceTypeIndex>();

            var otherId = DiligenceTypes.Single(x => x.Description.EqualsI("other type of social audit")).Id;
            var otherDiligence = DueDiligences.FirstOrDefault(x => x.Id == otherId);
            if (otherDiligence != null && string.IsNullOrWhiteSpace(otherDiligence.Details))
                yield return new ValidationResult("Please enter other details");

            if (HasForceLabour == true & string.IsNullOrWhiteSpace(ForcedLabourDetails))
                yield return new ValidationResult("Please provide the detail");

            if (HasSlaveryInstance == true & string.IsNullOrWhiteSpace(SlaveryInstanceDetails))
                yield return new ValidationResult("Please provide the detail");

            //TODO: how to check checkbox here as no isSelected
            //if (HasSlaveryInstance == true & SlaveryInstanceRemediation.None(x => x.IsSelected))
            //    validationResults.Add(new ValidationResult("Please provide the detail"));
        }
        public bool IsComplete()
        {
            var otherSocialAudit = DiligenceTypes.Single(x => x.Description.EqualsI("other type of social audit"));

            return DueDiligences.Any()
                && HasForceLabour.HasValue
                && HasSlaveryInstance.HasValue
                && !DueDiligences.Any(d=>d.Id==otherSocialAudit.Id && string.IsNullOrWhiteSpace(d.Details))
                && HasForceLabour == false || !string.IsNullOrWhiteSpace(ForcedLabourDetails)
                && HasSlaveryInstance == false || !string.IsNullOrWhiteSpace(SlaveryInstanceDetails)
                && HasSlaveryInstance == false || !string.IsNullOrWhiteSpace(SlaveryInstanceRemediation);
        }
    }
}
