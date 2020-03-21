using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.BusinessLogic
{
    public interface ISearchBusinessLogic
    {
        IRecordLogger SearchLog { get; }
        ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; set; }
        ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }
        IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations);
        Task UpdateSearchIndexAsync(params Organisation[] organisations);
    }

    public class SearchBusinessLogic : ISearchBusinessLogic
    {
        public SearchBusinessLogic(
            ISearchRepository<EmployerSearchModel> employerSearchRepository,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository,
            [KeyFilter(Filenames.SearchLog)] IRecordLogger searchLog
        )
        {
            EmployerSearchRepository = employerSearchRepository;
            SicCodeSearchRepository = sicCodeSearchRepository;
            SearchLog = searchLog;
        }

        public ISearchRepository<EmployerSearchModel> EmployerSearchRepository { get; set; }
        public ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository { get; }

        public IRecordLogger SearchLog { get; }


        //Returns a list of organisaations to include in search indexes
        public IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations)
        {
            return organisations.Where(
                o => o.Status == OrganisationStatuses.Active
                     && (o.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
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
                await EmployerSearchRepository.AddOrUpdateIndexDataAsync(includes.Select(o =>
                    EmployerSearchModel.Create(o)));

            //Batch remove the excluded organisations
            if (excludes.Any())
                await EmployerSearchRepository.RemoveFromIndexAsync(excludes.Select(o =>
                    EmployerSearchModel.Create(o, true)));
        }
    }
}