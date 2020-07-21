using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class OrganisationPageViewModelMapperProfile : Profile
    {
        public OrganisationPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,OrganisationPageViewModel>();
            CreateMap<OrganisationPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class OrganisationPageViewModel : BaseViewModel
    {
        public IList<SectorViewModel> Sectors { get; set; }

        public Presenters.LastFinancialYearBudget? Turnover { get; set; }

        public class SectorViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
