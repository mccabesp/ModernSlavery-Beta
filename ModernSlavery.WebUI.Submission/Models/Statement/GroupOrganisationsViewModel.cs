using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupOrganisationsViewModelMapperProfile : Profile
    {
        public GroupOrganisationsViewModelMapperProfile()
        {
            CreateMap<StatementModel.StatementOrganisationModel, GroupOrganisationsViewModel.StatementOrganisationViewModel>()
                .ForMember(d => d.OtherSubmissionsInformation, opt => opt.Ignore())
                .ForMember(d => d.ManuallyAdded, opt => opt.Ignore());

            CreateMap<GroupOrganisationsViewModel.StatementOrganisationViewModel, StatementModel.StatementOrganisationModel>();

            CreateMap<StatementModel, GroupOrganisationsViewModel>()
                .ForMember(s => s.GroupSubmission, opt => opt.MapFrom(d => d.GroupSubmission))
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<GroupOrganisationsViewModel, StatementModel>(MemberList.Source)
                .ForMember(s => s.GroupSubmission, opt => opt.MapFrom(d => d.GroupSubmission))
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class GroupOrganisationsViewModel : BaseViewModel
    {
        public bool? GroupSubmission { get; set; }

        public override string PageTitle => "Which organisations are included in your group statement?";

        #region Types
        public class StatementOrganisationViewModel
        {
            public long? StatementOrganisationId { get; set; }
            public long? OrganisationId { get; set; }
            public bool Included { get; set; }
            public string OrganisationName { get; set; }
            public AddressModel Address { get; set; }
            public string CompanyNumber { get; set; }
            public DateTime? DateOfCessation { get; set; }
            [NotMapped]
            [BindNever]
            public List<string> OtherSubmissionsInformation { get; set; }

            [NotMapped]
            [BindNever]
            public bool ManuallyAdded => OrganisationId == null && string.IsNullOrWhiteSpace(CompanyNumber);
        }
        #endregion

        public List<StatementOrganisationViewModel> StatementOrganisations { get; set; } = new List<StatementOrganisationViewModel>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (GroupSubmission.HasValue && (GroupSubmission == false || StatementOrganisations.Any())) return Status.Complete;
            return Status.Incomplete;
        }
    }
}
