using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ReviewPageViewModelMapperProfile : Profile
    {
        public ReviewPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, ReviewAndEditPageViewModel>()
                .ForMember(s => s.GroupOrganisations, opt => opt.MapFrom(s => s))
                .ForMember(s => s.UrlPageViewModel, opt => opt.MapFrom(s => s))
                .ForMember(s => s.StatementPeriodPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.ApprovalPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.CompliancePage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.SectorsPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.StatementYearsPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.PoliciesPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.TrainingPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.PartnersPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.SocialAuditsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.GrievancesPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.OtherWorkConditionsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.RiskDescriptionsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.RiskDetailsPages, opt => opt.MapFrom(s => s.Summary.Risks))
                .ForMember(s => s.IndicatorsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.RemediationPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.MonitoringProgressPage, opt => opt.MapFrom(s => s.Summary))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class ReviewAndEditPageViewModel : BaseViewModel
    {
        public override string PageTitle => $"Review before submitting";
        public override string SubTitle => GetSubtitle();

        private string GetSubtitle()
        {
            var complete = IsComplete();

            var result = $"Submission {(complete ? "" : "in")}complete.";

            if (complete)
                return result;

            result += " Section 1 must be completed in order to submit.";

            return result;
        }

        #region Individual Page ViewModels
        public GroupOrganisationsViewModel GroupOrganisations { get; set; }
        public UrlPageViewModel UrlPageViewModel { get; set; }
        public StatementPeriodPageViewModel StatementPeriodPage { get; set; }
        public ApprovalPageViewModel ApprovalPage { get; set; }
        public CompliancePageViewModel CompliancePage { get; set; }
        public SectorPageViewModel SectorsPage { get; set; }
        public StatementYearsPageViewModel StatementYearsPage { get; set; }
        public PoliciesPageViewModel PoliciesPage { get; set; }
        public TrainingPageViewModel TrainingPage { get; set; }
        public PartnersPageViewModel PartnersPage { get; set; }
        public SocialAuditsPageViewModel SocialAuditsPage { get; set; }
        public GrievancesPageViewModel GrievancesPage { get; set; }
        public MonitoringOtherWorkConditionsPageViewModel OtherWorkConditionsPage { get; set; }
        public RiskDescriptionsPageViewModel RiskDescriptionsPage { get; set; }
        public List<RiskDetailsPageViewModel> RiskDetailsPages { get; set; } = new List<RiskDetailsPageViewModel>();
        public IndicatorsPageViewModel IndicatorsPage { get; set; }
        public RemediationPageViewModel RemediationPage { get; set; }
        public MonitoringProgressPageViewModel MonitoringProgressPage { get; set; }
        #endregion

        [IgnoreMap]
        public IList<AutoMap.Diff> DraftModifications { get; set; }

        [IgnoreMap]
        public IList<AutoMap.Diff> SubmittedModifications { get; set; }

        #region Navigation Url
        [IgnoreMap]
        [BindNever]
        public string GroupReportingUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string UrlApprovalUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string ComplianceUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string SectorTurnoverUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string StatementYearsUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string PoliciesUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string TrainingUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string WorkingConditionsUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string RisksUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string IndicatorsUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string MonitoringProgressUrl { get; set; }
        #endregion

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (GroupOrganisations.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{GroupOrganisations.PageTitle}' is invalid");

            if (UrlPageViewModel.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{UrlPageViewModel.PageTitle}' is invalid");

            if (StatementPeriodPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{StatementPeriodPage.PageTitle}' is invalid");

            if (ApprovalPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{ApprovalPage.PageTitle}' is invalid");

            if (CompliancePage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{CompliancePage.PageTitle}' is invalid");

            if (SectorsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{SectorsPage.PageTitle}' is invalid");

            if (StatementYearsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{StatementYearsPage.PageTitle}' is invalid");

            if (PoliciesPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{PoliciesPage.PageTitle}' is invalid");

            if (TrainingPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{TrainingPage.PageTitle}' is invalid");

            if (PartnersPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{PartnersPage.PageTitle}' is invalid");

            if (SocialAuditsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{SocialAuditsPage.PageTitle}' is invalid");

            if (GrievancesPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{GrievancesPage.PageTitle}' is invalid");

            if (OtherWorkConditionsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{OtherWorkConditionsPage.PageTitle}' is invalid");

            if (RiskDescriptionsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{RiskDescriptionsPage.PageTitle}' is invalid");

            for (var pageIndex=0; pageIndex<RiskDetailsPages.Count; pageIndex++)
                if (RiskDetailsPages[pageIndex].Validate(validationContext).Any())
                    yield return new ValidationResult($"Section '{RiskDetailsPages[pageIndex].PageTitle} {pageIndex+1}' is invalid");

            if (IndicatorsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{IndicatorsPage.PageTitle}' is invalid");

            if (RemediationPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{RemediationPage.PageTitle}' is invalid");

            if (MonitoringProgressPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{MonitoringProgressPage.PageTitle}' is invalid");

            if (!IsComplete())
                yield return new ValidationResult("You must first complete all sections before you can submit");

            else if (Submitted && !HasChanged())
                yield return new ValidationResult("You must edit the statement before you can submit");
        }

        public const int SectionCount = 11;
        public int CompleteCount()
        {
            int count = 0;
            //1
            if (GroupOrganisations.IsComplete()) count++;
            //2
            if (UrlPageViewModel.IsComplete() && StatementPeriodPage.IsComplete() && ApprovalPage.IsComplete()) count++;
            //3
            if (CompliancePage.IsComplete()) count++;
            //4
            if (SectorsPage.IsComplete() && StatementYearsPage.IsComplete()) count++;
            //5
            if (StatementYearsPage.IsComplete()) count++;
            //6
            if (PoliciesPage.IsComplete()) count++;
            //7
            if (TrainingPage.IsComplete()) count++;
            //8
            if (PartnersPage.IsComplete() && SocialAuditsPage.IsComplete() && GrievancesPage.IsComplete() && OtherWorkConditionsPage.IsComplete()) count++;
            //9
            if (RiskDescriptionsPage.IsComplete() && RiskDetailsPages.All(v => v.IsComplete())) count++;
            //10
            if (IndicatorsPage.IsComplete() && RemediationPage.IsComplete()) count++;
            //11
            if (MonitoringProgressPage.IsComplete())count++;
            return count;
        }

        public override bool IsComplete()
        {
            return CompleteCount()==SectionCount;
        }

        public bool CanSubmit()
        {
            return IsComplete() && (!Submitted || HasSubmissionChanged());
        }

        public bool HasChanged()
        {
            return (!Submitted && HasDraftChanged()) || (Submitted && HasSubmissionChanged());
        }

        public bool HasDraftChanged()
        {
            return DraftModifications != null && DraftModifications.Any();
        }

        public bool HasSubmissionChanged()
        {
            return  SubmittedModifications != null && SubmittedModifications.Any();
        }
    }
}
