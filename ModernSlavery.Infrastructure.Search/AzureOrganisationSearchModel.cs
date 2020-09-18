using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    [Serializable]
    public class AzureOrganisationSearchModel : OrganisationSearchModel
    {
        #region Organisation Properties
        [Key] 
        public override string SearchDocumentKey { get; set; }

        [IsSearchable, Analyzer(AnalyzerName.AsString.EnLucene)]
        [IsFilterable]
        [IsSortable]
        public override string OrganisationName { get; set; }

        [IsSearchable]
        public override string CompanyNumber { get; set; }

        [IsFilterable]
        public override long ParentOrganisationId { get; set; }

        [IsFilterable]
        public override long? StatementId { get; set; }

        [IsRetrievable(false)]
        [IsSearchable] 
        public override string PartialNameForSuffixSearches { get; set; }

        [IsRetrievable(false)]
        [IsSearchable] 
        public override string PartialNameForCompleteTokenSearches { get; set; }

        [IsRetrievable(false)]
        [IsSearchable, Analyzer(AnalyzerName.AsString.EnLucene)]
        public override string[] Abbreviations { get; set; }

        [IsFilterable]
        //[IsFacetable]
        public override int? Turnover { get; set; }

        [IsFilterable] 
        //[IsFacetable] 
        public override int[] SectorTypeIds { get; set; }

        [IsSortable]
        [IsFilterable] 
        //[IsFacetable] 
        public override int? StatementDeadlineYear { get; set; }

        [IsRetrievable(false)]
        [IsSortable]
        public override DateTime Modified { get; set;  } = VirtualDateTime.Now;

        [IsRetrievable(false)]
        public override DateTime Timestamp { get; } = VirtualDateTime.Now;

        #endregion
    }
}