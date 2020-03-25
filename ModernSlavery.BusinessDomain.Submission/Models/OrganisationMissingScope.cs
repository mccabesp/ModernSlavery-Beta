using System.Collections.Generic;

namespace ModernSlavery.BusinessDomain.Submission.Models
{
    public class OrganisationMissingScope
    {
        public Core.Entities.Organisation Organisation { get; set; }

        public List<int> MissingSnapshotYears { get; set; }
    }
}