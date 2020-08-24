using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;
using ModernSlavery.Core.Classes.StatementTypeIndexes;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingPageViewModelMapperProfile : Profile
    {
        public TrainingPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, TrainingPageViewModel>();

            CreateMap<TrainingPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.TrainingTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    [DependencyModelBinder]
    public class TrainingPageViewModel : BaseViewModel
    {

        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public TrainingTypeIndex TrainingTypes { get; }
        public TrainingPageViewModel(TrainingTypeIndex trainingTypes)
        {
            TrainingTypes = trainingTypes;
        }

        public TrainingPageViewModel()
        {

        }
        public override string PageTitle => "Training";

        public List<short> Training { get; set; } = new List<short>();

        [MaxLength(256)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherTraining { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var otherId = TrainingTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Training.Contains(otherId) && string.IsNullOrWhiteSpace(OtherTraining))
                validationResults.AddValidationError(3700, nameof(OtherTraining));

            return validationResults;
        }

        public override bool IsComplete()
        {
            var other = TrainingTypes.Single(x => x.Description.Equals("Other"));

            return Training.Any()
                && !Training.Any(t => t == other.Id && string.IsNullOrWhiteSpace(OtherTraining));
        }
    }
}
