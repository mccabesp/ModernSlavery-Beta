using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Entities
{
    partial class StatementMetadata
    {
        public bool CanBeEdited
            // a stub for checking if this entity is allowed to be edited
            // eg checking state
            => true;

        public bool IsValid()
        {
            // Do we validate here?
            // The validation should run against the entity
            // But what about the ViewModel? There will most likely be overlap
            // Are data annotations enough for the view model?

            if (Status == ReturnStatuses.Draft)
            {
                // fields can be null
            }
            else
            {
                // fields are not allowed to be null
            }

            throw new NotImplementedException();
        }
    }
}
