using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    [Serializable]
    public class AzureEmployerSearchModel : OrganisationSearchModel
    {
        #region Organisation Properties

        [Key] public override string SearchDocumentKey { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        [IsFilterable]
        [IsSortable]
        public override string Name { get; set; }

        [IsFilterable]
        public override long OrganisationId { get; set; }

        [IsFilterable]
        public override long StatementId { get; set; }

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
        public override byte Turnover { get; set; }

        [IsFilterable] 
        [IsFacetable] 
        public override short[] SectorTypeIds { get; set; }

        [IsFilterable] 
        [IsFacetable] 
        public override string StatementDeadlineYear { get; set; }

        #endregion
    }
}