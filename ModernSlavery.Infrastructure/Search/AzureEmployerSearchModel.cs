using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure
{
    [Serializable]
    public class AzureEmployerSearchModel : EmployerSearchModel
    {
        #region Organisation Properties

        [Key] public override string OrganisationId { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        [IsFilterable]
        [IsSortable]
        public override string Name { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public override string PreviousName { get; set; }

        [IsSearchable] public override string PartialNameForSuffixSearches { get; set; }

        [IsSearchable] public string PartialNameForCompleteTokenSearches { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public override string[] Abbreviations { get; set; }

        [IsFilterable]
        [IsSortable]
        [IsFacetable]
        public override int Size { get; set; }

        [IsFilterable] [IsFacetable] public override string[] SicSectionIds { get; set; }

        public override string[] SicSectionNames { get; set; }

        [IsSearchable]
        [IsFilterable]
        [IsFacetable]
        public override string[] SicCodeIds { get; set; }

        [IsSearchable] public override string[] SicCodeListOfSynonyms { get; set; }

        [IsFilterable] [IsFacetable] public override string[] ReportedYears { get; set; }

        [IsFilterable] [IsFacetable] public override DateTimeOffset LatestReportedDate { get; set; }

        [IsFilterable] [IsFacetable] public override string[] ReportedLateYears { get; set; }

        [IsFilterable] [IsFacetable] public override string[] ReportedExplanationYears { get; set; }

        #endregion
    }
}