using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IOrganisationBusinessLogic
    {
        public delegate CustomResult<Organisation> ActionSecurityCodeDelegate(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        // Organisation repo
        IDnBOrgsRepository DnBOrgsRepository { get; }

        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<List<OrganisationsFileModel>> GetOrganisationFileModelByYearAsync(int year);

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
        string GetOrganisationSicSectorsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");
        string GetOrganisationSicSource(Organisation organisation, DateTime? maxDate = null);
        string GetOrganisationSicSectionIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");

        AddressModel GetOrganisationAddressModel(Organisation org, DateTime? maxDate = null,
            AddressStatuses status = AddressStatuses.Active);

        CustomError UnRetire(Organisation org, long byUserId, string details = null);

        /// <summary>
        ///     Returns Sector followed by list of SicCodes
        /// </summary>
        /// <param name="dataRepository"></param>
        /// <param name="sicCodes"></param>
        /// <returns></returns>
        IEnumerable<string> GetOrganisationSectors(string sicCodes);

        Organisation GetOrganisationById(long organisationId);
        IEnumerable<Return> GetOrganisationRecentReports(Organisation organisation,int recentCount);

        EmployerSearchModel CreateEmployerSearchModel(Organisation organisation, bool keyOnly = false,
            List<SicCodeSearchModel> listOfSicCodeSearchModels = null);

        EmployerRecord CreateEmployerRecord(Organisation org, long userId = 0);

        IEnumerable<int> GetOrganisationRecentReportingYears(Organisation organisation,int recentCount);
        bool GetOrganisationIsOrphan(Organisation organisation);

        bool GetOrganisationIsDissolved(Organisation organisation);

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        OrganisationName GetOrganisationName(Organisation organisation, DateTime? maxDate = null);

        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        OrganisationAddress GetOrganisationAddress(Organisation organisation, DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active);

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        string GetOrganisationAddressString(Organisation organisation, DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active,
            string delimiter = ", ");

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        IEnumerable<OrganisationSicCode> GetOrganisationSicCodes(Organisation organisation,DateTime? maxDate = null);

        SortedSet<int> GetOrganisationSicCodeIds(Organisation organisation, DateTime? maxDate = null);
        string GetOrganisationSicCodeIdsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ");
        Task<Organisation> GetOrganisationByEmployerReferenceAsync(string employerReference);

        Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
            string employerReference,
            string securityCode);

        Return GetOrganisationReturn(Organisation organisation, int year = 0);
        Task<Organisation> GetOrganisationByEmployerReferenceOrThrowAsync(string employerReference);

        Task<CustomResult<Organisation>> CreateOrganisationSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> CreateOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExtendOrganisationSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> ExtendOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExpireOrganisationSecurityCodeAsync(string employerRef);
        Task<CustomBulkResult<Organisation>> ExpireOrganisationSecurityCodesInBulkAsync();
        IQueryable<Organisation> SearchOrganisations(string searchText,int records);
    }
}