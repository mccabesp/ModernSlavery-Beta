using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.Registration
{
    public class OrganisationBusinessLogic : IOrganisationBusinessLogic
    {
        private readonly IDataRepository _DataRepository;
        private readonly IEncryptionHandler _encryptionHandler;
        private readonly IObfuscator _obfuscator;
        private readonly IScopeBusinessLogic _scopeLogic;
        private readonly ISecurityCodeBusinessLogic _securityCodeLogic;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly ISubmissionBusinessLogic _submissionLogic;

        public OrganisationBusinessLogic(
            ISharedBusinessLogic sharedBusinessLogic,
            IDataRepository dataRepo,
            ISubmissionBusinessLogic submissionLogic,
            IScopeBusinessLogic scopeLogic,
            IEncryptionHandler encryptionHandler,
            ISecurityCodeBusinessLogic securityCodeLogic,
            IDnBOrgsRepository dnBOrgsRepository,
            IObfuscator obfuscator)
        {
            _sharedBusinessLogic = sharedBusinessLogic;
            _DataRepository = dataRepo;
            _submissionLogic = submissionLogic;
            _scopeLogic = scopeLogic;
            _obfuscator = obfuscator;
            _securityCodeLogic = securityCodeLogic;
            _encryptionHandler = encryptionHandler;
            DnBOrgsRepository = dnBOrgsRepository;
        }

        public IDnBOrgsRepository DnBOrgsRepository { get; }

        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual async Task<List<OrganisationsFileModel>> GetOrganisationsFileModelByYearAsync(int year)
        {
#if DEBUG
            var orgs = Debugger.IsAttached
                ? _DataRepository.GetAll<Organisation>().Take(100)
                : _DataRepository.GetAll<Organisation>();
#else
            var orgs = _DataRepository.GetAll<Organisation>();
#endif
            var records = new List<OrganisationsFileModel>();

            await foreach (var o in orgs.ToAsyncEnumerable())
            {
                var record = new OrganisationsFileModel
                {
                    OrganisationId = o.OrganisationId,
                    DUNSNumber = o.DUNSNumber,
                    EmployerReference = o.EmployerReference,
                    OrganisationName = o.OrganisationName,
                    CompanyNo = o.CompanyNumber,
                    Sector = o.SectorType,
                    Status = o.Status,
                    StatusDate = o.StatusDate,
                    StatusDetails = o.StatusDetails,
                    Address = o.LatestAddress?.GetAddressString(),
                    SicCodes = GetSicCodeIdsString(o),
                    LatestRegistrationDate = o.LatestRegistration?.PINConfirmedDate,
                    LatestRegistrationMethod = o.LatestRegistration?.Method,
                    Created = o.Created,
                    SecurityCode = o.SecurityCode,
                    SecurityCodeExpiryDateTime = o.SecurityCodeExpiryDateTime,
                    SecurityCodeCreatedDateTime = o.SecurityCodeCreatedDateTime
                };

                var latestReturn =
                    await _submissionLogic.GetLatestSubmissionBySnapshotYearAsync(o.OrganisationId, year);
                var latestScope = await _scopeLogic.GetLatestScopeBySnapshotYearAsync(o.OrganisationId, year);

                record.LatestReturn = latestReturn?.Modified;
                record.ScopeStatus = latestScope?.ScopeStatus;
                record.ScopeDate = latestScope?.ScopeStatusDate;
                records.Add(record);
            }


            return records;
        }

        public virtual async Task SetUniqueEmployerReferencesAsync()
        {
            var orgs = _DataRepository.GetAll<Organisation>().Where(o => o.EmployerReference == null)
                .ToAsyncEnumerable();
            await foreach (var org in orgs) await SetUniqueEmployerReferenceAsync(org);
        }

        public virtual async Task SetUniqueEmployerReferenceAsync(Organisation organisation)
        {
            //Get the unique reference
            do
            {
                organisation.EmployerReference = GenerateEmployerReference();
            } while (await _DataRepository.AnyAsync<Organisation>(o =>
                o.OrganisationId != organisation.OrganisationId &&
                o.EmployerReference == organisation.EmployerReference));

            //Save the organisation
            await _DataRepository.SaveChangesAsync();
        }

        public virtual string GenerateEmployerReference()
        {
            return Crypto.GeneratePasscode(_sharedBusinessLogic.SharedOptions.EmployerCodeChars.ToCharArray(),
                _sharedBusinessLogic.SharedOptions.EmployerCodeLength);
        }

        public virtual string GeneratePINCode(bool isTestUser)
        {
            if (isTestUser) return "ABCDEFG";

            return Crypto.GeneratePasscode(_sharedBusinessLogic.SharedOptions.PinChars.ToCharArray(),
                _sharedBusinessLogic.SharedOptions.PinLength);
        }

        public virtual async Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string employerRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return await _scopeLogic.AddScopeAsync(
                org,
                scopeStatus,
                currentUser,
                changeScopeToSnapshotYear,
                changeScopeToComment,
                saveToDatabase);
        }

        public CustomResult<Organisation> LoadInfoFromEmployerIdentifier(string employerIdentifier)
        {
            var organisationId = _obfuscator.DeObfuscate(employerIdentifier);

            if (organisationId == 0)
                return new CustomResult<Organisation>(
                    InternalMessages.HttpBadRequestCausedByInvalidEmployerIdentifier(employerIdentifier));

            var organisation = GetOrganisationById(organisationId);

            if (organisation == null)
                return new CustomResult<Organisation>(
                    InternalMessages.HttpNotFoundCausedByOrganisationIdNotInDatabase(employerIdentifier));

            return new CustomResult<Organisation>(organisation);
        }


        public virtual CustomResult<Organisation> LoadInfoFromActiveEmployerIdentifier(string employerIdentifier)
        {
            var result = LoadInfoFromEmployerIdentifier(employerIdentifier);

            if (!result.Failed && !result.Result.IsActive())
                return new CustomResult<Organisation>(
                    InternalMessages.HttpGoneCausedByOrganisationBeingInactive(result.Result.Status));

            return result;
        }


        public async Task<CustomResult<Organisation>> GetOrganisationByEncryptedReturnIdAsync(string encryptedReturnId)
        {
            var decryptedReturnId = _encryptionHandler.DecryptAndDecode(encryptedReturnId);

            var result = await _submissionLogic.GetSubmissionByReturnIdAsync(decryptedReturnId.ToInt64());

            if (result == null)
                return new CustomResult<Organisation>(
                    InternalMessages.HttpNotFoundCausedByReturnIdNotInDatabase(encryptedReturnId));

            var organisation = GetOrganisationById(result.OrganisationId);

            return new CustomResult<Organisation>(organisation);
        }

        private async Task<IEnumerable<Organisation>> GetAllActiveOrPendingOrganisationsOrThrowAsync()
        {
            var orgList = _DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Pending);

            if (!orgList.Any())
                throw new Exception("Unable to find organisations with statuses 'Active' or 'Pending' in the database");

            return orgList;
        }


        #region Entity

        public string GetSicSectorsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ")
        {
            var organisationSicCodes = GetSicCodes(org, maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSection.Description.Trim())
                .UniqueI()
                .OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public IEnumerable<OrganisationSicCode> GetSicCodes(Organisation org, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
                maxDate = _sharedBusinessLogic.GetAccountingStartDate(org.SectorType).AddYears(1);

            return org.OrganisationSicCodes.Where(s =>
                s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value));
        }

        public SortedSet<int> GetSicCodeIds(Organisation org, DateTime? maxDate = null)
        {
            var organisationSicCodes = GetSicCodes(org, maxDate);

            var codes = new SortedSet<int>();
            foreach (var sicCode in organisationSicCodes) codes.Add(sicCode.SicCodeId);

            return codes;
        }


        public string GetSicSource(Organisation org, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
                maxDate = _sharedBusinessLogic.GetAccountingStartDate(org.SectorType).AddYears(1);

            return org.OrganisationSicCodes
                .FirstOrDefault(
                    s => s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value))
                ?.Source;
        }

        public string GetSicCodeIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ")
        {
            return GetSicCodes(org, maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId)
                .ToDelimitedString(delimiter);
        }

        public string GetSicSectionIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ")
        {
            var organisationSicCodes = GetSicCodes(org, maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSectionId).UniqueI().OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }


        public AddressModel GetAddressModel(Organisation org, DateTime? maxDate = null,
            AddressStatuses status = AddressStatuses.Active)
        {
            var address = org.GetAddress(maxDate, status);

            return address == null ? null : AddressModel.Create(address);
        }

        public CustomError UnRetire(Organisation org, long byUserId, string details = null)
        {
            if (org.Status != OrganisationStatuses.Retired)
                return InternalMessages.OrganisationRevertOnlyRetiredErrorMessage(org.OrganisationName,
                    org.EmployerReference, org.Status.ToString());

            org.RevertToLastStatus(byUserId, details);
            return null;
        }

        /// <summary>
        ///     Returns Sector followed by list of SicCodes
        /// </summary>
        /// <param name="dataRepository"></param>
        /// <param name="sicCodes"></param>
        /// <returns></returns>
        public IEnumerable<string> GetSectors(string sicCodes)
        {
            var results = new SortedDictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sicCodes)) yield break;

            foreach (var sicCode in sicCodes.SplitI(@";, \n\r\t"))
            {
                var code = sicCode.ToInt64();
                if (code < 1) continue;

                var sic = _DataRepository.GetAll<SicCode>().FirstOrDefault(s => s.SicCodeId == code);
                var sector = sic == null ? "Other" : sic.SicSection.Description;
                var sics = results.ContainsKey(sector) ? results[sector] : new HashSet<long>();
                sics.Add(code);
                results[sector] = sics;
            }

            foreach (var sector in results.Keys)
            {
                if (results[sector].Count == 0) continue;

                yield return $"{sector} ({results[sector].ToDelimitedString(", ")})";
            }
        }

        #endregion

        #region Repo

        public virtual Organisation GetOrganisationById(long organisationId)
        {
            return _DataRepository.Get<Organisation>(organisationId);
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAsync(string employerReference)
        {
            return await _DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference.ToUpper() == employerReference.ToUpper());
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
            string employerReference,
            string securityCode)
        {
            return await _DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference.ToUpper() == employerReference.ToUpper() && o.SecurityCode == securityCode);
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceOrThrowAsync(string employerReference)
        {
            var org = await GetOrganisationByEmployerReferenceAsync(employerReference);

            if (org == null)
                throw new ArgumentException(
                    $"Cannot find organisation with employerReference {employerReference}",
                    nameof(employerReference));

            return org;
        }

        #endregion

        #region Security Codes

        private async Task<CustomBulkResult<Organisation>> ActionSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime,
            IOrganisationBusinessLogic.ActionSecurityCodeDelegate actionSecurityCodeDelegate)
        {
            var listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync();

            var concurrentBagOfProcessedOrganisations = new ConcurrentBag<Organisation>();
            var concurrentBagOfErrors = new ConcurrentBag<CustomResult<Organisation>>();

            Parallel.ForEach(
                listOfOrganisations,
                organisation =>
                {
                    var securityCodeCreationResult =
                        actionSecurityCodeDelegate(organisation, securityCodeExpiryDateTime);

                    if (securityCodeCreationResult.Failed)
                        concurrentBagOfErrors.Add(securityCodeCreationResult);
                    else
                        concurrentBagOfProcessedOrganisations.Add(securityCodeCreationResult.Result);
                });

            return new CustomBulkResult<Organisation>
            {
                ConcurrentBagOfSuccesses = concurrentBagOfProcessedOrganisations,
                ConcurrentBagOfErrors = concurrentBagOfErrors
            };
        }

        public async Task<CustomResult<Organisation>> CreateSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.CreateSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> CreateSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.CreateSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExtendSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExtendSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> ExtendSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.ExtendSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExpireSecurityCodeAsync(string employerRef)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExpireSecurityCode(org);
        }

        public async Task<CustomBulkResult<Organisation>> ExpireSecurityCodesInBulkAsync()
        {
            var listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync();

            var concurrentBagOfProcessedOrganisations = new ConcurrentBag<Organisation>();
            var concurrentBagOfErrors = new ConcurrentBag<CustomResult<Organisation>>();

            Parallel.ForEach(
                listOfOrganisations,
                organisation =>
                {
                    var securityCodeCreationResult = _securityCodeLogic.ExpireSecurityCode(organisation);

                    if (securityCodeCreationResult.Failed)
                        concurrentBagOfErrors.Add(securityCodeCreationResult);
                    else
                        concurrentBagOfProcessedOrganisations.Add(securityCodeCreationResult.Result);
                });

            return new CustomBulkResult<Organisation>
            {
                ConcurrentBagOfSuccesses = concurrentBagOfProcessedOrganisations,
                ConcurrentBagOfErrors = concurrentBagOfErrors
            };
        }

        #endregion
    }
}