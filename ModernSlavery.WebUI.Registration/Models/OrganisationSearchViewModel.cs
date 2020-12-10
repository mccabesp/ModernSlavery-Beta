using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class OrganisationSearchViewModel
    {
        public PagedResult<OrganisationRecord> Organisations { get; set; }
        public SectorTypes? SectorType { get; set; }
        public int LastPrivateSearchRemoteTotal { get; set; }

        public int OrganisationStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return Organisations.CurrentPage * Organisations.PageSize - Organisations.PageSize + 1;
            }
        }

        public int OrganisationEndIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return OrganisationStartIndex + Organisations.Results.Count - 1;
            }
        }

        public int PagerStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.PageCount <= 5) return 1;

                if (Organisations.CurrentPage < 4) return 1;

                if (Organisations.CurrentPage + 2 > Organisations.PageCount) return Organisations.PageCount - 4;

                return Organisations.CurrentPage - 2;
            }
        }

        public int PagerEndIndex
        {
            get
            {
                if (Organisations == null) return 1;

                if (Organisations.PageCount <= 5) return Organisations.PageCount;

                if (PagerStartIndex + 4 > Organisations.PageCount) return Organisations.PageCount;

                return PagerStartIndex + 4;
            }
        }

        #region Search details

        [Required]
        [StringLength(
            100,
            ErrorMessage = "You must enter an organisations name or company number between 3 and 100 characters in length",
            MinimumLength = 3)]
        [DisplayName("Search")]
        public string SearchText { get; set; }

        #endregion

    }
}