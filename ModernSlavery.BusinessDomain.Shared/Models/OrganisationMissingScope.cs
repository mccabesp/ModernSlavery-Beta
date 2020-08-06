using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class OrganisationMissingScope
    {
        public Organisation Organisation { get; set; }

        public List<int> MissingYears { get; set; }
    }
}