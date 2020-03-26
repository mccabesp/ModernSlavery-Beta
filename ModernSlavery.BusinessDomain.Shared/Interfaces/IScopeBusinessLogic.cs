using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IScopeBusinessLogic
    {
        // scope repo

        // nso repo

        // business logic
        /// <summary>
        ///     Returns the latest scope status for an organisation and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        Task<ScopeStatuses> GetLatestScopeStatusForSnapshotYearAsync(long organisationId,
            int snapshotYear);

        /// <summary>
        ///     Returns the latest scope status for an organisation and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        ScopeStatuses GetLatestScopeStatusForSnapshotYear(Organisation org, int snapshotYear = 0);

        /// <summary>
        ///     Returns the latest scope for an organisation
        /// </summary>
        /// <param name="employerReference"></param>
        Task<OrganisationScope> GetScopeByEmployerReferenceAsync(string employerReference,
            int snapshotYear = 0);

        /// <summary>
        ///     Creates a new scope record using an existing scope and applies a new status
        /// </summary>
        /// <param name="existingOrgScopeId"></param>
        /// <param name="newStatus"></param>
        Task<OrganisationScope> UpdateScopeStatusAsync(long existingOrgScopeId,
            ScopeStatuses newStatus);

        Task<CustomResult<OrganisationScope>> AddScopeAsync(Organisation organisation,
            ScopeStatuses newStatus,
            User currentUser,
            int snapshotYear,
            string comment,
            bool saveToDatabase);

        Task SaveScopeAsync(Organisation org, bool saveToDatabase = true,
            params OrganisationScope[] newScopes);

        Task SaveScopesAsync(Organisation org, IEnumerable<OrganisationScope> newScopes,
            bool saveToDatabase = true);

        IEnumerable<ScopesFileModel> GetScopesFileModelByYear(int year);
        Task<HashSet<Organisation>> SetScopeStatusesAsync();
        Task<HashSet<Organisation>> SetPresumedScopesAsync();
        bool FillMissingScopes(Organisation org);
        Task<HashSet<OrganisationMissingScope>> FindOrgsWhereScopeNotSetAsync();

        /// <summary>
        ///     Adds a new scope and updates the latest scope (if required)
        /// </summary>
        /// <param name="org"></param>
        /// <param name="scopeStatus"></param>
        /// <param name="snapshotDate"></param>
        /// <param name="currentUser"></param>
        OrganisationScope SetPresumedScope(Organisation org,
            ScopeStatuses scopeStatus,
            DateTime snapshotDate,
            User currentUser = null);

        Task<Organisation> GetOrgByEmployerReferenceAsync(string employerReference);
        Task<OrganisationScope> GetScopeByIdAsync(long organisationScopeId);

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        Task<OrganisationScope> GetLatestScopeBySnapshotYearAsync(long organisationId,
            int snapshotYear = 0);

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        OrganisationScope GetLatestScopeBySnapshotYear(Organisation organisation, int snapshotYear = 0);

        Task<OrganisationScope> GetPendingScopeRegistrationAsync(string emailAddress);
    }
}