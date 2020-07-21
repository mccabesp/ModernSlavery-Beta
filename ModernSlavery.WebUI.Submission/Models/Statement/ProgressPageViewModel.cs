using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressPageViewModelMapperProfile : Profile
    {
        public ProgressPageViewModelMapperProfile()
        {
            CreateMap<ProgressPageViewModel, StatementModel>();
        }
    }

    public class ProgressPageViewModel : BaseViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public bool IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public Presenters.NumberOfYearsOfStatements? NumberOfYearsOfStatements { get; set; }
    }
}
