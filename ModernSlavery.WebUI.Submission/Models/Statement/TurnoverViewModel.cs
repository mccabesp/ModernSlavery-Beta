using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TurnoverViewModelMapperProfile : Profile
    {
        public TurnoverViewModelMapperProfile()
        {
            CreateMap<StatementModel, TurnoverViewModel>();

            CreateMap<TurnoverViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.Turnover, opt => opt.MapFrom(s=>s.Turnover))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class TurnoverViewModel : BaseViewModel
    {
        public override string PageTitle => "What was your turnover or budget during the financial year the statement relates to?";

        public StatementTurnoverRanges? Turnover { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Turnover != null && Turnover != StatementTurnoverRanges.NotProvided) return Status.Complete;

            return Status.Incomplete;
        }
    }
}
