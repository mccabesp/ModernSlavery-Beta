using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class CancelPageViewModelMapperProfile : Profile
    {
        public CancelPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,CancelPageViewModel>();
            CreateMap<CancelPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class CancelPageViewModel : BaseViewModel
    {
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
