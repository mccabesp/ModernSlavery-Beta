using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    [Serializable]
    public class GroupResultsViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string SearchKeywords { get; set; }
        [Required(AllowEmptyStrings =false)]
        [MaxLength(100)]
        public string OrganisationName { get; set; }

        public bool ShowResults { get; set; } = true;

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
                if (ResultsPage == null || ResultsPage.PageCount <= 5) return 1;

                if (ResultsPage.CurrentPage < 4) return 1;

                if (ResultsPage.CurrentPage + 2 > ResultsPage.PageCount) return ResultsPage.PageCount - 4;

                return ResultsPage.CurrentPage - 2;
            }
        }
    }
}