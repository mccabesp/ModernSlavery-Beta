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
        private List<string> _reportingYearFilterInfo;

        private List<string> _sectorFilterInfo;

        private List<string> _sizeFilterInfo;

        public SearchViewModel()
        {
            _sectorFilterInfo = new List<string>();
            _sizeFilterInfo = new List<string>();
            _reportingYearFilterInfo = new List<string>();
        }

        public List<OptionSelect> SectorOptions { get; set; }
        public List<OptionSelect> ReportingYearOptions { get; set; }
        public List<OptionSelect> TurnoverOptions { get; internal set; }

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

        public List<string> SectorFilterInfo
        {
            get
            {
                if (_sectorFilterInfo.Count == 0) _sectorFilterInfo = OptionSelect.GetCheckedString(SectorOptions);

                return _sectorFilterInfo;
            }
        }

        public List<string> TurnoverFilterInfo
        {
            get
            {
                if (_sectorFilterInfo.Count == 0) _sizeFilterInfo = OptionSelect.GetCheckedString(TurnoverOptions);

                return _sizeFilterInfo;
            }
        }

        public List<string> ReportingYearFilterInfo
        {
            get
            {
                if (_sectorFilterInfo.Count == 0)
                    _reportingYearFilterInfo = OptionSelect.GetCheckedString(ReportingYearOptions);

                return _reportingYearFilterInfo;
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