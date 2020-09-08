using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupOrganisationsViewModelMapperProfile : Profile
    {
        private readonly IObfuscator _obfuscator;
        public GroupOrganisationsViewModelMapperProfile(IObfuscator obfuscator)
        {
            _obfuscator = obfuscator;
        }

        public GroupOrganisationsViewModelMapperProfile()
        {
            CreateMap<StatementModel.StatementOrganisationModel, GroupOrganisationsViewModel.StatementOrganisationViewModel>()
                .ForMember(d => d.StatementOrganisationIdentifier, opt => opt.MapFrom(s => _obfuscator.Obfuscate(s.StatementOrganisationId)))
                .ForMember(d => d.OrganisationIdentifier, opt => opt.MapFrom(s => s.OrganisationId==null ? null : _obfuscator.Obfuscate(s.OrganisationId.Value)));

            CreateMap<GroupOrganisationsViewModel.StatementOrganisationViewModel, StatementModel.StatementOrganisationModel>()
                .ForMember(d => d.StatementOrganisationId, opt => opt.MapFrom(s => _obfuscator.DeObfuscate(s.StatementOrganisationIdentifier)))
                .ForMember(d => d.OrganisationId, opt => opt.MapFrom(s => s.OrganisationIdentifier == null ? default : _obfuscator.DeObfuscate(s.OrganisationIdentifier)));

            CreateMap<GroupOrganisationsViewModel, StatementModel>(MemberList.Source)
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());

            CreateMap<StatementModel, GroupOrganisationsViewModel>();
        }
    }

    public class GroupOrganisationsViewModel : BaseViewModel
    {
        public bool? GroupSubmission { get; set; }

        public override string PageTitle => "Which organisations are included in your group statement?";

        #region Types
        public class StatementOrganisationViewModel
        {
            public string StatementOrganisationIdentifier { get; set; }
            public string OrganisationIdentifier { get; set; }
            public bool Included { get; set; }

            [MaxLength(100)]
            public string OrganisationName { get; set; }
            [BindNever] 
            public string Address { get; set; }
            [BindNever]
            public string CompanyNumber { get; set; }
        }
        #endregion

        public List<StatementOrganisationViewModel> StatementOrganisations { get; set; } = new List<StatementOrganisationViewModel>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override bool IsComplete()
        {
            return GroupSubmission.HasValue && (GroupSubmission==false || StatementOrganisations.Any());
        }
    }
}
