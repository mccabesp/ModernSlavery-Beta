using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class SearchViewModel
    {
        public List<OptionSelect> SectorOptions { get; set; }
        public List<OptionSelect> ReportingYearOptions { get; set; }
        public List<OptionSelect> TurnoverOptions { get; internal set; }

        public FilterGroup GetTurnoverGroup() => new FilterGroup
        {
            Id = "TurnoverFilter",
            Group = "t",
            Label = "Turnover or budget",
            Expanded = false,
            Metadata = TurnoverOptions
        };

        public FilterGroup GetSectorGroup() => new FilterGroup
        {
            Id = "SectorFilter",
            Group = "s",
            Label = "Industry sector",
            Expanded = false,
            Metadata = SectorOptions,
            MaxHeight = "300px"
        };

        public FilterGroup GetYearGroup() => new FilterGroup
        {
            Id = "ReportingYearFilter",
            Group = "y",
            Label = "Statement year",
            Expanded = false,
            Metadata = ReportingYearOptions
        };
        /// <summary>
        /// The keyword to search for
        /// </summary>
        [FromQuery(Name = "search")]
        public string Keywords { get; set; }

        /// <summary>
        /// The sectors to search for
        /// </summary>
        [FromQuery(Name = "s")]
        public IEnumerable<short> Sectors { get; set; }

        /// <summary>
        /// The turnovers to include in the search 
        /// </summary>
        [FromQuery(Name = "t")]
        public IEnumerable<byte> Turnovers { get; set; }

        /// <summary>
        /// The the end year of thwe reporting deadlines to include
        /// </summary>
        [FromQuery(Name = "y")]
        public IEnumerable<int> Years { get; set; }

        /// <summary>
        /// The page of results to return
        /// </summary>
        [FromQuery(Name = "p")]
        public int PageNumber { get; set; } = 1;

        public PagedResult<OrganisationSearchModel> Organisations { get; set; }

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

        //public OrganisationSearchModel GetOrganisation(string organisationIdentifier)
        //{
        //    //Get the organisation from the last search results
        //    return Organisations?.Results?.FirstOrDefault(e => e.OrganisationIdEncrypted == organisationIdentifier);
        //}

        [Serializable]
        public class SectorTypeViewModel
        {
            public short SectorTypeId { get; set; }

            public string Description { get; set; }
        }
    }
}