using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.WebUI.Submission.Models.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class YourStatementPageViewModelMapperProfile: Profile
    {
        public YourStatementPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,YourStatementPageViewModel>()
                .ForMember(d => d.StatementStartDay, opt => opt.MapFrom(s => s.StatementStartDate.Value.Day))
                .ForMember(d => d.StatementStartMonth, opt => opt.MapFrom(s => s.StatementStartDate.Value.Month))
                .ForMember(d => d.StatementStartYear, opt => opt.MapFrom(s => s.StatementStartDate.Value.Year))
                .ForMember(d => d.StatementEndDay, opt => opt.MapFrom(s => s.StatementEndDate.Value.Day))
                .ForMember(d => d.StatementEndMonth, opt => opt.MapFrom(s => s.StatementEndDate.Value.Month))
                .ForMember(d => d.StatementEndYear, opt => opt.MapFrom(s => s.StatementEndDate.Value.Year))
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate.Value.Year));

            CreateMap<YourStatementPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.StatementStartDate, opt => opt.MapFrom(s => new DateTime(s.StatementStartYear.Value, s.StatementStartMonth.Value, s.StatementStartDay.Value)))
                .ForMember(d => d.StatementEndDate, opt => opt.MapFrom(s => new DateTime(s.StatementEndYear.Value, s.StatementEndMonth.Value, s.StatementEndDay.Value)))
                .ForMember(d => d.ApprovedDate, opt => opt.MapFrom(s => new DateTime(s.ApprovedYear.Value, s.ApprovedMonth.Value, s.ApprovedDay.Value)));
        }
    }

    public class YourStatementPageViewModel: BaseViewModel
    {
        public string StatementUrl { get; set; }

        public int? StatementStartDay { get; set; }
        public int? StatementStartMonth { get; set; }
        public int? StatementStartYear { get; set; }

        public int? StatementEndDay { get; set; }
        public int? StatementEndMonth { get; set; }
        public int? StatementEndYear { get; set; }

        public string ApproverJobTitle { get; set; }
        public string ApproverFirstName { get; set; }
        public string ApproverLastName { get; set; }

        public int? ApprovedDay { get; set; }
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }
    }
}
