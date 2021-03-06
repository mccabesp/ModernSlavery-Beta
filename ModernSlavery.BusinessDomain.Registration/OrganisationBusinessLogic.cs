﻿using System;
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
        public IDataRepository DataRepository { get; }
        private readonly IScopeBusinessLogic _scopeLogic;
        private readonly ISecurityCodeBusinessLogic _securityCodeLogic;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly IAuditLogger _badSicLog;
        public OrganisationBusinessLogic(SharedOptions sharedOptions,
            IDataRepository dataRepository, IReportingDeadlineHelper reportingDeadlineHelper,
            IScopeBusinessLogic scopeLogic,
            ISecurityCodeBusinessLogic securityCodeLogic,
            [KeyFilter(Filenames.BadSicLog)] IAuditLogger badSicLog)
        {
            _sharedOptions = sharedOptions;
            DataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _scopeLogic = scopeLogic;
            _securityCodeLogic = securityCodeLogic;
            _badSicLog = badSicLog;
        }

        public IQueryable<Organisation> SearchOrganisations(string searchText, int records)
        {
            var searchData = DataRepository.GetAll<Organisation>();
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
#if DEBUG || DEBUGLOCAL
            var orgs = Debugger.IsAttached
                ? DataRepository.GetAll<Organisation>().Take(100)
                : DataRepository.GetAll<Organisation>();
#else
            var orgs = DataRepository.GetAll<Organisation>();
#endif
            var records = new List<OrganisationsFileModel>();

            foreach (var org in orgs)
            {
                var record = await MapToOrgFileModel(org, year).ConfigureAwait(false);
                records.Add(record);
            }

            return records;
        }

        private async Task<OrganisationsFileModel> MapToOrgFileModel(Organisation org, int year)
        {
            var reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(org.SectorType, year);
            var statement = await GetPrimaryStatementForYear(org, year).ConfigureAwait(false);
            var latestScope = await _scopeLogic.GetScopeByReportingDeadlineOrLatestAsync(org, reportingDeadline).ConfigureAwait(false);
            var address = org.LatestAddress;

            var record = new OrganisationsFileModel
            {
                OrganisationId = org.OrganisationId,
                OrganisationReference = org.OrganisationReference,
                OrganisationName = org.OrganisationName,
                CompanyNo = org.CompanyNumber,
                Sector = org.SectorType,
                Status = org.Status,
                StatusDate = org.StatusDate,
                StatusDetails = org.StatusDetails,
                AddressLine1 = address?.Address1,
                AddressLine2 = address?.Address2,
                AddressLine3 = address?.Address3,
                AddressTownCity = address?.TownCity,
                AddressCounty = address?.County,
                AddressCountry = address?.Country,
                AddressPostCode = address?.PostCode,
                SicCodes = GetOrganisationSicCodeIdsString(org),
                LatestRegistrationDate = org.LatestRegistration?.PINConfirmedDate,
                LatestRegistrationMethod = org.LatestRegistration?.Method,
                Created = org.Created,
                SecurityCode = org.SecurityCode,
                SecurityCodeExpiryDateTime = org.SecurityCodeExpiryDateTime,
                SecurityCodeCreatedDateTime = org.SecurityCodeCreatedDateTime,

                ScopeStatus = latestScope?.ScopeStatus,
                ScopeDate = latestScope?.ScopeStatusDate,

                IsGroupStatement = statement?.StatementOrganisations.Any(),
                FirstSubmittedDate = statement?.Created,
                LatestSubmission = statement?.Modified,
                NumberOfStatements = await CountStatementsIncludedForYear(org, year).ConfigureAwait(false)
            };

            return record;
        }

        public async Task<int> CountStatementsIncludedForYear(Organisation organisation, int year)
        {
            var selfSubmittedReport = await DataRepository.FirstOrDefaultAsync<Statement>(s =>
                s.OrganisationId == organisation.OrganisationId
                && s.SubmissionDeadline.Year == year
                && s.Status == StatementStatuses.Submitted).ConfigureAwait(false);

            var count = (selfSubmittedReport != null ? 1 : 0);

            var submittedGroup = DataRepository.GetAll<StatementOrganisation>()
                .Where(so =>
                    so.OrganisationId == organisation.OrganisationId
                    && so.Included
                    && so.Statement.SubmissionDeadline.Year == year
                    && so.Statement.Status == StatementStatuses.Submitted)
                .OrderBy(so => so.Statement.StatusDate)
                .Count();

            count += submittedGroup;

            return count;
        }

        public async Task<Statement> GetPrimaryStatementForYear(Organisation organisation, int year)
        {
            // if they reported for themselves, that takes primacy
            var submittedStatement = await DataRepository.FirstOrDefaultAsync<Statement>(s =>
                s.OrganisationId == organisation.OrganisationId
                && s.SubmissionDeadline.Year == year
                && s.Status == StatementStatuses.Submitted).ConfigureAwait(false);

            if (submittedStatement != null)
                return submittedStatement;

            // if they are in a group, the earlist submitted report has primacy
            var earliestSubmittedGroup = DataRepository.GetAll<StatementOrganisation>()
                .Where(so =>
                    so.OrganisationId == organisation.OrganisationId
                    && so.Included
                    && so.Statement.SubmissionDeadline.Year == year
                    && so.Statement.Status == StatementStatuses.Submitted)
                .OrderBy(so => so.Statement.StatusDate)
                .FirstOrDefault()
                ?.Statement;

            return earliestSubmittedGroup;
        }

        public IEnumerable<Statement> GetAllStatements(Organisation organisation)
        {
            var single = organisation.Statements.AsEnumerable();
            var group = DataRepository.GetAll<StatementOrganisation>()
                .Where(so => so.OrganisationId == organisation.OrganisationId)
                .Select(so => so.Statement)
                .AsEnumerable();

            return single.Concat(group);
        }

        public virtual async Task SetUniqueOrganisationReferencesAsync()
        {
            var orgs = DataRepository.GetAll<Organisation>().Where(o => o.OrganisationReference == null).ToList();
            foreach (var org in orgs) await SetUniqueOrganisationReferenceAsync(org).ConfigureAwait(false);
            await DataRepository.BulkUpdateAsync(orgs).ConfigureAwait(false);
        }

        public virtual async Task SetUniqueOrganisationReferenceAsync(Organisation organisation)
        {
            //Get the unique reference
            do
            {
                organisation.OrganisationReference = GenerateOrganisationReference();
            } while (await DataRepository.AnyAsync<Organisation>(o =>
                o.OrganisationId != organisation.OrganisationId &&
                o.OrganisationReference == organisation.OrganisationReference).ConfigureAwait(false));
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
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef).ConfigureAwait(false);
            var reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(org.SectorType, changeScopeToSnapshotYear);
            return await _scopeLogic.AddScopeAsync(
                org,
                scopeStatus,
                currentUser,
                reportingDeadline,
                changeScopeToComment,
                saveToDatabase).ConfigureAwait(false);
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
            var orgList = DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Pending);

            if (!orgList.Any())
                throw new Exception("Unable to find organisations with statuses 'Active' or 'Pending' in the database");

            return orgList;
        }

        public Organisation CreateOrganisation(string organisationName, string source, SectorTypes sectorType, OrganisationStatuses status, AddressModel addressModel, AddressStatuses addressStatus = AddressStatuses.Pending, string companyNumber = null, DateTime? dateOfCessation = null, Dictionary<string, string> references = null, SortedSet<int> sicCodes = null, long userId = -1)
        {
            #region Check the parameters
            if (string.IsNullOrWhiteSpace(organisationName)) throw new ArgumentNullException(nameof(organisationName));
            if (sectorType == SectorTypes.Unknown) throw new ArgumentOutOfRangeException(nameof(sectorType));
            if (!status.IsAny(OrganisationStatuses.Active, OrganisationStatuses.Pending)) throw new ArgumentOutOfRangeException(nameof(status), $"Organisation status must be active or pending but was {status}");
            #endregion

            #region Create the basic organisation
            var organisation = new Organisation
            {
                OrganisationName = organisationName,
                SectorType = sectorType,
                CompanyNumber = companyNumber,
                DateOfCessation = dateOfCessation,
            };
            DataRepository.Insert(organisation);

            var orgName = new OrganisationName { Organisation=organisation, Name = organisation.OrganisationName, Source = source };
            organisation.OrganisationNames.Add(orgName);

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
                    DataRepository.Insert(reference);
                    organisation.OrganisationReferences.Add(reference);
                }
            }
            #endregion

            #region Add the SIC codes
            if (sicCodes == null) sicCodes = new SortedSet<int>();
            if (organisation.SectorType == SectorTypes.Public) sicCodes.Add(1);

            //Remove invalid SicCodes
            var badSicCodes = new SortedSet<int>();
            if (sicCodes.Count > 0)
            {
                //TODO we should cache these SIC codes
                var allSicCodes = DataRepository.GetAll<SicCode>().Select(s => s.SicCodeId)
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
                        DataRepository.Insert(sicCode);
                        organisation.OrganisationSicCodes.Add(sicCode);
                    }
                }
            }

            #endregion

            #region Add the organisation address
            //Use the old address for this registration
            //Create address received from user
            var address = new OrganisationAddress {
                Organisation = organisation,
                CreatedByUserId = userId,
                Address1 = addressModel.Address1,
                Address2 = addressModel.Address2,
                Address3 = addressModel.Address3,
                TownCity = addressModel.City,
                County = addressModel.County,
                Country = addressModel.Country,
                PostCode = addressModel.PostCode,
                PoBox = addressModel.PoBox,
                IsUkAddress = addressModel.IsUkAddress,
                Source = source
            };
            address.Trim();
            address.SetStatus(addressStatus, userId);

            DataRepository.Insert(address);
            #endregion

            return organisation;
        }

        public async Task SaveOrganisationAsync(Organisation organisation)
        {
            #region Check the parameters
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));
            #endregion

            //Check if we are creating a new organisation
            var isnew = organisation.OrganisationId == 0;

            //Save the organisation to ensure it has an OrganisationId
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);

            //Ensure the organisation has an organisation reference
            if (string.IsNullOrWhiteSpace(organisation.OrganisationReference))
                await SetUniqueOrganisationReferenceAsync(organisation).ConfigureAwait(false);

            //Create a presumed in-scope for current and previous years
            if (isnew)
            {
                await organisation.SetPresumedScopesAsync(_reportingDeadlineHelper.GetReportingDeadlines(organisation.SectorType)).ConfigureAwait(false);
                await DataRepository.SaveChangesAsync().ConfigureAwait(false);
            }

            //Set the latest scope
            organisation.FixLatestScope();

            //Set the latest address
            organisation.FixLatestAddress();

            //Set the latest statement
            organisation.FixLatestStatement();

            //Set the latest registration
            organisation.FixLatestRegistration();

            await DataRepository.SaveChangesAsync().ConfigureAwait(false);
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

            foreach (var sicCode in sicCodes.SplitI(@";, \n\r\t".ToCharArray()))
            {
                var code = sicCode.ToInt64();
                if (code < 1) continue;

                var sic = DataRepository.GetAll<SicCode>().FirstOrDefault(s => s.SicCodeId == code);
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
            if (organisation.OrganisationId == 0) throw new ArgumentException("Organisation must be saved first before logging bad SIC codes", nameof(organisation));
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
            await Task.WhenAll(badSicLoggingtasks).ConfigureAwait(false);
        }
        #endregion

        #region Repo

        public virtual Organisation GetOrganisationById(long organisationId)
        {
            return DataRepository.Get<Organisation>(organisationId);
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

        public IEnumerable<OrganisationRecord> CreateOrganisationRecords(IEnumerable<Organisation> orgs, bool detailed, long userId = 0)
        {
            if (detailed || userId > 0) orgs.SelectMany(o => o.UserOrganisations);
            if (detailed)
            {
                orgs.SelectMany(o => o.OrganisationAddresses).ToList();
                orgs.SelectMany(o => o.OrganisationReferences).ToList();
                orgs.SelectMany(o => o.OrganisationSicCodes).ToList();
            }
            return orgs.Select(o => CreateOrganisationRecord(o, detailed, userId));
        }

        public OrganisationRecord CreateOrganisationRecord(Organisation org, bool detailed, long userId = 0)
        {
            var organisationRecord=new OrganisationRecord {
                OrganisationId = org.OrganisationId,
                SectorType = org.SectorType,
                OrganisationName = org.OrganisationName,
                CompanyNumber = org.CompanyNumber,
                OrganisationReference = org.OrganisationReference,
                DateOfCessation = org.DateOfCessation,
                DUNSNumber = org.DUNSNumber
            };

            OrganisationAddress address = null;
            if (userId > 0) address = org.UserOrganisations.FirstOrDefault(uo => uo.UserId == userId)?.Address;
            if (address == null) address = org.LatestAddress ?? org.GetLatestAddress();

            if (address != null)
            {
                organisationRecord.ActiveAddressId = address.AddressId;
                organisationRecord.AddressSource = address.Source;
                organisationRecord.Address1 = address.Address1;
                organisationRecord.Address2 = address.Address2;
                organisationRecord.Address3 = address.Address3;
                organisationRecord.City = address.TownCity;
                organisationRecord.County = address.County;
                organisationRecord.Country = address.Country;
                organisationRecord.PostCode = address.PostCode;
                organisationRecord.PoBox = address.PoBox;
                organisationRecord.IsUkAddress = address.IsUkAddress;
            }

            //Add details
            if (detailed)
            {
                organisationRecord.NameSource = GetOrganisationName(org)?.Source;
                organisationRecord.SicCodeIds = GetOrganisationSicCodeIdsString(org);
                organisationRecord.SicSource = GetOrganisationSicSource(org);
                organisationRecord.RegistrationStatus = org.GetRegistrationStatus();
                organisationRecord.References = org.OrganisationReferences.ToDictionary(r => r.ReferenceName, r => r.ReferenceValue, StringComparer.OrdinalIgnoreCase);
            }
            return organisationRecord;
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
            return await DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationReference.ToUpper() == organisationReference.ToUpper()).ConfigureAwait(false);
        }

        public virtual async Task<Organisation> GetOrganisationByOrganisationReferenceAndSecurityCodeAsync(
            string organisationReference,
            string securityCode)
        {
            return await DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationReference.ToUpper() == organisationReference.ToUpper() && o.SecurityCode == securityCode).ConfigureAwait(false);
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
            var org = await GetOrganisationByOrganisationReferenceAsync(organisationReference).ConfigureAwait(false);

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
            var listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync().ConfigureAwait(false);

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
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef).ConfigureAwait(false);
            return _securityCodeLogic.CreateSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> CreateOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.CreateSecurityCode).ConfigureAwait(false);
        }

        public async Task<CustomResult<Organisation>> ExtendOrganisationSecurityCodeAsync(string organisationRef,
            DateTime securityCodeExpiryDateTime)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef).ConfigureAwait(false);
            return _securityCodeLogic.ExtendSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> ExtendOrganisationSecurityCodesInBulkAsync(
            DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime,
                _securityCodeLogic.ExtendSecurityCode).ConfigureAwait(false);
        }

        public async Task<CustomResult<Organisation>> ExpireOrganisationSecurityCodeAsync(string organisationRef)
        {
            var org = await GetOrganisationByOrganisationReferenceOrThrowAsync(organisationRef).ConfigureAwait(false);
            return _securityCodeLogic.ExpireSecurityCode(org);
        }

        public async Task<CustomBulkResult<Organisation>> ExpireOrganisationSecurityCodesInBulkAsync()
        {
            var listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync().ConfigureAwait(false);

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
            var organisations = DataRepository.GetAll<Organisation>().Where(o => o.LatestAddress == null && o.OrganisationAddresses.Any(a => a.Status == AddressStatuses.Active)).ToList();
            organisations.SelectMany(o => o.OrganisationAddresses).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestAddress();
            });
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task FixLatestScopesAsync()
        {
            var organisations = DataRepository.GetAll<Organisation>().Where(o => o.LatestScope == null && o.OrganisationScopes.Any(a => a.Status == ScopeRowStatuses.Active)).ToList();
            organisations.SelectMany(o => o.OrganisationScopes).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestScope();
            });
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task FixLatestStatementsAsync()
        {
            var organisations = DataRepository.GetAll<Organisation>().Where(o => o.LatestStatement == null && o.Statements.Any(a => a.Status == StatementStatuses.Submitted)).ToList();
            organisations.SelectMany(o => o.Statements).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestStatement();
            });
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task FixLatestRegistrationsAsync()
        {
            var organisations = DataRepository.GetAll<Organisation>().Where(o => o.LatestRegistration == null && o.UserOrganisations.Any(a => a.PINConfirmedDate != null)).ToList();
            organisations.SelectMany(o => o.UserOrganisations).ToList();

            Parallel.ForEach(organisations, organisation =>
            {
                organisation.FixLatestRegistration();
            });
            await DataRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        #endregion

    }
}