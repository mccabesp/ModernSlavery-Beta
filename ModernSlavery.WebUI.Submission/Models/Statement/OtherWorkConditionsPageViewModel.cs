﻿using AutoMapper;
using ModernSlavery.Core.Entities.StatementSummary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class OtherWorkConditionsPageViewModelMapperProfile : Profile
    {
        public OtherWorkConditionsPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, MonitoringOtherWorkConditionsPageViewModel>();

            CreateMap<MonitoringOtherWorkConditionsPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.OtherWorkConditionsMonitoring, opt => opt.MapFrom(s=>s.OtherWorkConditionsMonitoring))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class MonitoringOtherWorkConditionsPageViewModel : BaseViewModel
    {
        public override string PageTitle => "Do you want to tell us about any other ways you monitored working conditions across your organisation and supply chain?";

        [MaxLength(1024)]
        public string OtherWorkConditionsMonitoring { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(OtherWorkConditionsMonitoring);
        }
    }
}
