using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GroupSearchViewModelMapperProfile : Profile
    {
        public GroupSearchViewModelMapperProfile()
        {
            CreateMap<StatementModel, GroupSearchViewModel>()
                .ForMember(d => d.SearchKeywords, opt => opt.Ignore())
                .ForMember(d => d.ResultsPage, opt => opt.Ignore())
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));

            CreateMap<GroupSearchViewModel, StatementModel>(MemberList.None)
                .ForMember(s => s.StatementOrganisations, opt => opt.MapFrom(d => d.StatementOrganisations));
        }
    }

    public class GroupSearchViewModel : GroupOrganisationsViewModel
    {
        public override string PageTitle => "Which organisations are included in your group statement?";

        [Required(AllowEmptyStrings = false)]
        [StringLength(100, ErrorMessage = "You must enter an organisations name or company number between 3 and 100 characters in length", MinimumLength = 3)]
        [Text] 
        public string SearchKeywords { get; set; }

        public PagedResult<OrganisationRecord> ResultsPage { get; set; } = new PagedResult<OrganisationRecord>();

        public int ResultsStartIndex
        {
            get
            {
                if (ResultsPage == null || ResultsPage.Results == null || ResultsPage.Results.Count < 1) return 1;

                return ResultsPage.CurrentPage * ResultsPage.PageSize - ResultsPage.PageSize + 1;
            }
        }

        public int ResultsEndIndex
        {
            get
            {
                if (ResultsPage == null || ResultsPage.Results == null || ResultsPage.Results.Count < 1) return 1;

                return ResultsStartIndex + ResultsPage.Results.Count - 1;
            }
        }

        public int PagerStartIndex
        {
            get
            {
                if (ResultsPage == null || ResultsPage.ActualPageCount <= 5) return 1;

                if (ResultsPage.CurrentPage < 4) return 1;

                if (ResultsPage.CurrentPage + 2 > ResultsPage.ActualPageCount) return ResultsPage.ActualPageCount - 4;

                return ResultsPage.CurrentPage - 2;
            }
        }


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
            var address = organisationRecord?.GetFullAddress();
            if (!string.IsNullOrWhiteSpace(address)) return StatementOrganisations.FindIndex(o => o.OrganisationName.EqualsI(organisationRecord.NameSource) && address.EqualsI(o.Address?.GetFullAddress()));
            return StatementOrganisations.FindIndex(o => o.OrganisationName.EqualsI(organisationRecord.NameSource));
        }

    }
}
