using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
