using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingViewModelMapperProfile : Profile
    {
        public TrainingViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, TrainingViewModel>();

            CreateMap<TrainingViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.TrainingTargets, opt => opt.MapFrom(s=>s.TrainingTargets))
                .ForMember(d => d.OtherTrainingTargets, opt => opt.MapFrom(s=>s.OtherTraining))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class TrainingViewModel : BaseViewModel
    {
        public override string PageTitle => "During the period of the statement, did you provide training on modern slavery, or any other activities to raise awareness?";

        public List<TrainingTargetTypes> TrainingTargets { get; set; } = new List<TrainingTargetTypes>();

        [MaxLength(256)]
        public string OtherTraining { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (TrainingTargets.Contains(TrainingTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTraining))
                validationResults.AddValidationError(3700, nameof(OtherTraining));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (TrainingTargets.Any())
            {
                if (TrainingTargets.Contains(TrainingTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTraining)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
