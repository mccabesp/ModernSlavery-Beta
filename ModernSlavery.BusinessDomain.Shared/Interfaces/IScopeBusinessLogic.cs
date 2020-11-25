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
        Task<OrganisationScope> GetScopeByIdAsync(long organisationScopeId);

        #region GetScopeByReportingDeadlineOrLatest
        /// <summary>
        ///     Gets the latest scope for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadline"></param>
        Task<OrganisationScope> GetScopeByReportingDeadlineYearOrLatestAsync(long organisationId, int reportingDeadlineYear = 0);


        /// <summary>
        ///     Gets the latest scope for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadline"></param>
        Task<OrganisationScope> GetScopeByReportingDeadlineYearOrLatestAsync(Organisation organisation, int reportingDeadlineYear = 0);
        #endregion

        #region GetScopeByReportingDeadlineOrLatest
        /// <summary>
        ///     Gets the latest scope for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadline"></param>
        Task<OrganisationScope> GetScopeByReportingDeadlineOrLatestAsync(long organisationId, DateTime? reportingDeadline = null);

    
        /// <summary>
        ///     Gets the latest scope for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadline"></param>
        Task<OrganisationScope> GetScopeByReportingDeadlineOrLatestAsync(Organisation organisation, DateTime? reportingDeadline = null);
        #endregion

        #region GetScopeStatusByReportingDeadlineOrLatest
        /// <summary>
        ///     Gets the latest scope status for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadline"></param>
        Task<ScopeStatuses> GetScopeStatusByReportingDeadlineOrLatestAsync(long organisationId,DateTime reportingDeadline);

        /// <summary>
        /// Gets the latest scope status for the specified organisation or for a specified reporting deadline
        /// </summary>
        /// <param name="org"></param>
        /// <param name="reportingDeadline"></param>
        /// <returns></returns>
        Task<ScopeStatuses> GetScopeStatusByReportingDeadlineOrLatestAsync(Organisation organisation, DateTime? reportingDeadline = null);
        #endregion

        /// <summary>
        ///     Creates a new scope record using an existing scope and applies a new status
        /// </summary>
        /// <param name="existingOrgScopeId"></param>
        /// <param name="newStatus"></param>
        Task<OrganisationScope> UpdateScopeStatusAsync(long existingOrgScopeId,ScopeStatuses newStatus);

        Task<CustomResult<OrganisationScope>> AddScopeAsync(Organisation organisation,ScopeStatuses newStatus,User currentUser,DateTime reportingDeadline,string comment,bool saveToDatabase);

        Task SaveScopeAsync(Organisation organisation, bool saveToDatabase = true,params OrganisationScope[] newScopes);

        Task SaveScopesAsync(Organisation organisation, IEnumerable<OrganisationScope> newScopes,bool saveToDatabase = true);

        IEnumerable<ScopesFileModel> GetScopesFileModelByYear(int year);

        /// <summary>
        /// Ensure only latest scope for each year is active and rest are retired
        /// </summary>
        /// <returns></returns>
        Task<HashSet<Organisation>> FixScopeRowStatusesAsync();

        /// <summary>
        /// Ensure all organisations have an active scope for every year
        /// </summary>
        /// <returns>List of organisations whose scoped were changed</returns>
        Task<HashSet<Organisation>> SetPresumedScopesAsync();
    }
}