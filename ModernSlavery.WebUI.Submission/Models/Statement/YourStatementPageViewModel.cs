using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
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
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Year))
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

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
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }

        private DateTime? ToDateTime(int? year, int? month, int? day)
        {
            if (year == null || month == null || day == null) return null;
            return new DateTime(year.Value, month.Value, day.Value);
        }

        public DateTime? StatementStartDate => ToDateTime(StatementStartYear, StatementStartMonth, StatementStartDay);

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? StatementStartDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? StatementStartMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? StatementStartYear { get; set; }

        public DateTime? StatementEndDate => ToDateTime(StatementEndYear, StatementEndMonth, StatementEndDay);

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? StatementEndDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? StatementEndMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? StatementEndYear { get; set; }

        [Display(Name = "Job Title")]
        public string ApproverJobTitle { get; set; }
        [Display(Name = "First Name")]
        public string ApproverFirstName { get; set; }
        [Display(Name = "Last Name")]
        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate => ToDateTime(ApprovedYear, ApprovedMonth, ApprovedDay);

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? ApprovedDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? ApprovedMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? ApprovedYear { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var startDateList = new List<int?> { StatementStartDay, StatementStartMonth, StatementStartYear };
            if (startDateList.Any(x => x.HasValue) && startDateList.Any(x => !x.HasValue))
                yield return new ValidationResult("Please complete the Start Date");

            var endDateList = new List<int?> { StatementEndDay, StatementEndMonth, StatementEndYear };
            if (endDateList.Any(x => x.HasValue) && !endDateList.Any(x => !x.HasValue))
                yield return new ValidationResult("Please complete the End Date");

            var approvalDateList = new List<int?> { ApprovedDay, ApprovedMonth, ApprovedYear };
            if (approvalDateList.Any(x => x.HasValue) && !approvalDateList.Any(x => !x.HasValue))
                yield return new ValidationResult("Please complete the Approved Date");

            var detailsList = new List<string> { ApproverFirstName, ApproverLastName, ApproverJobTitle };
            if (detailsList.Any(x => string.IsNullOrWhiteSpace(x)) && !detailsList.Any(x => string.IsNullOrWhiteSpace(x)))
                yield return new ValidationResult("Please complete First name, Last name, Job title");
        }

        public override bool IsComplete(IServiceProvider serviceProvider)
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