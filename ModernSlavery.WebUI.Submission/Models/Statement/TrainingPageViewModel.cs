using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class TrainingPageViewModelMapperProfile : Profile
    {
        public TrainingPageViewModelMapperProfile()
        {
            CreateMap<TrainingPageViewModel, StatementModel>();
        }
    }

    public class TrainingPageViewModel : BaseViewModel
    {
        public IList<TrainingViewModel> Training { get; set; }

        public string OtherTraining { get; set; }

        public class TrainingViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
