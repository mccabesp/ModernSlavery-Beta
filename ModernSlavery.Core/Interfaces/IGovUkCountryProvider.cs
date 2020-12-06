using ModernSlavery.Core.Entities.StatementSummary.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Interfaces
{
    public interface IGovUkCountryProvider
    {
        IEnumerable<GovUkCountry> GetAll();

        GovUkCountry FindByReference(string reference);

        GovUkCountry Find(string text);
    }
}
