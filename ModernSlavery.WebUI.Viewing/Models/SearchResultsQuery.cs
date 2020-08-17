using System;
using System.Collections.Generic;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class SearchResultsQuery
    {
        // TODO: dotnet core supports defining named query params using FromQueryAttribute
        // Example:
        // [FromQuery("s")] 
        // public IEnumerable<char> OrganisationSize { get; set; }

        // Keywords
        public string search { get; set; }

        // Sector
        public IEnumerable<short> s { get; set; }

        // Turnover Range
        public IEnumerable<int> tr { get; set; }

        // Reporting Year
        public IEnumerable<int> y { get; set; }

        // Page
        public int p { get; set; } = 1;

        public bool IsPageValid()
        {
            return p > 0;
        }

        public bool IsOrganisationTurnoverValid()
        {
            // if null then we won't filter on this so its valid
            if (tr == null) return true;

            foreach (var turnover in tr)
                // ensure we have a valid org turnover
                if (!Enum.IsDefined(typeof(TurnoverRanges), turnover))
                    return false;

            return true;
        }


        public bool TryValidateSearchParams(out HttpStatusViewResult exception)
        {
            exception = null;

            if (!IsPageValid()) exception = new HttpBadRequestResult($"OrganisationSearch: Invalid page {p}");

            if (!IsOrganisationTurnoverValid())
                exception = new HttpBadRequestResult($"OrganisationSearch: Invalid Turnover {tr.ToDelimitedString()}");

            return exception == null;
        }
    }
}