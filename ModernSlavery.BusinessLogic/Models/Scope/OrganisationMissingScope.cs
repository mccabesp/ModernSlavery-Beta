using System.Collections.Generic;
using ModernSlavery.Entities;

namespace ModernSlavery.BusinessLogic.Models.Scope
{
    public class OrganisationMissingScope
    {

        public ModernSlavery.Entities.Organisation Organisation { get; set; }

        public List<int> MissingSnapshotYears { get; set; }

    }
}
