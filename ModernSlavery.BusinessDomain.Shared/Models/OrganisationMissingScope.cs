using System;
using System.Collections.Generic;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class OrganisationMissingScope
    {
        public Organisation Organisation { get; set; }

        public List<DateTime> MissingDeadlines { get; set; }
    }
}