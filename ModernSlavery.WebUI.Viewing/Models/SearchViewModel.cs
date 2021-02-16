using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Models;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public partial class SearchViewModel: BaseViewModel
    {
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly SectorTypeIndex _sectorTypes;

        public SearchViewModel(IReportingDeadlineHelper reportingDeadlineHelper, SectorTypeIndex sectorTypes)
        {
            _reportingDeadlineHelper = reportingDeadlineHelper ?? throw new ArgumentNullException(nameof(reportingDeadlineHelper));
            _sectorTypes = sectorTypes ?? throw new ArgumentNullException(nameof(sectorTypes));
        }

        #region Search querystring parameters
        /// <summary>
        /// The keyword to search for
        /// </summary>
        [FromQuery(Name = "Search")]
        [Text]
        public string Search { get; set; }

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

        /// <summary>
        /// The page size of ther results
        /// </summary>
        [FromQuery(Name = "z")]
        public int PageSize { get; set; } = 10;
        #endregion

        #region Results Data
        [BindNever] 
        public PagedResult<OrganisationSearchModel> Organisations { get; set; }

        [BindNever]
        public int OrganisationStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return Organisations.CurrentPage * Organisations.PageSize - Organisations.PageSize + 1;
            }
        }

        [BindNever]
        public int OrganisationEndIndex
        {
            get
            {
                if (Organisations == null || Organisations.Results == null || Organisations.Results.Count < 1) return 1;

                return OrganisationStartIndex + Organisations.Results.Count - 1;
            }
        }

        [BindNever]
        public int PagerStartIndex
        {
            get
            {
                if (Organisations == null || Organisations.ActualPageCount <= 5) return 1;

                if (Organisations.CurrentPage < 4) return 1;

                if (Organisations.CurrentPage + 2 > Organisations.ActualPageCount) return Organisations.ActualPageCount - 4;

                return Organisations.CurrentPage - 2;
            }
        }
        #endregion

        #region Filter Groups
        public FilterGroup GetTurnoverGroup() => new FilterGroup {
            Id = "TurnoverFilter",
            Group = "t",
            Label = "Turnover or budget",
            Expanded = false,
            Metadata = GetTurnoverOptions()
        };

        public FilterGroup GetSectorGroup() => new FilterGroup {
            Id = "SectorFilter",
            Group = "s",
            Label = "Sectors",
            Expanded = false,
            Metadata = GetSectorOptions(),
            MaxHeight = "300px"
        };

        public FilterGroup GetYearGroup() => new FilterGroup {
            Id = "ReportingYearFilter",
            Group = "y",
            Label = "Statement year",
            Expanded = false,
            Metadata = GetReportingYearOptions()
        };
        public List<OptionSelect> GetTurnoverOptions()
        {
            var allRanges = Enums.GetValues<StatementTurnoverRanges>();

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (var range in allRanges)
            {
                if (range == StatementTurnoverRanges.NotProvided) continue;
                var id = (byte)range;
                var label = range.GetEnumDescription();
                var isChecked = Turnovers != null && Turnovers.Any(t => t == id);
                results.Add(
                    new OptionSelect {
                        Id = $"Turnover{id}",
                        Label = label,
                        Value = id.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }
        public List<OptionSelect> GetSectorOptions()
        {
            // setup the filters
            var sources = new List<OptionSelect>();
            foreach (var sectorType in _sectorTypes)
            {
                sources.Add(
                    new OptionSelect {
                        Id = sectorType.Id.ToString(),
                        Label = sectorType.Description.TrimEnd('\r', '\n'),
                        Value = sectorType.Id.ToString(),
                        Checked = Sectors != null && Sectors.Any(s => s == sectorType.Id)
                    });
            }

            return sources;
        }
        public List<OptionSelect> GetReportingYearOptions()
        {
            // setup the filters
            var reportingDeadlines = _reportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Public);
            var sources = new List<OptionSelect>();
            foreach (var reportingDeadline in reportingDeadlines)
            {
                var isChecked = Years != null && Years.Any(y => y == reportingDeadline.Year);
                sources.Add(
                    new OptionSelect {
                        Id = reportingDeadline.Year.ToString(),
                        Label = reportingDeadline.Year.ToString(),
                        Value = reportingDeadline.Year.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }


        #endregion

        #region Validation methods
        public bool IsOrganisationTurnoverValid()
        {
            // if null then we won't filter on this so its valid
            if (Turnovers == null) return true;

            foreach (var turnover in Turnovers)
                // ensure we have a valid org turnover
                if (!Enum.IsDefined(typeof(StatementTurnoverRanges), turnover))
                    return false;

            return true;
        }
        public bool TryValidateSearchParams(out HttpStatusViewResult exception)
        {
            exception = null;

            if (!IsPageValid()) exception = new HttpBadRequestResult($"Invalid page {PageNumber}");

            if (!IsOrganisationTurnoverValid())
                exception = new HttpBadRequestResult($"Invalid Turnover {Turnovers.ToDelimitedString()}");

            return exception == null;
        }
        public bool IsPageValid()
        {
            return PageNumber > 0;
        }
        #endregion
    }
}