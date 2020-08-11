using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISearchBusinessLogic
    {
        SearchOptions SearchOptions { get; }
        IAuditLogger SearchLog { get; }
        ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; set; }
        ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }
        IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations);
        Task UpdateSearchIndexAsync(params Organisation[] organisations);
    }
}