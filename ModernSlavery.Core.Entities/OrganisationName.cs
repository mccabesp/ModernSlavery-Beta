using System;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    public class OrganisationName
    {
        public long OrganisationNameId { get; set; }
        public long OrganisationId { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual Organisation Organisation { get; set; }
    }
}