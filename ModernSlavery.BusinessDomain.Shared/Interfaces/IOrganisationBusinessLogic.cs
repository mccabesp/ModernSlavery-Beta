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
        IDataRepository DataRepository { get; }

        public delegate CustomResult<Organisation> ActionSecurityCodeDelegate(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        // Organisation repo
        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<List<OrganisationsFileModel>> GetOrganisationFileModelByYearAsync(int year);

        /// <summary>
        ///     Get all statements that have been submitted for an organisation.
        ///     Includes group and single statements for all years.
        /// </summary>
        IEnumerable<Statement> GetAllStatements(Organisation organisation);

        Task SetUniqueOrganisationReferencesAsync();
        Task SetUniqueOrganisationReferenceAsync(Organisation organisation);
        string GenerateOrganisationReference();
        string GeneratePINCode();

        Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string organisationRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase);

        CustomResult<Organisation> LoadInfoFromOrganisationId(long organisationId);
        CustomResult<Organisation> LoadInfoFromActiveOrganisationId(long organisationId);
        string GetOrganisationSicSectorsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");
        string GetOrganisationSicSource(Organisation organisation, DateTime? maxDate = null);
        string GetOrganisationSicSectionIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ");

        CustomError UnRetire(Organisation org, long byUserId, string details = null);

        /// <summary>
        ///     Returns Sector followed by list of SicCodes
        /// </summary>
        /// <param name="dataRepository"></param>
        /// <param name="sicCodes"></param>
        /// <returns></returns>
        IEnumerable<string> GetOrganisationSectors(string sicCodes);

        Organisation GetOrganisationById(long organisationId);
        IEnumerable<Statement> GetOrganisationRecentStatements(Organisation organisation,int recentCount);

        OrganisationRecord CreateOrganisationRecord(Organisation org, bool detailed, long userId = 0);

        IEnumerable<OrganisationRecord> CreateOrganisationRecords(IEnumerable<Organisation> orgs, bool detailed, long userId = 0);

        bool GetOrganisationIsOrphan(Organisation organisation);

        bool GetOrganisationWasDissolvedBeforeCurrentAccountingYear(Organisation organisation);

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        OrganisationName GetOrganisationName(Organisation organisation, DateTime? maxDate = null);

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        IEnumerable<OrganisationSicCode> GetOrganisationSicCodes(Organisation organisation,DateTime? maxDate = null);

        SortedSet<int> GetOrganisationSicCodeIds(Organisation organisation, DateTime? maxDate = null);
        string GetOrganisationSicCodeIdsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ");
        Task<Organisation> GetOrganisationByOrganisationReferenceAsync(string organisationReference);

        Task<Organisation> GetOrganisationByOrganisationReferenceAndSecurityCodeAsync(
            string organisationReference,
            string securityCode);

        Statement GetOrganisationStatement(Organisation organisation, int year = 0);

        Task<CustomResult<Organisation>> CreateOrganisationSecurityCodeAsync(string organisationRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> CreateOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExtendOrganisationSecurityCodeAsync(string organisationRef,
            DateTime securityCodeExpiryDateTime);

        Task<CustomBulkResult<Organisation>> ExtendOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime);

        Task<CustomResult<Organisation>> ExpireOrganisationSecurityCodeAsync(string organisationRef);
        Task<CustomBulkResult<Organisation>> ExpireOrganisationSecurityCodesInBulkAsync();
        IQueryable<Organisation> SearchOrganisations(string searchText,int records);
        Task FixLatestAddressesAsync();
        Task FixLatestScopesAsync();
        Task FixLatestStatementsAsync();
        Task FixLatestRegistrationsAsync();
        /// <summary>
        /// Save bad sic codes for an organisation to log file
        /// </summary>
        /// <param name="organisation">The organisation with the bad sic codes</param>
        /// <param name="badSicCodes">The list of bad sic code ids</param>
        /// <returns></returns>
        Task LogBadSicCodesAsync(Organisation organisation, SortedSet<int> badSicCodes);

        /// <summary>
        /// Create a new (detached) organisation with address, SicCodes and statuses
        /// </summary>
        /// <param name="organisationName">The name of the organisation</param>
        /// <param name="source">The source of the organisation (eg., CoHo, External, (user email) etc)</param>
        /// <param name="sectorType">The sector type of the organisation (ie., Private or Public)</param>
        /// <param name="addressModel">The address of the organisation</param>
        /// <param name="addressStatus">The status of the address (ie., Active or Pending (default))</param>
        /// <param name="status">The status of the organisation (ie., Active (default) or Pending)</param>
        /// <param name="companyNumber">The company number of the organisation</param>
        /// <param name="dateOfCessation">The date the company ceased trading</param>
        /// <param name="references">A list of unique organisation references (eg., Charity number) </param>
        /// <param name="sicCodes">A list of SIC code ids</param>
        /// <param name="user">The user who created the organisation. If empty a userId of 0 (system) will be assumed</param>
        /// <returns>The new detached organisation entity ready for saving</returns>
        Organisation CreateOrganisation(string organisationName, string source, SectorTypes sectorType, OrganisationStatuses status, AddressModel addressModel, AddressStatuses addressStatus=AddressStatuses.Pending, string companyNumber = null, DateTime? dateOfCessation = null, Dictionary<string, string> references = null, SortedSet<int> sicCodes = null, long userId = -1);

        /// <summary>
        /// Save a new or existing organisation with OrganisationReference, presumed scopes, LatestScope, LatestAddress, LatestStatement, LatestRegistration
        /// </summary>
        /// <param name="organisation">The organisation to save</param>
        /// <returns></returns>
        Task SaveOrganisationAsync(Organisation organisation);
    }
}