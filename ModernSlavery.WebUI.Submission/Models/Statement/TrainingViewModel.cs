using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingViewModelMapperProfile : Profile
    {
        public TrainingViewModelMapperProfile()
        {
            CreateMap<StatementModel, TrainingViewModel>()
                .ForMember(d => d.TrainingTargets, opt => opt.MapFrom(s => s.Summary.TrainingTargets))
                .ForMember(d => d.OtherTrainingTargets, opt => opt.MapFrom(s => s.Summary.OtherTrainingTargets));

            CreateMap<TrainingViewModel, StatementModel>(MemberList.None)
                .ForPath(d => d.Summary.TrainingTargets, opt => opt.MapFrom(s=>s.TrainingTargets))
                .ForPath(d => d.Summary.OtherTrainingTargets, opt => opt.MapFrom(s=>s.OtherTrainingTargets));
        }
    }

    public class TrainingViewModel : BaseViewModel
    {
        public override string PageTitle => "During the period of the statement, did you provide training on modern slavery, or any other activities to raise awareness?";

        public List<TrainingTargetTypes> TrainingTargets { get; set; } = new List<TrainingTargetTypes>();

        [MaxLength(256)]
        public string OtherTrainingTargets { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (TrainingTargets.Contains(TrainingTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTrainingTargets))
                validationResults.AddValidationError(3700, nameof(OtherTrainingTargets));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (TrainingTargets.Any())
            {
                if (TrainingTargets.Contains(TrainingTargetTypes.Other) && string.IsNullOrWhiteSpace(OtherTrainingTargets)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }
    }
}
