using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static ModernSlavery.Core.Entities.Statement;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class YearsPageViewModelMapperProfile : Profile
    {
        public YearsPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, YearsViewModel>();

            CreateMap<YearsViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s=>s.StatementYears))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class YearsViewModel : BaseViewModel
    {
        public override string PageTitle => "How many years has your organisation been producing modern slavery statements?";

        public StatementYearRanges? StatementYears { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (StatementYears != null && StatementYears != StatementYearRanges.NotProvided) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
