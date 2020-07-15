using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Viewing
{
    public class SearchBusinessLogic : ISearchBusinessLogic
    {
        public SearchBusinessLogic(
            ISearchRepository<EmployerSearchModel> employerSearchRepository,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository,
            IOrganisationBusinessLogic organisationBusinessLogic,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog
        )
        {
            EmployerSearchRepository = employerSearchRepository;
            SicCodeSearchRepository = sicCodeSearchRepository;
            _organisationBusinessLogic = organisationBusinessLogic;
            SearchLog = searchLog;
        }

        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        public ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; set; }
        public ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }

        public IAuditLogger SearchLog { get; }


        //Returns a list of organisaations to include in search indexes
        public IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations)
        {
            return organisations.Where(
                o => o.Status == OrganisationStatuses.Active
                     && (o.Statements.Any(r => r.Status == StatementStatuses.Submitted)
                         || o.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope ||
                                       sc.ScopeStatus == ScopeStatuses.PresumedInScope))));
        }

        //Add or remove an organisation from the search indexes based on status and scope
        public async Task UpdateSearchIndexAsync(params Organisation[] organisations)
        {
            //Ensure we have a at least one saved organisation
            if (organisations == null || !organisations.Any(o => o.OrganisationId > 0))
                throw new ArgumentNullException(nameof(organisations), "Missing organisations");

            //Get the organisations to include or exclude from search
            var includes = LookupSearchableOrganisations(organisations).ToList();
            var excludes = organisations.Except(includes).ToList();

            //Batch update the included organisations
            if (includes.Any())
                await EmployerSearchRepository.AddOrUpdateIndexDataAsync(includes.Select(o => _organisationBusinessLogic.CreateEmployerSearchModel(o)));

            //Batch remove the excluded organisations
            if (excludes.Any())
                await EmployerSearchRepository.RemoveFromIndexAsync(excludes.Select(o =>
                    _organisationBusinessLogic.CreateEmployerSearchModel(o, true)));
        }
    }
}