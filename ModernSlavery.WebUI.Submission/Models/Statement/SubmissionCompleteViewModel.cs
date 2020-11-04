using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SubmissionCompleteViewModelMapperProfile : Profile
    {
        public SubmissionCompleteViewModelMapperProfile()
        {
            CreateMap<StatementModel, SubmissionCompleteViewModel>();
        }
    }

    public class SubmissionCompleteViewModel : BaseViewModel
    {
        public override string PageTitle => "Submission complete";
    }
}
