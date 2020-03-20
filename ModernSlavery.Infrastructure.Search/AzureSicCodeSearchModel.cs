using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    [Serializable]
    public class AzureSicCodeSearchModel : SicCodeSearchModel
    {
        [Key] [IsSearchable] public override string SicCodeId { get; set; }

        [IsSearchable] public override string SicCodeDescription { get; set; }

        [IsSearchable] public override string[] SicCodeListOfSynonyms { get; set; }
    }
}