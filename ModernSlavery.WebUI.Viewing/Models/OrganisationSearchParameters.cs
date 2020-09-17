using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class OrganisationSearchParameters
    {
        public string Keywords { get; set; }

        public IList<short> FilterSectorTypeIds { get; set; } = new List<short>();

        public IList<byte> FilterTurnoverRanges { get; set; } = new List<byte>();

        public IList<int> FilterReportedYears { get; set; } = new List<int>();

        public int FilterCount => FilterSectorTypeIds.Count + FilterTurnoverRanges.Count + FilterReportedYears.Count;

        public bool SubmittedOnly => string.IsNullOrWhiteSpace(Keywords);

        public int Page { get; set; } = 1;

        public string SearchFields { get; set; }

        public SearchModes SearchMode { get; set; }

        public int PageSize { get; set; } = 20;

        public string RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords()
        {
            if (string.IsNullOrEmpty(Keywords)) return Keywords;

            const string patternToReplace = "(?i)(limited|ltd|llp|uk | uk|\\(uk\\)|-uk|plc)[\\.]*";

            string resultingString;

            resultingString = Regex.Replace(Keywords, patternToReplace, string.Empty);
            resultingString = resultingString.Trim();

            var willThisReplacementClearTheString = resultingString == string.Empty;

            return willThisReplacementClearTheString
                ? Keywords // don't replace - user wants to search 'limited' or 'uk'...
                : resultingString;
        }

        public string ToFilterQuery()
        {
            var queryFilter = new List<string>();

            if (FilterTurnoverRanges.Count>0)
            {
                var turnoverQuery = FilterTurnoverRanges.Select(x => $"Turnover eq {x}");
                queryFilter.Add($"({string.Join(" or ", turnoverQuery)})");
            }

            if (FilterSectorTypeIds.Count > 0)
            {
                var sectorQuery = FilterSectorTypeIds.Select(x => $"id eq {x}");
                queryFilter.Add($"SectorTypeIds/any(id: {string.Join(" or ", sectorQuery)})");
            }

            if (FilterReportedYears.Count > 0)
            {
                var deadlineQuery = FilterReportedYears.Select(x => $"StatementDeadlineYear eq {x}");
                queryFilter.Add($"({string.Join(" or ", deadlineQuery)})");
            }

            //Only show submitted organisations
            if (SubmittedOnly)
            {
                queryFilter.Add($"StatementId ne null");
            }

            return string.Join(" and ", queryFilter);
        }

        public string ToSearchCriteria()
        {
            if (string.IsNullOrWhiteSpace(Keywords) && FilterCount == 0)
                return $"{nameof(OrganisationSearchModel.Modified)} desc, {nameof(OrganisationSearchModel.OrganisationName)}, {nameof(OrganisationSearchModel.StatementDeadlineYear)} desc";

            return $"{nameof(OrganisationSearchModel.OrganisationName)}, {nameof(OrganisationSearchModel.StatementDeadlineYear)} desc";
        }

    }
}