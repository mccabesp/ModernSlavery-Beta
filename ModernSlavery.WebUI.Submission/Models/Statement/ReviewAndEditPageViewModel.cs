using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using System;
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
                .ForMember(s => s.YourStatement, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Compliance, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Organisation, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Policies, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Risks, opt => opt.MapFrom(s => s))
                .ForMember(s => s.DueDiligence, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Training, opt => opt.MapFrom(s => s))
                .ForMember(s => s.Progress, opt => opt.MapFrom(s => s))
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());
        }
    }

    public class ReviewAndEditPageViewModel : BaseViewModel
    {
        public override string PageTitle => $"Review {ReportingDeadlineYear - 1} to {ReportingDeadlineYear} group report for {OrganisationName}";
        public override string SubTitle => GetSubtitle();

        private string GetSubtitle()
        {
            var complete = YourStatement.IsComplete() && Compliance.IsComplete();

            var result = $"Submission {(complete ? "" : "in")}complete.";

            if (complete)
                return result;

            result += " Section 1 must be completed in order to submit.";

            return result;
        }

        public YourStatementPageViewModel YourStatement { get; set; }
        public CompliancePageViewModel Compliance { get; set; }
        public YourOrganisationPageViewModel Organisation { get; set; }
        public PoliciesPageViewModel Policies { get; set; }
        public SupplyChainRisksPageViewModel Risks { get; set; }
        public DueDiligencePageViewModel DueDiligence { get; set; }
        public TrainingPageViewModel Training { get; set; }
        public MonitoringProgressPageViewModel Progress { get; set; }
        
        [IgnoreMap]
        public IList<AutoMap.Diff> Modifications { get; set; }

        [IgnoreMap]
        [BindNever]
        public string YourStatementUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string ComplianceUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string OrganisationUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string PoliciesUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string RisksUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string DueDiligenceUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string TrainingUrl { get; set; }
        [IgnoreMap]
        [BindNever]
        public string ProgressUrl { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (YourStatement.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{YourStatement.PageTitle}' is invalid");

            if (Compliance.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Compliance.PageTitle}' is invalid");

            if (Organisation.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Organisation.PageTitle}' is invalid");

            if (Policies.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Policies.PageTitle}' is invalid");

            if (Risks.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Risks.PageTitle}{(string.IsNullOrWhiteSpace(Risks.PageTitle) ? "" : $" ({Risks.SubTitle})")}' is invalid");

            if (DueDiligence.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{DueDiligence.PageTitle}{(string.IsNullOrWhiteSpace(DueDiligence.PageTitle) ? "" : $" ({DueDiligence.SubTitle})")}' is invalid");

            if (Training.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Training.PageTitle}' is invalid");

            if (Progress.Validate(validationContext).Any())
                yield return new ValidationResult($"Section '{Progress.PageTitle}' is invalid");

            var serviceProvider = validationContext.GetService<IServiceProvider>();
            if (!IsComplete())
                yield return new ValidationResult("You must first complete all sections before you can submit");
        }

        public override bool IsComplete()
        {
            return YourStatement.IsComplete() 
                && Compliance.IsComplete() 
                && Organisation.IsComplete() 
                && Policies.IsComplete() 
                && Risks.IsComplete() 
                && DueDiligence.IsComplete() 
                && Training.IsComplete() 
                && Progress.IsComplete();
        }

        public bool HasChanged()
        {
            return Modifications!=null && Modifications.Any();
        }
    }
}
