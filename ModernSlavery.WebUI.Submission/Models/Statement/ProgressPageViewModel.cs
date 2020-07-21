using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class ProgressPageViewModelMapperProfile : Profile
    {
        public ProgressPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,ProgressPageViewModel>()
                .ForMember(d => d.StatementYears, opt => opt.MapFrom(s => Enums.GetEnumFromRange<StatementYears>((int)s.MinStatementYears, (int)s.MaxStatementYears)));

            CreateMap<ProgressPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.MinStatementYears, opt => opt.MapFrom(s => s.StatementYears.GetAttribute<RangeAttribute>().Minimum))
                .ForMember(d => d.MinStatementYears, opt => opt.MapFrom(s => s.StatementYears.GetAttribute<RangeAttribute>().Maximum));
        }
    }

    public class ProgressPageViewModel : BaseViewModel
    {
        public bool IncludesMeasuringProgress { get; set; }

        public string ProgressMeasures { get; set; }

        public string KeyAchievements { get; set; }

        public StatementYears StatementYears { get; set; }
    }
}
