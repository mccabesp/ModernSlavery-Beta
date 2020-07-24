using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingPageViewModelMapperProfile : Profile
    {
        public TrainingPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, TrainingPageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<TrainingPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
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
        public override string PageTitle => "Training";

        public IList<short> Training { get; set; }

        [MaxLength(50)]
        public string OtherTraining { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Get the training types
            var trainingTypes = validationContext.GetService<TrainingTypeIndex>();

            var otherId = trainingTypes.Single(x => x.Description.Equals("Other")).Id;
            if (Training.Contains(otherId) && string.IsNullOrWhiteSpace(OtherTraining))
                yield return new ValidationResult("Please provide other details");
        }

        public override bool IsComplete()
        {
            return base.IsComplete();
        }
    }
}
