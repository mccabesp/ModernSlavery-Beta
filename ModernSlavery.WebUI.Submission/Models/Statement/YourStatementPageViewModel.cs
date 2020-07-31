using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class YourStatementPageViewModelMapperProfile : Profile
    {
        public YourStatementPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, YourStatementPageViewModel>()
                .ForMember(d => d.StatementStartDay, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Day))
                .ForMember(d => d.StatementStartMonth, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Month))
                .ForMember(d => d.StatementStartYear, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Year))
                .ForMember(d => d.StatementEndDay, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Day))
                .ForMember(d => d.StatementEndMonth, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Month))
                .ForMember(d => d.StatementEndYear, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Year))
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Year));

            CreateMap<YourStatementPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementStartDate, opt => { opt.MapFrom(s => s.StatementStartDate); })
                .ForMember(d => d.StatementEndDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.StatementEndDate); })
                .ForMember(d => d.ApprovedDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.ApprovedDate); })
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.StatementStartYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementStartMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementStartDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class YourStatementPageViewModel : BaseViewModel
    {
        public override string PageTitle => "Your modern slavery statement";

        [Url(ErrorMessage = "URL is not valid")]
        [MaxLength(256, ErrorMessage = "The web address (URL) cannot be longer than 256 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }

        private DateTime? ToDateTime(int? year, int? month, int? day)
        {
            if (year == null || month == null || day == null) return null;

            if (DateTime.TryParse($"{year}-{month}-{day}", out var result))
                return result;

            return null;
        }

        public DateTime? StatementStartDate => ToDateTime(StatementStartYear, StatementStartMonth, StatementStartDay);

        public int? StatementStartDay { get; set; }
        public int? StatementStartMonth { get; set; }
        public int? StatementStartYear { get; set; }

        public DateTime? StatementEndDate => ToDateTime(StatementEndYear, StatementEndMonth, StatementEndDay);

        public int? StatementEndDay { get; set; }
        public int? StatementEndMonth { get; set; }
        public int? StatementEndYear { get; set; }

        [Display(Name = "Job title")]
        public string ApproverJobTitle { get; set; }
        [Display(Name = "First name")]
        public string ApproverFirstName { get; set; }
        [Display(Name = "Last name")]
        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate => ToDateTime(ApprovedYear, ApprovedMonth, ApprovedDay);

        public int? ApprovedDay { get; set; }
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //Validate the start date parts
            var partsComplete = !Text.IsAnyNull(StatementStartDay, StatementStartMonth, StatementStartYear);
            var partsEmpty = Text.IsAllNull(StatementStartDay, StatementStartMonth, StatementStartYear);
            if (!partsComplete && !partsEmpty)
            {
                if (StatementStartDay == null)
                    validationResults.AddValidationError(3101, nameof(StatementStartDay));

                if (StatementStartMonth == null)
                    validationResults.AddValidationError(3102, nameof(StatementStartMonth));

                if (StatementStartYear == null)
                    validationResults.AddValidationError(3103, nameof(StatementStartYear));
            }
            if (partsComplete && StatementStartDate == null)
                validationResults.AddValidationError(3104, nameof(StatementStartDate));

            //Validate the end date parts
            partsComplete = !Text.IsAnyNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            partsEmpty = Text.IsAllNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            if (!partsComplete && !partsEmpty)
            {
                if (StatementEndDay == null)
                    validationResults.AddValidationError(3105, nameof(StatementEndDay));

                if (StatementEndMonth == null)
                    validationResults.AddValidationError(3106, nameof(StatementEndMonth));

                if (StatementEndYear == null)
                    validationResults.AddValidationError(3107, nameof(StatementEndYear));
            }
            if (partsComplete && StatementEndDate == null)
                validationResults.AddValidationError(3108, nameof(StatementEndDate));

            //Validate the approved date parts
            partsComplete = !Text.IsAnyNull(ApprovedDay, ApprovedMonth, ApprovedYear);
            partsEmpty = Text.IsAllNull(ApprovedDay, ApprovedMonth, ApprovedYear);
            if (!partsComplete && !partsEmpty)
            {
                if (ApprovedDay == null)
                    validationResults.AddValidationError(3109, nameof(ApprovedDay));

                if (ApprovedMonth == null)
                    validationResults.AddValidationError(3110, nameof(ApprovedMonth));

                if (ApprovedYear == null)
                    validationResults.AddValidationError(3111, nameof(ApprovedYear));
            }
            if (partsComplete && ApprovedDate == null)
                validationResults.AddValidationError(3112, nameof(ApprovedDate));

            //Validate the approver parts
            partsComplete = !Text.IsAnyNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);
            partsEmpty = Text.IsAllNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);

            if (!partsComplete && !partsEmpty)
            {
                if (string.IsNullOrWhiteSpace(ApproverFirstName))
                    validationResults.AddValidationError(3113, nameof(ApproverFirstName));

                if (string.IsNullOrWhiteSpace(ApproverLastName))
                    validationResults.AddValidationError(3114, nameof(ApproverLastName));

                if (string.IsNullOrWhiteSpace(ApproverJobTitle))
                    validationResults.AddValidationError(3115, nameof(ApproverJobTitle));
            }

            return validationResults;
        }

        public override bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(StatementUrl)
                && StatementStartDay.HasValue
                && StatementStartMonth.HasValue
                && StatementStartYear.HasValue
                && StatementEndDay.HasValue
                && StatementEndMonth.HasValue
                && StatementEndYear.HasValue
                && !string.IsNullOrWhiteSpace(ApproverFirstName)
                && !string.IsNullOrWhiteSpace(ApproverLastName)
                && !string.IsNullOrWhiteSpace(ApproverJobTitle)
                && ApprovedDay.HasValue
                && ApprovedMonth.HasValue
                && ApprovedYear.HasValue;
        }
    }
}