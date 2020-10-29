using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupAddViewModelMapperProfile : Profile
    {
        public GroupAddViewModelMapperProfile()
        {
            CreateMap<GroupAddViewModel, StatementModel>(MemberList.Source)
                .IncludeBase<GroupOrganisationsViewModel, StatementModel>()
                .ForSourceMember(d => d.GroupResults, opt => opt.DoNotValidate())
                .ForSourceMember(d => d.GroupReviewUrl, opt => opt.DoNotValidate())
                .ForSourceMember(d => d.ShowBanner, opt => opt.DoNotValidate());

            CreateMap<StatementModel, GroupAddViewModel>()
                .IncludeBase<StatementModel, GroupOrganisationsViewModel>();
        }
    }

    public class GroupAddViewModel : GroupOrganisationsViewModel
    {  
        public override string PageTitle => "Which organisations are included in your group statement?";
        [IgnoreMap]
        [BindNever]
        public string GroupReviewUrl { get; set; }

        [IgnoreMap]
        public GroupResultsViewModel GroupResults { get; set; } = new GroupResultsViewModel();
        [IgnoreMap]
        [BindNever]
        public bool? ShowBanner { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }

        /// <summary>
        /// Checks if a search record matches an included group organisation
        /// </summary>
        /// <param name="organisationRecord">The search record to check</param>
        /// <returns>True if the organisation has already been included otherwise false</returns>
        public bool ContainsGroupOrganisation(OrganisationRecord organisationRecord)
        {
            return FindGroupOrganisation(organisationRecord) > -1;
        }

        /// <summary>
        /// Find the index of and existing search record matches included as a group organisation
        /// </summary>
        /// <param name="organisationRecord">The search record to find</param>
        /// <returns>The index of the found organisation otherwise -1</returns>
        public int FindGroupOrganisation(OrganisationRecord organisationRecord)
        {
            if (organisationRecord.OrganisationId > 0) return StatementOrganisations.FindIndex(o => o.OrganisationId == organisationRecord.OrganisationId);
            if (!string.IsNullOrWhiteSpace(organisationRecord.CompanyNumber)) return StatementOrganisations.FindIndex(o => o.CompanyNumber.EqualsI(organisationRecord.CompanyNumber));
            var address = organisationRecord.GetFullAddress();
            if (!string.IsNullOrWhiteSpace(address)) return StatementOrganisations.FindIndex(o => o.OrganisationName.EqualsI(organisationRecord.NameSource) && o.Address.GetFullAddress().EqualsI(address));
            return StatementOrganisations.FindIndex(o => o.OrganisationName.EqualsI(organisationRecord.NameSource));
        }

    }
}
