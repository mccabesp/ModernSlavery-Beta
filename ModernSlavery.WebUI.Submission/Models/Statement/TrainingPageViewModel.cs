using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingPageViewModelMapperProfile : Profile
    {
        public TrainingPageViewModelMapperProfile()
        {
            CreateMap<StatementModel.TrainingModel, TrainingPageViewModel.TrainingViewModel>().ReverseMap();

            CreateMap<StatementModel, TrainingPageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<TrainingPageViewModel, StatementModel>(MemberList.Source)
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
        #region Types
        public class TrainingViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
        #endregion

        public override string PageTitle => "Training";

        public IList<TrainingViewModel> Training { get; set; }

        [MaxLength(50)]
        public string OtherTraining { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Training.Single(x => x.Description.Equals("Other")).IsSelected && OtherTraining.IsNullOrWhiteSpace())
                yield return new ValidationResult("Please provide other details");
        }

        public override bool IsComplete()
        {
            return Training.Any(x => x.IsSelected)
                && !Training.Single(x => x.Description.Equals("Other")).IsSelected || !OtherTraining.IsNullOrWhiteSpace();
        }
    }
}
