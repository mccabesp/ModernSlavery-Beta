using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Admin.Models
{
    public class AdminSearchViewModel
    {
        /// <summary>
        /// The keyword to search for
        /// </summary>
        [FromQuery(Name = "Search")]
        [Required(AllowEmptyStrings =false)]
        [Text]
        public string Search { get; set; }

        [BindNever]
        public AdminSearchResultsViewModel SearchResults { get; set; }
    }

    public class AdminSearchResultsViewModel
    {
        public List<AdminSearchResultOrganisationViewModel> OrganisationResults { get; set; }
        public List<AdminSearchResultUserViewModel> UserResults { get; set; }

        public double LoadingMilliSeconds { get; set; }
        public double FilteringMilliSeconds { get; set; }
        public double OrderingMilliSeconds { get; set; }
        public double HighlightingMilliSeconds { get; set; }
        public int SearchCacheUpdatedSecondsAgo { get; set; }
        public bool UsedCache { get; set; }
    }

    public class AdminSearchResultOrganisationViewModel
    {
        public AdminSearchMatchViewModel OrganisationName { get; set; }
        public List<AdminSearchMatchViewModel> OrganisationPreviousNames { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationRef { get; set; }
        public string CompanyNumber { get; set; }
    }

    public class AdminSearchResultUserViewModel
    {
        public AdminSearchMatchViewModel UserFullName { get; set; }
        public AdminSearchMatchViewModel UserEmailAddress { get; set; }
        public long UserId { get; set; }
    }

    public class AdminSearchMatchViewModel
    {
        public string Text { get; set; }
        public List<AdminSearchMatchGroupViewModel> MatchGroups { get; set; }
    }

    public class AdminSearchMatchGroupViewModel
    {
        public int Start { get; set; }
        public int Length { get; set; }
    }
}