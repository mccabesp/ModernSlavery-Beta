﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class DueDiligencePageViewModelMapperProfile : Profile
    {
        public DueDiligencePageViewModelMapperProfile()
        {
            CreateMap<StatementModel,DueDiligencePageViewModel>();
            CreateMap<DueDiligencePageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class DueDiligencePageViewModel: BaseViewModel
    {
        public List<DueDiligenceViewModel> DueDiligences { get; set; }

        public bool HasForceLabour { get; set; }
        public string ForcedLabourDetails { get; set; }

        public bool HasSlaveryInstance { get; set; }
        public string SlaveryInstanceDetails { get; set; }

        public IList<Presenters.StatementRemediation> SlaveryInstanceRemediation { get; set; }

        public class DueDiligenceViewModel
        {
            // TODO - James Handle "Other" case
            // It seems to only appear once under "Social audits"
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
