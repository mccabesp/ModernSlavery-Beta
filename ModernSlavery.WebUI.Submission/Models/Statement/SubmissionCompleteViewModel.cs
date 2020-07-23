using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SubmissionCompleteViewModelMapperProfile : Profile
    {
        public SubmissionCompleteViewModelMapperProfile()
        {
            CreateMap<StatementModel, SubmissionCompleteViewModel>();
            CreateMap<SubmissionCompleteViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class SubmissionCompleteViewModel : BaseViewModel
    {
        public int Year { get; set; }

        public override string PageTitle => "Submission Complete";

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return null;
        }
    }
}
