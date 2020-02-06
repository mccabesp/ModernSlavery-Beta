using System;
using ModernSlavery.Extensions;

namespace ModernSlavery.Database
{
    public class OrganisationReference
    {

        public long OrganisationReferenceId { get; set; }
        public long OrganisationId { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceValue { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual Organisation Organisation { get; set; }

    }
}
