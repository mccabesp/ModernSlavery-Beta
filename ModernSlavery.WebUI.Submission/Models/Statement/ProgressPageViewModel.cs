using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressPageViewModelMapperProfile : Profile
    {
        public ProgressPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, ProgressPageViewModel>();
            CreateMap<ProgressPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class ProgressPageViewModel : BaseViewModel
    {
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

        [Display(Name = "Does you modern slavery statement include goals relating to how you will prevent modern slavery in your operations and supply chains?")] 
        public bool IncludesMeasuringProgress { get; set; }

        [Display(Name = "How is your organisation measuring progress towards these goals?")]
        public string ProgressMeasures { get; set; }
        [Display(Name = "What were your key achievements in relation to reducing modern slavery during the period covered by this statement?")]
        public string KeyAchievements { get; set; }

        [Display(Name = "How many years has your organisation been producing modern slavery statements?")]
        public YearRanges StatementYears { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new System.NotImplementedException();
        }

    }
}
