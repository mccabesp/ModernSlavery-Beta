using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class SearchViewModel
    {
        public enum TurnoverRanges : byte
        {
            //Not Provided
            NotProvided = 0,

            [GovUkRadioCheckboxLabelText(Text = "Under £36 million")]
            Under36Million = 1,

            [GovUkRadioCheckboxLabelText(Text = "£36 million to £60 million")]
            From36to60Million = 2,

            [GovUkRadioCheckboxLabelText(Text = "£60 million to £100 million")]
            From60to100Million = 3,

            [GovUkRadioCheckboxLabelText(Text = "£100 million to £500 million")]
            From100to500Million = 4,

            [GovUkRadioCheckboxLabelText(Text = "Over £500 million")]
            Over500Million = 5,
        }

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

        public string search { get; set; }

        public IEnumerable<short> s { get; set; }
        public IEnumerable<byte> tr { get; set; }
        public IEnumerable<int> y { get; set; }
        public int p { get; set; }

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
                if (_sectorFilterInfo == null) _sectorFilterInfo = OptionSelect.GetCheckedString(SectorOptions);

                return _sectorFilterInfo;
            }
        }

        public List<string> TurnoverFilterInfo
        {
            get
            {
                if (_sizeFilterInfo == null) _sizeFilterInfo = OptionSelect.GetCheckedString(TurnoverOptions);

                return _sizeFilterInfo;
            }
        }

        public List<string> ReportingYearFilterInfo
        {
            get
            {
                if (_reportingYearFilterInfo == null)
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