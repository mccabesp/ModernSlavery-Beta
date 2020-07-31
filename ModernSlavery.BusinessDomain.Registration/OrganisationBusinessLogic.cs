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

namespace ModernSlavery.BusinessDomain.Registration
{
    public class OrganisationBusinessLogic : IOrganisationBusinessLogic
    {
        private readonly IScopeBusinessLogic _scopeLogic;
        private readonly ISecurityCodeBusinessLogic _securityCodeLogic;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly ISubmissionBusinessLogic _submissionLogic;

        public OrganisationBusinessLogic(
            ISharedBusinessLogic sharedBusinessLogic,
            ISubmissionBusinessLogic submissionLogic,
            IScopeBusinessLogic scopeLogic,
            ISecurityCodeBusinessLogic securityCodeLogic)
        {
            _sharedBusinessLogic = sharedBusinessLogic;
            _submissionLogic = submissionLogic;
            _scopeLogic = scopeLogic;
            _securityCodeLogic = securityCodeLogic;
        }

        public IQueryable<Organisation> SearchOrganisations(string searchText,int records)
        {
            var searchData = _sharedBusinessLogic.DataRepository.GetAll<Organisation>();
            var levenshteinRecords =searchData.ToList().Select(o => new{ distance = o.OrganisationName.LevenshteinCompute(searchText), org = o });
            var pattern = searchText?.ToLower();

            var searchResults = levenshteinRecords.AsQueryable()
                .Where(
                    data => data.org.OrganisationName.ToLower().Contains(pattern)
                            || data.org.OrganisationName.Length > _sharedBusinessLogic.SharedOptions.LevenshteinDistance &&
                            data.distance <= _sharedBusinessLogic.SharedOptions.LevenshteinDistance)
                .OrderBy(o => o.distance)
                .Take(records)
                .Select(o => o.org);
            return searchResults;
        }

        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual async Task<List<OrganisationsFileModel>> GetOrganisationFileModelByYearAsync(int year)
        {
#if DEBUG
            var orgs = Debugger.IsAttached
                ? _sharedBusinessLogic.DataRepository.GetAll<Organisation>().Take(100)
                : _sharedBusinessLogic.DataRepository.GetAll<Organisation>();
#else
            var orgs = _sharedBusinessLogic.DataRepository.GetAll<Organisation>();
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
                    SicCodes = GetOrganisationSicCodeIdsString(o),
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
            var orgs = _sharedBusinessLogic.DataRepository.GetAll<Organisation>().Where(o => o.EmployerReference == null)
                .ToAsyncEnumerable();
            await foreach (var org in orgs) await SetUniqueEmployerReferenceAsync(org);
        }

        public virtual async Task SetUniqueEmployerReferenceAsync(Organisation organisation)
        {
            //Get the unique reference
            do
            {
                organisation.EmployerReference = GenerateEmployerReference();
            } while (await _sharedBusinessLogic.DataRepository.AnyAsync<Organisation>(o =>
                o.OrganisationId != organisation.OrganisationId &&
                o.EmployerReference == organisation.EmployerReference));

            //Save the organisation
            await _sharedBusinessLogic.DataRepository.SaveChangesAsync();
        }

        public virtual string GenerateEmployerReference()
        {
            return Crypto.GeneratePasscode(_sharedBusinessLogic.SharedOptions.EmployerCodeChars.ToCharArray(),
                _sharedBusinessLogic.SharedOptions.EmployerCodeLength);
        }

        public virtual string GeneratePINCode()
        {
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
            var organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(employerIdentifier);

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
            var decryptedReturnId = _sharedBusinessLogic.Obfuscator.DeObfuscate(encryptedReturnId);

            var result = await _submissionLogic.GetSubmissionByReturnIdAsync(decryptedReturnId.ToInt64());

            if (result == null)
                return new CustomResult<Organisation>(
                    InternalMessages.HttpNotFoundCausedByReturnIdNotInDatabase(encryptedReturnId));

            var organisation = GetOrganisationById(result.OrganisationId);

            return new CustomResult<Organisation>(organisation);
        }

        private async Task<IEnumerable<Organisation>> GetAllActiveOrPendingOrganisationsOrThrowAsync()
        {
            var orgList = _sharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Pending);

            if (!orgList.Any())
                throw new Exception("Unable to find organisations with statuses 'Active' or 'Pending' in the database");

            return orgList;
        }


        #region Entity

        public string GetOrganisationSicSectorsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ")
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1);

            return organisation.GetSicSectorsString(maxDate.Value, delimiter);
        }


        public string GetOrganisationSicSource(Organisation organisation, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1);

            return organisation.GetSicSource(maxDate.Value);
        }

        public string GetOrganisationSicSectionIdsString(Organisation org, DateTime? maxDate = null, string delimiter = ", ")
        {
            var organisationSicCodes = GetOrganisationSicCodes(org, maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSectionId).UniqueI().OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        public AddressModel GetOrganisationAddressModel(Organisation org, DateTime? maxDate = null,
            AddressStatuses status = AddressStatuses.Active)
        {
            var address = GetOrganisationAddress(org, maxDate, status);

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
        public IEnumerable<string> GetOrganisationSectors(string sicCodes)
        {
            var results = new SortedDictionary<string, HashSet<long>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(sicCodes)) yield break;

            foreach (var sicCode in sicCodes.SplitI(@";, \n\r\t"))
            {
                var code = sicCode.ToInt64();
                if (code < 1) continue;

                var sic = _sharedBusinessLogic.DataRepository.GetAll<SicCode>().FirstOrDefault(s => s.SicCodeId == code);
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
            return _sharedBusinessLogic.DataRepository.Get<Organisation>(organisationId);
        }

        public IEnumerable<Return> GetOrganisationRecentReports(Organisation organisation,int recentCount)
        {
            foreach (var year in GetOrganisationRecentReportingYears(organisation,recentCount))
            {
                var defaultReturn = new Return
                {
                    Organisation = organisation,
                    AccountingDate =_sharedBusinessLogic.GetReportingStartDate(organisation.SectorType,year),
                    Modified = VirtualDateTime.Now
                };
                defaultReturn.IsLateSubmission = defaultReturn.CalculateIsLateSubmission();

                yield return  organisation.GetReturn(year) ?? defaultReturn;
            }
        }
        public EmployerRecord CreateEmployerRecord(Organisation org, long userId = 0)
        {
            OrganisationAddress address = null;
            if (userId > 0) address = org.UserOrganisations.FirstOrDefault(uo => uo.UserId == userId)?.Address;

            if (address == null) address = org.LatestAddress;

            if (address == null)
                return new EmployerRecord
                {
                    OrganisationId = org.OrganisationId,
                    SectorType = org.SectorType,
                    OrganisationName = org.OrganisationName,
                    NameSource = GetOrganisationName(org)?.Source,
                    EmployerReference = org.EmployerReference,
                    DateOfCessation = org.DateOfCessation,
                    DUNSNumber = org.DUNSNumber,
                    CompanyNumber = org.CompanyNumber,
                    SicSectors = GetOrganisationSicSectorsString(org,null, ",<br/>"),
                    SicCodeIds = GetOrganisationSicCodeIdsString(org),
                    SicSource = GetOrganisationSicSource(org),
                    RegistrationStatus = org.GetRegistrationStatus(),
                    References = org.OrganisationReferences.ToDictionary(
                        r => r.ReferenceName,
                        r => r.ReferenceValue,
                        StringComparer.OrdinalIgnoreCase)
                };

            return new EmployerRecord
            {
                OrganisationId = org.OrganisationId,
                SectorType = org.SectorType,
                OrganisationName = org.OrganisationName,
                NameSource = GetOrganisationName(org)?.Source,
                EmployerReference = org.EmployerReference,
                DateOfCessation = org.DateOfCessation,
                DUNSNumber = org.DUNSNumber,
                CompanyNumber = org.CompanyNumber,
                SicSectors = GetOrganisationSicSectorsString(org,null, ",<br/>"),
                SicCodeIds = GetOrganisationSicCodeIdsString(org),
                SicSource = GetOrganisationSicSource(org),
                ActiveAddressId = address.AddressId,
                AddressSource = address.Source,
                Address1 = address.Address1,
                Address2 = address.Address2,
                Address3 = address.Address3,
                City = address.TownCity,
                County = address.County,
                Country = address.Country,
                PostCode = address.PostCode,
                PoBox = address.PoBox,
                IsUkAddress = address.IsUkAddress,
                RegistrationStatus = org.GetRegistrationStatus(),
                References = org.OrganisationReferences.ToDictionary(
                    r => r.ReferenceName,
                    r => r.ReferenceValue,
                    StringComparer.OrdinalIgnoreCase)
            };
        }
        public EmployerSearchModel CreateEmployerSearchModel(Organisation organisation, bool keyOnly = false,
            List<SicCodeSearchModel> listOfSicCodeSearchModels = null)
        {
            if (keyOnly) return new EmployerSearchModel { OrganisationId = organisation.OrganisationId.ToString() };


            // Get the last two names for the org. Most recent name first
            var names = organisation.OrganisationNames.Select(n => n.Name).Reverse().Take(2).ToArray();

            var abbreviations = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            names.ForEach(n => abbreviations.Add(n.ToAbbr()));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".")));
            var excludes = new[]
                {"Ltd", "Limited", "PLC", "Corporation", "Incorporated", "LLP", "The", "And", "&", "For", "Of", "To"};
            names.ForEach(n => abbreviations.Add(n.ToAbbr(excludeWords: excludes)));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".", excludeWords: excludes)));

            abbreviations.RemoveWhere(a => string.IsNullOrWhiteSpace(a));
            abbreviations.Remove(organisation.OrganisationName);

            // extract the prev org name (if exists)
            var prevOrganisationName = "";
            if (names.Length > 1)
            {
                prevOrganisationName = names[names.Length - 1];
                abbreviations.Remove(prevOrganisationName);
            }

            //Get the latest sic codes
            var sicCodes = GetOrganisationSicCodes(organisation);

            var submittedStatements = organisation.GetSubmittedStatements().ToArray();

            var result = new EmployerSearchModel
            {
                OrganisationId = organisation.OrganisationId.ToString(),
                OrganisationIdEncrypted = _sharedBusinessLogic.Obfuscator.Obfuscate(organisation.OrganisationId),
                Name = organisation.OrganisationName,
                PreviousName = prevOrganisationName,
                PartialNameForSuffixSearches = organisation.OrganisationName,
                PartialNameForCompleteTokenSearches = organisation.OrganisationName,
                Abbreviations = abbreviations.ToArray(),
                Size = 0,
                SicSectionIds = sicCodes.Select(sic => sic.SicCode.SicSectionId.ToString()).Distinct().ToArray(),
                SicSectionNames = sicCodes.Select(sic => sic.SicCode.SicSection.Description).Distinct().ToArray(),
                SicCodeIds = sicCodes.Select(sicCode => sicCode.SicCodeId.ToString()).Distinct().ToArray(),
                Address = organisation.LatestAddress?.GetAddressString(),
                LatestReportedDate = submittedStatements.Select(x => x.Created).FirstOrDefault(),
                ReportedYears = submittedStatements.Select(x => x.SubmissionDeadline.Year.ToString()).ToArray(),
                ReportedLateYears =
                    submittedStatements.Where(x => x.CalculateIsLateSubmission()).Select(x => x.SubmissionDeadline.Year.ToString())
                        .ToArray(),
                ReportedExplanationYears = submittedStatements
                    .Where(x => string.IsNullOrEmpty(x.StatementUrl) == false)
                    .Select(x => x.SubmissionDeadline.Year.ToString())
                    .ToArray()
            };

            if (listOfSicCodeSearchModels != null)
                result.SicCodeListOfSynonyms = GetListOfSynonyms(result.SicCodeIds, listOfSicCodeSearchModels);

            return result;
        }

        private string[] GetListOfSynonyms(string[] resultSicCodeIds,
            List<SicCodeSearchModel> listOfSicCodeSearchModels)
        {
            var result = new List<string>();

            foreach (var resultSicCodeId in resultSicCodeIds)
            {
                var sicCodeSearchModel = listOfSicCodeSearchModels.FirstOrDefault(x => x.SicCodeId == resultSicCodeId);

                if (sicCodeSearchModel == null) continue;

                result.Add(sicCodeSearchModel.SicCodeDescription);

                if (sicCodeSearchModel.SicCodeListOfSynonyms != null &&
                    sicCodeSearchModel.SicCodeListOfSynonyms.Length > 0)
                    result.AddRange(sicCodeSearchModel.SicCodeListOfSynonyms);
            }

            return result.Any()
                ? result.ToArray()
                : null;
        }

        public IEnumerable<int> GetOrganisationRecentReportingYears(Organisation organisation,int recentCount)
        {
            var endYear = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).Year;
            var startYear = endYear - (recentCount - 1);
            if (startYear < _sharedBusinessLogic.SharedOptions.FirstReportingYear) startYear = _sharedBusinessLogic.SharedOptions.FirstReportingYear;

            for (var year = endYear; year >= startYear; year--) yield return year;
        }

        public bool GetOrganisationIsOrphan(Organisation organisation)
        {
            return organisation.Status == OrganisationStatuses.Active
                   && (organisation.LatestScope.ScopeStatus == ScopeStatuses.InScope ||
                       organisation.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                   && (organisation.UserOrganisations == null
                       || !organisation.UserOrganisations.Any(uo => uo.PINConfirmedDate != null
                                                                   || uo.Method ==
                                                                   RegistrationMethods.Manual
                                                                   || uo.Method ==
                                                                   RegistrationMethods.PinInPost
                                                                   && uo.PINSentDate.HasValue
                                                                   && uo.PINSentDate.Value >
                                                                   _sharedBusinessLogic.SharedOptions.PinExpiresDate));
        }

        public bool GetOrganisationWasDissolvedBeforeCurrentAccountingYear(Organisation organisation)
        {
            var accountingStartDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType);
            return organisation.GetWasDissolvedBefore(accountingStartDate);
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public OrganisationName GetOrganisationName(Organisation organisation, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate =_sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1);

            return organisation.GetName(maxDate.Value);
        }

        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        public OrganisationAddress GetOrganisationAddress(Organisation organisation, DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1);

            if (status == AddressStatuses.Active && organisation.LatestAddress != null &&
                maxDate == _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1)) return organisation.LatestAddress;

            return organisation.GetAddress(maxDate.Value);
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public string GetOrganisationAddressString(Organisation organisation, DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active,
            string delimiter = ", ")
        {
            var address = GetOrganisationAddress(organisation, maxDate, status);

            return address?.GetAddressString(delimiter);
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public IEnumerable<OrganisationSicCode> GetOrganisationSicCodes(Organisation organisation,DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).AddYears(1);

            return organisation.OrganisationSicCodes.Where(s =>
                s.Created < maxDate && (s.Retired == null || s.Retired.Value > maxDate));
        }

        public SortedSet<int> GetOrganisationSicCodeIds(Organisation organisation, DateTime? maxDate = null)
        {
            var organisationSicCodes = GetOrganisationSicCodes(organisation,maxDate);

            var codes = new SortedSet<int>();
            foreach (var sicCode in organisationSicCodes) codes.Add(sicCode.SicCodeId);

            return codes;
        }
        public string GetOrganisationSicCodeIdsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ")
        {
            return GetOrganisationSicCodes(organisation, maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString(delimiter);
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAsync(string employerReference)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference.ToUpper() == employerReference.ToUpper());
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(
            string employerReference,
            string securityCode)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference.ToUpper() == employerReference.ToUpper() && o.SecurityCode == securityCode);
        }

        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Return GetOrganisationReturn(Organisation organisation, int year = 0)
        {
            var accountingStartDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType,year);
            return organisation.GetReturn(year);
        }

        //Returns the latest scope for the current accounting date
        public OrganisationScope GetOrganisationCurrentScope(Organisation organisation)
        {
            var accountingStartDate = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType);

            return organisation.GetScope(accountingStartDate);
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

        public async Task<CustomResult<Organisation>> CreateOrganisationSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.CreateSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> CreateOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.CreateSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExtendOrganisationSecurityCodeAsync(string employerRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExtendSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> ExtendOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.ExtendSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExpireOrganisationSecurityCodeAsync(string employerRef)
        {
            var org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExpireSecurityCode(org);
        }

        public async Task<CustomBulkResult<Organisation>> ExpireOrganisationSecurityCodesInBulkAsync()
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