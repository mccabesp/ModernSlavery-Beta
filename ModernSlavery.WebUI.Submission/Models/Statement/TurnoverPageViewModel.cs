using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TurnoverPageViewModelMapperProfile : Profile
    {
        public TurnoverPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, YourTurnoverPageViewModel>();

            CreateMap<YourTurnoverPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.Turnover, opt => opt.MapFrom(s=>s.Turnover))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class YourTurnoverPageViewModel : BaseViewModel
    {
        public YourTurnoverPageViewModel()
        {

        }

        public override string PageTitle => "What was your turnover or budget during the financial year the statement relates to?";

        public StatementTurnoverRanges? Turnover { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return Turnover !=null && Turnover != StatementTurnoverRanges.NotProvided;
        }
    }
}
