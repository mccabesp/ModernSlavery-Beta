using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace ModernSlavery.WebAPI.Models
{
    [Serializable]
    [BindProperties(SupportsGet = true)]
    public class SearchQueryModel
    {
        /// <summary>
        /// The keyword to search for
        /// </summary>
        [FromQuery(Name = "search")]
        public string Keywords { get; set; }

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


        public bool IsPageValid()
        {
            return PageNumber > 0;
        }

        public bool IsOrganisationTurnoverValid()
        {
            // if null then we won't filter on this so its valid
            if (Turnovers == null) return true;

            foreach (var turnover in Turnovers)
                // ensure we have a valid org turnover
                if (!Enum.IsDefined(typeof(StatementTurnovers), turnover))
                    return false;

            return true;
        }

        public bool TryValidateSearchParams(out HttpStatusCodeResult result)
        {
            result = null;

            if (!IsPageValid()) result = new HttpStatusCodeResult(HttpStatusCode.BadRequest,$"Invalid page {PageNumber}");

            if (!IsOrganisationTurnoverValid())
                result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"Invalid Turnovers {Turnovers.ToDelimitedString()}");

            return result == null;
        }

    }
}