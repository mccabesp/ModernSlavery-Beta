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

        public IList<short> Sectors { get; set; } = new List<short>();

        public IList<byte> Turnovers { get; set; } = new List<byte>();

        public IList<int> DeadlineYears { get; set; } = new List<int>();

        

        public bool SubmittedOnly => string.IsNullOrWhiteSpace(Keywords);

        public int Page { get; set; } = 1;

        public string SearchFields { get; set; }

        public SearchModes SearchMode { get; set; }

        public int PageSize { get; set; } = 20;

        

    }
}