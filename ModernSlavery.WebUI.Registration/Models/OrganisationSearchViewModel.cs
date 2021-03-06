﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Registration.Models
{
    [Serializable]
    public class OrganisationSearchViewModel
    {
        [BindNever]
        public PagedResult<OrganisationRecord> Organisations { get; set; }
        [BindNever]
        public int LastPrivateSearchRemoteTotal { get; set; }

        [BindNever] 
        public SectorTypes? SectorType { get; set; }
        

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
                if (Organisations == null || Organisations.VirtualPageCount <= 5) return 1;

                if (Organisations.CurrentPage < 4) return 1;

                if (Organisations.CurrentPage + 2 > Organisations.VirtualPageCount) return Organisations.VirtualPageCount - 4;

                return Organisations.CurrentPage - 2;
            }
        }

        public int PagerEndIndex
        {
            get
            {
                if (Organisations == null) return 1;

                if (Organisations.VirtualPageCount <= 5) return Organisations.VirtualPageCount;

                if (PagerStartIndex + 4 > Organisations.VirtualPageCount) return Organisations.VirtualPageCount;

                return PagerStartIndex + 4;
            }
        }

        #region Search details

        [Required]
        [StringLength(160,ErrorMessage = "You must enter an organisations name or company number between 3 and 160 characters in length",MinimumLength = 3)]
        [DisplayName("Search")]
        [Text]
        public string SearchText { get; set; }

        #endregion

    }
}