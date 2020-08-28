using System;
using System.Collections.Generic;

namespace ModernSlavery.WebAPI.Public.Models
{
    [Serializable]
    public class SearchResultsQuery
    {
        /// <summary>
        /// The keyword to search for
        /// </summary>
        public string search { get; set; }

        /// <summary>
        /// The sectors to search for
        /// </summary>
        public IEnumerable<short> s { get; set; }

        /// <summary>
        /// The turnovers to include in the search 
        /// </summary>
        public IEnumerable<int> tr { get; set; }

        /// <summary>
        /// The reporting year to include
        /// </summary>
        public IEnumerable<int> y { get; set; }

        /// <summary>
        /// The page of results to return
        /// </summary>
        public int p { get; set; } = 1;

        /// <summary>
        /// The page size of ther results
        /// </summary>
        public int z { get; set; } = 10;


    }
}