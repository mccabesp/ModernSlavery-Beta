﻿using System.Collections.Generic;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Viewing.Models
{
    public class CompareViewModel
    {
        public int Year { get; set; }

        public string YearFormatted => $"{Year}-{(Year + 1).ToTwoDigitYear()}";

        public IEnumerable<CompareReportModel> CompareReports { get; set; }

        public string LastSearchUrl { get; set; }

        public int CompareBasketCount { get; set; }

        public string ShareEmailUrl { get; set; }

        public string ShareEmailSubject => "Comparing employer%27s gender pay gaps";

        public string ShareEmailBody =>
            $"Hi there,%0A%0AI compared these employers on GOV.UK. Thought you'd like to see the results...%0A%0A{ShareEmailUrl}%0A%0A";

        public bool SortAscending { get; set; }

        public string SortColumn { get; set; }

        public string FormatValue(decimal? value)
        {
            return (value ?? default(long)).ToString("0.0") + "%";
        }

        public string GetColumnSortCssClass(string columnName)
        {
            if (SortColumn == columnName) return SortAscending ? "ascending" : "descending";

            return "";
        }

        public string GetColumnSortIcon(string columnName)
        {
            if (SortColumn == columnName)
                return SortAscending ? "/assets/images/sort-glyph-up-white.png" : "/assets/images/sort-glyph-down-white.png";

            return "/assets/images/sort-glyph-noSort.png";
        }
    }
}