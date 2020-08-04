using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Submission.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class DueDiligencePageViewModelMapperProfile : Profile
    {
        public DueDiligencePageViewModelMapperProfile()
        {
            CreateMap<StatementModel.DiligenceModel, DueDiligencePageViewModel.DueDiligenceViewModel>().ReverseMap();

            CreateMap<StatementModel, DueDiligencePageViewModel>();

            CreateMap<DueDiligencePageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.DueDiligences, opt => opt.MapFrom(s => s.DueDiligences.Where(r => r.Id > 0)))
                .ForSourceMember(s => s.DiligenceTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.DueDiligences, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SelectedRemediationTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.RemediationTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.OtherRemediation, opt => opt.DoNotValidate());
        }
    }

    [DependencyModelBinder]
    public class DueDiligencePageViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public DiligenceTypeIndex DiligenceTypes { get; }
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
            [MaxLength(256)]
            public string Details { get; set; }
        }

        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 2";

        public List<DueDiligenceViewModel> DueDiligences { get; set; } = new List<DueDiligenceViewModel>();

        public bool? HasForceLabour { get; set; }

        [MaxLength(512)]//We need at least one validation annotation otherwise Validate wont execute
        public string ForcedLabourDetails { get; set; }

        public bool? HasSlaveryInstance { get; set; }

        public bool? HasRemediation { get; set; }

        [MaxLength(512)]//We need at least one validation annotation otherwise Validate wont execute
        public string SlaveryInstanceDetails { get; set; }

        [IgnoreMap]
        public string[] RemediationTypes = new[]
        {
            "repayment of recruitment fees",
            "change in policy",
            "referring victims to government services",
            "supporting victims via NGOs",
            "supporting criminal justice against perpetrator",
            "other"
        };

        [IgnoreMap]
        public List<string> SelectedRemediationTypes { get; set; } = new List<string>();

        [IgnoreMap]
        [MaxLength(512)]
        public string OtherRemediation { get; set; }

        public string SlaveryInstanceRemediation
        {
            get
            {
                var selectedRemediationTypes = new List<string>(SelectedRemediationTypes.Where(s => !string.IsNullOrWhiteSpace(s)));

                if (selectedRemediationTypes.Contains("other"))
                {
                    selectedRemediationTypes.Remove("other");
                    selectedRemediationTypes.Add(OtherRemediation);
                }
                return selectedRemediationTypes.ToDelimitedString(Environment.NewLine);
            }
            set
            {
                var selectedRemediationTypes = new List<string>(value.SplitI(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s)));

                //Set the selected types
                SelectedRemediationTypes.Clear();
                for (int i = selectedRemediationTypes.Count - 1; i >= 0; i--)
                {
                    if (RemediationTypes.ContainsI(selectedRemediationTypes[i]))
                    {
                        SelectedRemediationTypes.Add(selectedRemediationTypes[i]);
                        selectedRemediationTypes.RemoveAt(i);
                    }
                }
                OtherRemediation = selectedRemediationTypes.ToDelimitedString(Environment.NewLine);
                if (!string.IsNullOrWhiteSpace(OtherRemediation)) SelectedRemediationTypes.Add("other");
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var otherId = DiligenceTypes.Single(x => x.Description.EqualsI("other type of social audit")).Id;
            var otherIndex = DiligenceTypes.FindIndex(r => r.Id == otherId);
            var otherDiligence = DueDiligences.FirstOrDefault(x => x.Id == otherId);
            if (otherDiligence != null && string.IsNullOrWhiteSpace(otherDiligence.Details))
                validationResults.AddValidationError(3600, $"DueDiligences[{otherIndex}].Details");

            if (HasForceLabour == true)
            {
                if (string.IsNullOrWhiteSpace(ForcedLabourDetails))
                    validationResults.AddValidationError(3600, nameof(ForcedLabourDetails));
            }
            else
                ForcedLabourDetails = null;


            if (HasSlaveryInstance == true)
            {
                if (string.IsNullOrWhiteSpace(SlaveryInstanceDetails))
                    validationResults.AddValidationError(3600, nameof(SlaveryInstanceDetails));

                if (HasRemediation == null)
                    validationResults.AddValidationError(3601, nameof(HasRemediation));

                if (HasRemediation == true)
                {
                    if (!SelectedRemediationTypes.Any())
                        validationResults.AddValidationError(3602, nameof(SelectedRemediationTypes));
                    else if (SelectedRemediationTypes.Contains("other") && string.IsNullOrWhiteSpace(OtherRemediation))
                        validationResults.AddValidationError(3603, nameof(OtherRemediation));
                }
            }
            else
            {
                SlaveryInstanceDetails = null;
                SelectedRemediationTypes.Clear();
                OtherRemediation = null;
            }

            //Remove all the empty due diligences
            DueDiligences.RemoveAll(r => r.Id == 0);

            return validationResults;
        }
        public bool IsComplete()
        {
            var otherSocialAudit = DiligenceTypes.Single(x => x.Description.EqualsI("other type of social audit"));

            return DueDiligences.Any()
                && HasForceLabour.HasValue
                && HasSlaveryInstance.HasValue
                && !DueDiligences.Any(d => d.Id == otherSocialAudit.Id && string.IsNullOrWhiteSpace(d.Details))
                && HasForceLabour == false || !string.IsNullOrWhiteSpace(ForcedLabourDetails)
                && HasSlaveryInstance == false || !string.IsNullOrWhiteSpace(SlaveryInstanceDetails)
                && HasSlaveryInstance == false || !string.IsNullOrWhiteSpace(SlaveryInstanceRemediation);
        }
    }
}
