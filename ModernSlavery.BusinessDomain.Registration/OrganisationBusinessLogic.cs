using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using User = ModernSlavery.Core.Entities.User;

namespace ModernSlavery.BusinessDomain.Registration
{
    public class OrganisationBusinessLogic : IOrganisationBusinessLogic
    {
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        private readonly IScopeBusinessLogic _scopeLogic;
        private readonly ISecurityCodeBusinessLogic _securityCodeLogic;
        private readonly ISubmissionBusinessLogic _submissionLogic;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly IAuditLogger _badSicLog;
        public OrganisationBusinessLogic(SharedOptions sharedOptions,
            IDataRepository dataRepository, IReportingDeadlineHelper reportingDeadlineHelper,
            ISubmissionBusinessLogic submissionLogic,
            IScopeBusinessLogic scopeLogic,
            ISecurityCodeBusinessLogic securityCodeLogic,
            [KeyFilter(Filenames.BadSicLog)] IAuditLogger badSicLog)
        {
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _submissionLogic = submissionLogic;
            _scopeLogic = scopeLogic;
            _securityCodeLogic = securityCodeLogic;
            _badSicLog = badSicLog;
        }

        public IQueryable<Organisation> SearchOrganisations(string searchText, int records)
        {
            var searchData = _dataRepository.GetAll<Organisation>();
            var levenshteinRecords = searchData.ToList().Select(o => new { distance = o.OrganisationName.LevenshteinCompute(searchText), org = o });
            var pattern = searchText?.ToLower();

            var searchResults = levenshteinRecords.AsQueryable()
                .Where(
                    data => data.org.OrganisationName.ToLower().Contains(pattern)
                            || data.org.OrganisationName.Length > _sharedOptions.LevenshteinDistance &&
                            data.distance <= _sharedOptions.LevenshteinDistance)
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
                ? _dataRepository.GetAll<Organisation>().Take(100)
                : _dataRepository.GetAll<Organisation>();
#else
            var orgs = _dataRepository.GetAll<Organisation>();
#endif
            var records = new List<OrganisationsFileModel>();

            foreach (var o in orgs)
            {
                var record = new OrganisationsFileModel
                {
                    OrganisationId = o.OrganisationId,
                    DUNSNumber = o.DUNSNumber,
                    OrganisationReference = o.OrganisationReference,
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

                var latestReturn = await _submissionLogic.GetLatestStatementBySnapshotYearAsync(o.OrganisationId, year);
                var reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(o.SectorType, year);
                var latestScope = await _scopeLogic.GetScopeByReportingDeadlineOrLatestAsync(o.OrganisationId, reportingDeadline);

                record.LatestReturn = latestReturn?.Modified;
                record.ScopeStatus = latestScope?.ScopeStatus;
                record.ScopeDate = latestScope?.ScopeStatusDate;
                records.Add(record);
            }


            return records;
        }

        public virtual async Task SetUniqueOrganisationReferencesAsync()
        {
            var orgs = await _dataRepository.ToListAsync<Organisation>(o => o.OrganisationReference == null);
            foreach (var org in orgs) await SetUniqueOrganisationReferenceAsync(org);
            await _dataRepository.BulkUpdateAsync(orgs);
        }

        public virtual async Task SetUniqueOrganisationReferenceAsync(Organisation organisation)
        {
            //Get the unique reference
            do
            {
                organisation.OrganisationReference = GenerateOrganisationReference();
            } while (await _dataRepository.AnyAsync<Organisation>(o =>
                o.OrganisationId != organisation.OrganisationId &&
                o.OrganisationReference == organisation.OrganisationReference));
        }

        public virtual string GenerateOrganisationReference()
        {
            return Crypto.GeneratePasscode(_sharedOptions.OrganisationCodeChars.ToCharArray(),
                _sharedOptions.OrganisationCodeLength);
        }

        public virtual string GeneratePINCode()
        {
            return Crypto.GeneratePasscode(_sharedOptions.PinChars.ToCharArray(),
                _sharedOptions.PinLength);
        }

        public virtual async Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string organisationRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef);
            var reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(org.SectorType, changeScopeToSnapshotYear);
            return await _scopeLogic.AddScopeAsync(
                org,
                scopeStatus,
                currentUser,
                reportingDeadline,
                changeScopeToComment,
                saveToDatabase);
        }

        public CustomResult<Organisation> LoadInfoFromOrganisationId(long organisationId)
        {
            var organisation = GetOrganisationById(organisationId);

            if (organisation == null) return new CustomResult<Organisation>(InternalMessages.HttpNotFoundCausedByOrganisationIdNotInDatabase(organisationId.ToString()));

            return new CustomResult<Organisation>(organisation);
        }


        public virtual CustomResult<Organisation> LoadInfoFromActiveOrganisationId(long organisationId)
        {
            var result = LoadInfoFromOrganisationId(organisationId);

            if (!result.Failed && !result.Result.IsActive())
                return new CustomResult<Organisation>(
                    InternalMessages.HttpGoneCausedByOrganisationBeingInactive(result.Result.Status));

            return result;
        }

        private async Task<IEnumerable<Organisation>> GetAllActiveOrPendingOrganisationsOrThrowAsync()
        {
            var orgList = _dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Pending);

            if (!orgList.Any())
                throw new Exception("Unable to find organisations with statuses 'Active' or 'Pending' in the database");

            return orgList;
        }

        public Organisation CreateOrganisation(string organisationName, string source, SectorTypes sectorType, OrganisationStatuses status, AddressModel addressModel, AddressStatuses addressStatus= AddressStatuses.Pending, string companyNumber = null, DateTime? dateOfCessation = null, Dictionary<string, string> references = null, SortedSet<int> sicCodes = null, long userId = -1)
        {
            #region Check the parameters
            if (string.IsNullOrWhiteSpace(organisationName)) throw new ArgumentNullException(nameof(organisationName));
            if (sectorType == SectorTypes.Unknown) throw new ArgumentOutOfRangeException(nameof(sectorType));
            if (!status.IsAny(OrganisationStatuses.Active, OrganisationStatuses.Pending)) throw new ArgumentOutOfRangeException(nameof(status),$"Organisation status must be active or pending but was {status}");
            if (addressModel == null || addressModel.IsEmpty()) throw new ArgumentNullException(nameof(addressModel), "Cannot save an organisation with no address");
            #endregion

            #region Create the basic organisation
            var organisation = new Organisation
            {
                SectorType = sectorType,
                CompanyNumber = companyNumber,
                DateOfCessation = dateOfCessation,
            };
            _dataRepository.Insert(organisation);

            //Set the organisation status
            organisation.SetStatus(status, userId);
            #endregion

            #region Add the references
            if (references != null)
            {
                foreach (var key in references.Keys)
                {
                    var value = references[key];
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    var reference = new OrganisationReference
                    {
                        ReferenceName = key,
                        ReferenceValue = value,
                        Organisation = organisation
                    };

                    _dataRepository.Insert(reference);
                    organisation.OrganisationReferences.Add(reference);
                }
            }
            #endregion

            #region Add the SIC codes
            if (organisation.SectorType == SectorTypes.Public) sicCodes.Add(1);

            //Remove invalid SicCodes
            var badSicCodes = new SortedSet<int>();
            if (sicCodes.Count > 0)
            {
                //TODO we should cache these SIC codes
                var allSicCodes = _dataRepository.GetAll<SicCode>().Select(s => s.SicCodeId)
                    .ToSortedSet();
                badSicCodes = sicCodes.Except(allSicCodes).ToSortedSet();
                sicCodes = sicCodes.Except(badSicCodes).ToSortedSet();

                //Update the new and retire the old SIC codes
                if (sicCodes.Count > 0)
                {
                    foreach (var newSicCodeId in sicCodes)
                    {
                        var sicCode = new OrganisationSicCode
                        { Organisation = organisation, SicCodeId = newSicCodeId, Source = source };
                        _dataRepository.Insert(sicCode);
                        organisation.OrganisationSicCodes.Add(sicCode);
                    }
                }
            }

            #endregion

            #region Add the organisation address
            //Use the old address for this registration
            //Create address received from user
            var address = new OrganisationAddress();
            address.Organisation = organisation;
            address.CreatedByUserId = userId;
            address.Address1 = addressModel.Address1;
            address.Address2 = addressModel.Address2;
            address.Address3 = addressModel.Address3;
            address.TownCity = addressModel.City;
            address.County = addressModel.County;
            address.Country = addressModel.Country;
            address.PostCode = addressModel.PostCode;
            address.PoBox = addressModel.PoBox;
            address.IsUkAddress = addressModel.IsUkAddress;
            address.Source = source;
            address.SetStatus(addressStatus, userId);
            _dataRepository.Insert(address);
            #endregion
            
            return organisation;
        }

        public async Task SaveOrganisationAsync(IDataRepository dataRepository, Organisation organisation)
        {
            #region Check the parameters
            if (dataRepository==null) throw new ArgumentNullException(nameof(dataRepository));
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));
            #endregion

            //Check if we are creating a new organisation
            var isnew = organisation.OrganisationId == 0;

            //Save the organisation to ensure it has an OrganisationId
            await dataRepository.SaveChangesAsync();

            //Ensure the organisation has an organisation reference
            if (string.IsNullOrWhiteSpace(organisation.OrganisationReference))
                await SetUniqueOrganisationReferenceAsync(organisation);

            //Create a presumed in-scope for current and previous years
            if (isnew)
            {
                await organisation.SetPresumedScopesAsync(_reportingDeadlineHelper.GetReportingDeadlines(organisation.SectorType));
                await dataRepository.SaveChangesAsync();
            }

            //Set the latest scope
            organisation.FixLatestScope();

            //Set the latest address
            organisation.FixLatestAddress();

            //Set the latest statement
            organisation.FixLatestStatement();

            //Set the latest registration
            organisation.FixLatestRegistration();

            await dataRepository.SaveChangesAsync();
        }

        #region Organisation

        public string GetOrganisationSicSectorsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ")
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1);

            return organisation.GetSicSectorsString(maxDate.Value, delimiter);
        }


        public string GetOrganisationSicSource(Organisation organisation, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1);

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
                    org.OrganisationReference, org.Status.ToString());

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

                var sic = _dataRepository.GetAll<SicCode>().FirstOrDefault(s => s.SicCodeId == code);
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

        #region Sic Codes

        public async Task LogBadSicCodesAsync(Organisation organisation, SortedSet<int> badSicCodes)
        {
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));
            if (organisation.OrganisationId == 0) throw new ArgumentException("Organisation must be saved first before logging bad SIC codes",nameof(organisation));
            if (badSicCodes == null) throw new ArgumentNullException(nameof(badSicCodes));

            //Log the bad sic codes here to ensure organisation identifiers have been created when saved
            if (badSicCodes.Count == 0) return;

            //Create the logging tasks
            var badSicLoggingtasks = new List<Task>();
            badSicCodes.ForEach(
                code => badSicLoggingtasks.Add(
                    _badSicLog.WriteAsync(
                                new BadSicLogModel
                                {
                                    OrganisationId = organisation.OrganisationId,
                                    OrganisationName = organisation.OrganisationName,
                                    SicCode = code,
                                    Source = "CoHo"
                                })));

            //Wait for all the logging tasks to complete
            await Task.WhenAll(badSicLoggingtasks);
        }
        #endregion

        #region Repo

        public virtual Organisation GetOrganisationById(long organisationId)
        {
            return _dataRepository.Get<Organisation>(organisationId);
        }

        public IEnumerable<Statement> GetOrganisationRecentStatements(Organisation organisation, int recentCount)
        {
            foreach (var reportingDeadline in _reportingDeadlineHelper.GetReportingDeadlines(organisation.SectorType, recentCount))
            {
                var defaultStatement = new Statement
                {
                    Organisation = organisation,
                    SubmissionDeadline = reportingDeadline,
                    Modified = VirtualDateTime.Now
                };

                yield return organisation.GetStatement(reportingDeadline) ?? defaultStatement;
            }
        }
        public OrganisationRecord CreateOrganisationRecord(Organisation org, long userId = 0)
        {
            OrganisationAddress address = null;
            if (userId > 0) address = org.UserOrganisations.FirstOrDefault(uo => uo.UserId == userId)?.Address;

            if (address == null) address = org.LatestAddress;

            if (address == null)
                return new OrganisationRecord
                {
                    OrganisationId = org.OrganisationId,
                    SectorType = org.SectorType,
                    OrganisationName = org.OrganisationName,
                    NameSource = GetOrganisationName(org)?.Source,
                    OrganisationReference = org.OrganisationReference,
                    DateOfCessation = org.DateOfCessation,
                    DUNSNumber = org.DUNSNumber,
                    CompanyNumber = org.CompanyNumber,
                    SicSectors = GetOrganisationSicSectorsString(org, null, ",<br/>"),
                    SicCodeIds = GetOrganisationSicCodeIdsString(org),
                    SicSource = GetOrganisationSicSource(org),
                    RegistrationStatus = org.GetRegistrationStatus(),
                    References = org.OrganisationReferences.ToDictionary(
                        r => r.ReferenceName,
                        r => r.ReferenceValue,
                        StringComparer.OrdinalIgnoreCase)
                };

            return new OrganisationRecord
            {
                OrganisationId = org.OrganisationId,
                SectorType = org.SectorType,
                OrganisationName = org.OrganisationName,
                NameSource = GetOrganisationName(org)?.Source,
                OrganisationReference = org.OrganisationReference,
                DateOfCessation = org.DateOfCessation,
                DUNSNumber = org.DUNSNumber,
                CompanyNumber = org.CompanyNumber,
                SicSectors = GetOrganisationSicSectorsString(org, null, ",<br/>"),
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
                                                                   _sharedOptions.PinExpiresDate));
        }

        public bool GetOrganisationWasDissolvedBeforeCurrentAccountingYear(Organisation organisation)
        {

            var accountingStartDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType);
            return organisation.GetWasDissolvedBefore(accountingStartDate);
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public OrganisationName GetOrganisationName(Organisation organisation, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1);

            return organisation.GetName(maxDate.Value);
        }

        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        public OrganisationAddress GetOrganisationAddress(Organisation organisation, DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1);

            if (status == AddressStatuses.Active && organisation.LatestAddress != null &&
                maxDate == _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1)) return organisation.LatestAddress;

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
        public IEnumerable<OrganisationSicCode> GetOrganisationSicCodes(Organisation organisation, DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType).AddYears(1);

            return organisation.OrganisationSicCodes.Where(s =>
                s.Created < maxDate && (s.Retired == null || s.Retired.Value > maxDate));
        }

        public SortedSet<int> GetOrganisationSicCodeIds(Organisation organisation, DateTime? maxDate = null)
        {
            var organisationSicCodes = GetOrganisationSicCodes(organisation, maxDate);

            var codes = new SortedSet<int>();
            foreach (var sicCode in organisationSicCodes) codes.Add(sicCode.SicCodeId);

            return codes;
        }
        public string GetOrganisationSicCodeIdsString(Organisation organisation, DateTime? maxDate = null, string delimiter = ", ")
        {
            return GetOrganisationSicCodes(organisation, maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString(delimiter);
        }

        public virtual async Task<Organisation> GetOrganisationByOrganisationReferenceAsync(string organisationReference)
        {
            return await _dataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationReference.ToUpper() == organisationReference.ToUpper());
        }

        public virtual async Task<Organisation> GetOrganisationByOrganisationReferenceAndSecurityCodeAsync(
            string organisationReference,
            string securityCode)
        {
            return await _dataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationReference.ToUpper() == organisationReference.ToUpper() && o.SecurityCode == securityCode);
        }


        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Statement GetOrganisationStatement(Organisation organisation, int year = 0)
        {
            var reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType, year);
            return organisation.GetStatement(year);
        }

        //Returns the latest scope for the current accounting date
        public OrganisationScope GetOrganisationLastestScope(Organisation organisation)
        {
            var accountingStartDate = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType);

            return organisation.GetActiveScope(accountingStartDate);
        }
        public virtual async Task<Organisation> GetOrganisationByOrganisationReferenceOrThrowAsync(string organisationReference)
        {
            var org = await GetOrganisationByOrganisationReferenceAsync(organisationReference);

            if (org == null)
                throw new ArgumentException(
                    $"Cannot find organisation with organisationReference {organisationReference}",
                    nameof(organisationReference));

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

        public async Task<CustomResult<Organisation>> CreateOrganisationSecurityCodeAsync(string organisationRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef);
            return _securityCodeLogic.CreateSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> CreateOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.CreateSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExtendOrganisationSecurityCodeAsync(string organisationRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef);
            return _securityCodeLogic.ExtendSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> ExtendOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.ExtendSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExpireOrganisationSecurityCodeAsync(string organisationRef)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef);
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

        public async Task FixLatestAddressesAsync()
        {
            var organisations = _dataRepository.GetAll<Organisation>().Where(o => o.LatestAddress == null && o.OrganisationAddresses.Any(a => a.Status == AddressStatuses.Active)).ToList();
            organisations.SelectMany(o => o.OrganisationAddresses).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestAddress();
            });
            await _dataRepository.SaveChangesAsync();
        }

        public async Task FixLatestScopesAsync()
        {
            var organisations = _dataRepository.GetAll<Organisation>().Where(o => o.LatestScope == null && o.OrganisationScopes.Any(a => a.Status == ScopeRowStatuses.Active)).ToList();
            organisations.SelectMany(o => o.OrganisationScopes).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestScope();
            });
            await _dataRepository.SaveChangesAsync();
        }

        public async Task FixLatestStatementsAsync()
        {
            var organisations = _dataRepository.GetAll<Organisation>().Where(o => o.LatestStatement == null && o.Statements.Any(a => a.Status == StatementStatuses.Submitted)).ToList();
            organisations.SelectMany(o => o.Statements).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestStatement();
            });
            await _dataRepository.SaveChangesAsync();
        }

        public async Task FixLatestRegistrationsAsync()
        {
            var organisations = _dataRepository.GetAll<Organisation>().Where(o => o.LatestRegistration == null && o.UserOrganisations.Any(a => a.PINConfirmedDate != null)).ToList();
            organisations.SelectMany(o => o.UserOrganisations).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestRegistration();
            });
            await _dataRepository.SaveChangesAsync();
        }
        #endregion

    }
}