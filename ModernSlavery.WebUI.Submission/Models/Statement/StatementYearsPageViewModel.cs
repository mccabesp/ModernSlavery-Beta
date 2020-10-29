using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ModernSlavery.Core.Entities.Statement;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class StatementYearsPageViewModelMapperProfile : Profile
    {
        public StatementYearsPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, StatementYearsPageViewModel>();

            CreateMap<StatementYearsPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s=>s.StatementYears))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class StatementYearsPageViewModel : BaseViewModel
    {
        public override string PageTitle => "How many years has your organisation been producing modern slavery statements?";

        public StatementYearRanges? StatementYears { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return StatementYears!=null && StatementYears != StatementYearRanges.NotProvided;
        }
    }
}
