using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressPageViewModelMapperProfile : Profile
    {
        public ProgressPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, MonitoringProgressPageViewModel>();

            CreateMap<MonitoringProgressPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class MonitoringProgressPageViewModel : BaseViewModel
    {
        #region Types
        public enum YearRanges : byte
        {
            NotProvided = 0,

            [GovUkRadioCheckboxLabelText(Text = "This is the first time")]
            Year1 = 1,

            [GovUkRadioCheckboxLabelText(Text = "1 to 5 Years")]
            Years1To5 = 2,

            [GovUkRadioCheckboxLabelText(Text = "More than 5 years")]
            Over5Years = 3,
        }
        #endregion

        public override string PageTitle => "Monitoring progress";

        public bool? IncludesMeasuringProgress { get; set; }
        [MaxLength(1024)]
        public string ProgressMeasures { get; set; }
        [MaxLength(1024)]
        public string KeyAchievements { get; set; }

        public YearRanges? StatementYears { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return IncludesMeasuringProgress != null
                && !string.IsNullOrWhiteSpace(ProgressMeasures)
                && !string.IsNullOrWhiteSpace(KeyAchievements)
                && StatementYears != YearRanges.NotProvided;
        }
    }
}
