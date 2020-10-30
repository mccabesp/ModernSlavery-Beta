﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ReviewViewModelMapperProfile : Profile
    {
        public ReviewViewModelMapperProfile()
        {
            CreateMap<StatementModel, ReviewViewModel>()
                .ForMember(s => s.GroupOrganisationsPages, opt => opt.MapFrom(s => s))
                .ForMember(s => s.UrlPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.PeriodCoveredPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.SignOffPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.CompliancePage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.SectorsPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.YearsPage, opt => opt.MapFrom(s => s))
                .ForMember(s => s.PoliciesPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.TrainingPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.PartnersPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.SocialAuditsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.GrievancesPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.MonitoringPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.HighestRisksPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.HighRiskPages, opt => opt.MapFrom(s => s.Summary.Risks))
                .ForMember(s => s.IndicatorsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.RemediationsPage, opt => opt.MapFrom(s => s.Summary))
                .ForMember(s => s.ProgressPage, opt => opt.MapFrom(s => s.Summary))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class ReviewViewModel : BaseViewModel
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
        public GroupOrganisationsViewModel GroupOrganisationsPages { get; set; }
        public UrlEmailViewModel UrlPage { get; set; }
        public PeriodCoveredViewModel PeriodCoveredPage { get; set; }
        public SignOffViewModel SignOffPage { get; set; }
        public ComplianceViewModel CompliancePage { get; set; }
        public SectorsViewModel SectorsPage { get; set; }
        public TurnoverViewModel TurnoverPage { get; set; }
        public YearsViewModel YearsPage { get; set; }
        public PoliciesViewModel PoliciesPage { get; set; }
        public TrainingViewModel TrainingPage { get; set; }
        public PartnersViewModel PartnersPage { get; set; }
        public SocialAuditsViewModel SocialAuditsPage { get; set; }
        public GrievancesViewModel GrievancesPage { get; set; }
        public MonitoringViewModel MonitoringPage { get; set; }
        public HighestRisksViewModel HighestRisksPage { get; set; }
        public List<HighRiskViewModel> HighRiskPages { get; set; } = new List<HighRiskViewModel>();
        public IndicatorsViewModel IndicatorsPage { get; set; }
        public RemediationsViewModel RemediationsPage { get; set; }
        public ProgressViewModel ProgressPage { get; set; }
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
            if (GroupOrganisationsPages.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{GroupOrganisationsPages.PageTitle}' is invalid");

            if (UrlPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{UrlPage.PageTitle}' is invalid");

            if (PeriodCoveredPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{PeriodCoveredPage.PageTitle}' is invalid");

            if (SignOffPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{SignOffPage.PageTitle}' is invalid");

            if (CompliancePage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{CompliancePage.PageTitle}' is invalid");

            if (SectorsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{SectorsPage.PageTitle}' is invalid");

            if (YearsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{YearsPage.PageTitle}' is invalid");

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

            if (MonitoringPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{MonitoringPage.PageTitle}' is invalid");

            if (HighestRisksPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{HighestRisksPage.PageTitle}' is invalid");

            for (var pageIndex=0; pageIndex<HighRiskPages.Count; pageIndex++)
                if (HighRiskPages[pageIndex].Validate(validationContext).Any())
                    yield return new ValidationResult($"Section '{HighRiskPages[pageIndex].PageTitle} {pageIndex+1}' is invalid");

            if (IndicatorsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{IndicatorsPage.PageTitle}' is invalid");

            if (RemediationsPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{RemediationsPage.PageTitle}' is invalid");

            if (ProgressPage.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{ProgressPage.PageTitle}' is invalid");

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
            if (GroupOrganisationsPages.GetStatus()==Status.Complete) count++;
            //2
            if (GetUrlSignOffStatus() == Status.Complete) count++;
            //3
            if (CompliancePage.GetStatus() == Status.Complete) count++;
            //4
            if (GetSectorTurnoverStatus() == Status.Complete) count++;
            //5
            if (YearsPage.GetStatus() == Status.Complete) count++;
            //6
            if (PoliciesPage.GetStatus() == Status.Complete) count++;
            //7
            if (TrainingPage.GetStatus() == Status.Complete) count++;
            //8
            if (GetWorkingConditionsStatus() == Status.Complete) count++;
            //9
            if (HighestRisksPage.GetStatus() == Status.Complete && HighRiskPages.All(v => v.GetStatus() == Status.Complete)) count++;
            //10
            if (IndicatorsPage.GetStatus() == Status.Complete && RemediationsPage.GetStatus() == Status.Complete) count++;
            //11
            if (ProgressPage.GetStatus() == Status.Complete) count++;
            return count;
        }

        public Status GetUrlSignOffStatus()
        {
            if (UrlPage.GetStatus() == Status.Complete && PeriodCoveredPage.GetStatus() == Status.Complete && SignOffPage.GetStatus()== Status.Complete)return Status.Complete;
            return Status.Incomplete;
        }

        public Status GetSectorTurnoverStatus()
        {
            if (SectorsPage.GetStatus() == Status.Complete && TurnoverPage.GetStatus() == Status.Complete) return Status.Complete;
            return Status.Incomplete;
        }

        public Status GetWorkingConditionsStatus()
        {
            if (PartnersPage.GetStatus() == Status.Complete && SocialAuditsPage.GetStatus() == Status.Complete && GrievancesPage.GetStatus() == Status.Complete && MonitoringPage.IsComplete()) return Status.Complete;
            if (PartnersPage.GetStatus() == Status.Incomplete && SocialAuditsPage.GetStatus() == Status.Incomplete && GrievancesPage.GetStatus() == Status.Incomplete && MonitoringPage.GetStatus()==Status.Incomplete) return Status.Incomplete;
            return Status.InProgress;

        }

        public bool BasicsComplete()
        {
            return GroupOrganisationsPages.GetStatus() == Status.Complete 
                && GetUrlSignOffStatus() == Status.Complete 
                && CompliancePage.GetStatus() == Status.Complete
                && GetSectorTurnoverStatus() == Status.Complete
                && YearsPage.GetStatus() == Status.Complete;
        }

        public bool CanSubmit()
        {
            return BasicsComplete() && (!Submitted || HasSubmissionChanged());
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
