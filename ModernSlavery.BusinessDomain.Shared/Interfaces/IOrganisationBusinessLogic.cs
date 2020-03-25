using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IOrganisationBusinessLogic
    {
        // Organisation repo
        IDnBOrgsRepository DnBOrgsRepository { get; }

        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<List<OrganisationsFileModel>> GetOrganisationsFileModelByYearAsync(int year);

        Task SetUniqueEmployerReferencesAsync();
        Task SetUniqueEmployerReferenceAsync(Organisation organisation);
        string GenerateEmployerReference();
        string GeneratePINCode(bool isTestUser);

        Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string employerRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase);

        CustomResult<Organisation> LoadInfoFromEmployerIdentifier(string employerIdentifier);
        CustomResult<Organisation> LoadInfoFromActiveEmployerIdentifier(string employerIdentifier);
        Task<CustomResult<Organisation>> GetOrganisationByEncryptedReturnIdAsync(string encryptedReturnId);
        string GetSicSectorsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        IEnumerable<OrganisationSicCode> GetSicCodes(Organisation org, DateTime? maxDate = null);

        SortedSet<int> GetSicCodeIds(Organisation org, DateTime? maxDate = null);
        string GetSicSource(Organisation org, DateTime? maxDate = null);
        string GetSicCodeIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");
        string GetSicSectionIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");

        AddressModel GetAddressModel(Organisation org, DateTime? maxDate = null,
            AddressStatuses status = AddressStatuses.Active);

        CustomError UnRetire(Organisation org, long byUserId, string details = null);

        /// <summary>
        ///     Returns Sector followed by list of SicCodes
        /// </summary>
        /// <param name="dataRepository"></param>
        /// <param name="sicCodes"></param>
        /// <returns></returns>
        IEnumerable<string> GetSectors(string sicCodes);

        Organisation GetOrganisationById(long organisationId);
        Task<Organisation> GetOrganisationByEmployerReferenceAsync(string employerReference);

        Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
            string employerReference,
            string securityCode);

        Task<Organisation> GetOrganisationByEmployerReferenceOrThrowAsync(string employerReference);

        public delegate CustomResult<Organisation> ActionSecurityCodeDelegate(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> CreateSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> CreateSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExtendSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> ExtendSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExpireSecurityCodeAsync(string employerRef);
        Task<CustomBulkResult<Organisation>> ExpireSecurityCodesInBulkAsync();
    }
}