﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ModernSlavery.Core;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class OrganisationSearchParameters
    {
        public string Keywords { get; set; }

        public IEnumerable<short> FilterSectorTypeIds { get; set; }

        public IEnumerable<byte> FilterTurnoverRanges { get; set; }

        public IEnumerable<int> FilterReportedYears { get; set; }

        public IEnumerable<int> FilterCodeIds { get; set; }

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

            if (FilterSectorTypeIds != null && FilterSectorTypeIds.Any())
            {
                var sectorQuery = FilterSectorTypeIds.Select(x => $"id eq '{x}'");
                queryFilter.Add($"SicSectionIds/any(id: {string.Join(" or ", sectorQuery)})");
            }

            if (FilterTurnoverRanges != null && FilterTurnoverRanges.Any())
            {
                var sizeQuery = FilterTurnoverRanges.Select(x => $"Size eq {x}");
                queryFilter.Add($"({string.Join(" or ", sizeQuery)})");
            }

            if (FilterCodeIds != null && FilterCodeIds.Any())
            {
                var codeIdQuery = FilterCodeIds.Select(x => $"id eq '{x}'");
                queryFilter.Add($"SicCodeIds/any(id: {string.Join(" or ", codeIdQuery)})");
            }

            var anyReportedYearParam = "";
            if (FilterReportedYears != null && FilterReportedYears.Any())
            {
                anyReportedYearParam = "ReportedYear: " +
                                       string.Join(" or ", FilterReportedYears.Select(x => $"ReportedYear eq '{x}'"));
                queryFilter.Add($"ReportedYears/any({anyReportedYearParam})");
            }

            return string.Join(" and ", queryFilter);
        }
    }
}