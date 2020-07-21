using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ReviewPageViewModelMapperProfile : Profile
    {
        public ReviewPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,ReviewPageViewModel>();
            CreateMap<ReviewPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class ReviewPageViewModel : BaseViewModel
    {
    }
}
