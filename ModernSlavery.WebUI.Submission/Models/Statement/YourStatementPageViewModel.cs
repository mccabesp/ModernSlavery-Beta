using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
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
                .ForMember(d => d.StatementStartDay, opt => opt.MapFrom(s => s.StatementStartDate.Value.Day))
                .ForMember(d => d.StatementStartMonth, opt => opt.MapFrom(s => s.StatementStartDate.Value.Month))
                .ForMember(d => d.StatementStartYear, opt => opt.MapFrom(s => s.StatementStartDate.Value.Year))
                .ForMember(d => d.StatementEndDay, opt => opt.MapFrom(s => s.StatementEndDate.Value.Day))
                .ForMember(d => d.StatementEndMonth, opt => opt.MapFrom(s => s.StatementEndDate.Value.Month))
                .ForMember(d => d.StatementEndYear, opt => opt.MapFrom(s => s.StatementEndDate.Value.Year))
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate.Value.Year));

            CreateMap<YourStatementPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementStartDate, opt => opt.MapFrom(s => new DateTime(s.StatementStartYear.Value, s.StatementStartMonth.Value, s.StatementStartDay.Value)))
                .ForMember(d => d.StatementEndDate, opt => opt.MapFrom(s => new DateTime(s.StatementEndYear.Value, s.StatementEndMonth.Value, s.StatementEndDay.Value)))
                .ForMember(d => d.ApprovedDate, opt => opt.MapFrom(s => new DateTime(s.ApprovedYear.Value, s.ApprovedMonth.Value, s.ApprovedDay.Value)));
        }
    }

    public class YourStatementPageViewModel : BaseViewModel
    {
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a URL")] //this will only get triggeres through parseAndValidate
        [Url(ErrorMessage = "URL is not valid")]
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? StatementStartDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? StatementStartMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? StatementStartYear { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? StatementEndDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? StatementEndMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? StatementEndYear { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a job title")]
        public string ApproverJobTitle { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a first name")]
        public string ApproverFirstName { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a last name")]
        public string ApproverLastName { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Day format is incorrect")]
        public int? ApprovedDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Month format is incorrect")]
        public int? ApprovedMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Year format is incorrect")]
        public int? ApprovedYear { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var startDateList = new List<int?> { StatementStartDay, StatementStartMonth, StatementStartYear };
            if (startDateList.Any(x => x.HasValue) && !startDateList.Any(x => x.HasValue))
                yield return new ValidationResult("Please complete the Start Date");

            var endDateList = new List<int?> { StatementEndDay, StatementEndMonth, StatementEndYear };
            if (endDateList.Any(x => x.HasValue) && !endDateList.Any(x => x.HasValue))
                yield return new ValidationResult("Please complete the End Date");

            var approvalDateList = new List<int?> { ApprovedDay, ApprovedMonth, ApprovedYear };
            if (approvalDateList.Any(x => x.HasValue) && !approvalDateList.Any(x => x.HasValue))
                yield return new ValidationResult("Please complete the Approved Date");

            var detailsList = new List<string> { ApproverFirstName, ApproverLastName, ApproverJobTitle };
            if (detailsList.Any(x => x.IsNull()) && !detailsList.Any(x => x.IsNull()))
                yield return new ValidationResult("Please complete First name, Last name, Job title");
        }
    }
}