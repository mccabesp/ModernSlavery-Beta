using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System;
using System.Text.Json.Serialization;

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

    public class TrainingPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        public TrainingTypeIndex TrainingTypes { get; set; }
        public TrainingPageViewModel(TrainingTypeIndex trainingTypes)
        {
            TrainingTypes = trainingTypes;
        }

        public TrainingPageViewModel()
        {

        }
        public override string PageTitle => "Training";

        public List<short> Training { get; set; } = new List<short>();

        [MaxLength(1024)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherTraining { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Get the training types
            TrainingTypes = validationContext.GetService<TrainingTypeIndex>();

            var otherId = TrainingTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Training.Contains(otherId) && string.IsNullOrWhiteSpace(OtherTraining))
                yield return new ValidationResult("Please provide other details");
        }

        public override bool IsComplete()
        {
            var other = TrainingTypes.Single(x => x.Description.Equals("Other"));

            return Training.Any() 
                && !Training.Any(t=>t==other.Id && string.IsNullOrWhiteSpace(OtherTraining));
        }
    }
}
