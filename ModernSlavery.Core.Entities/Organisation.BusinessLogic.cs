using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    [DebuggerDisplay("{OrganisationName},{Status}")]
    public partial class Organisation
    {
        private readonly int PinInPostExpiryDays;
        private readonly string SecurityCodeChars;
        private readonly int SecurityCodeLength;
        private IObfuscator _obfuscator;
        private ISnapshotDateHelper _snapshotDateHelper;
        private GlobalOptions GlobalOptions;

        public Organisation(GlobalOptions globalOptions, IObfuscator obfuscator, ISnapshotDateHelper snapshotDateHelper):this()
        {
            GlobalOptions = globalOptions;
            _obfuscator = obfuscator;
            _snapshotDateHelper = snapshotDateHelper;

            PinInPostExpiryDays = globalOptions.PinInPostExpiryDays;
            SecurityCodeChars = globalOptions.SecurityCodeChars;
            SecurityCodeLength = globalOptions.SecurityCodeLength;
        }

        private DateTime PinExpiresDate => VirtualDateTime.Now.AddDays(0 - PinInPostExpiryDays);

        public OrganisationStatus PreviousStatus
        {
            get
            {
                return Enumerable.Skip<OrganisationStatus>(OrganisationStatuses
                        .OrderByDescending(os => os.StatusDate), 1)
                    .FirstOrDefault();
            }
        }

        public void SetStatus(OrganisationStatuses status, long byUserId, string details = null)
        {
            if (status == Status && details == StatusDetails) return;

            OrganisationStatuses.Add(
                new OrganisationStatus
                {
                    OrganisationId = OrganisationId,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        /// <summary>
        ///     Returns true if organisation has been made an orphan and is in scope
        /// </summary>
        public bool GetIsOrphan()
        {
            return Status == Entities.OrganisationStatuses.Active
                   && (LatestScope.ScopeStatus == ScopeStatuses.InScope ||
                       LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                   && (UserOrganisations == null
                       || !Enumerable.Any<Entities.UserOrganisation>(UserOrganisations, uo => uo.PINConfirmedDate != null
                                                                      || uo.Method == RegistrationMethods.Manual
                                                                      || uo.Method == RegistrationMethods.PinInPost
                                                                      && uo.PINSentDate.HasValue
                                                                      && uo.PINSentDate.Value > PinExpiresDate));
        }

        public string GetRegistrationStatus()
        {
            var reg = Enumerable.OrderBy<Entities.UserOrganisation, DateTime?>(UserOrganisations, uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINConfirmedDate != null);
            if (reg != null) return $"Registered {reg.PINConfirmedDate?.ToFriendly(false)}";

            reg = Enumerable.OrderBy<Entities.UserOrganisation, DateTime?>(UserOrganisations, uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINSentDate != null && uo.PINConfirmedDate == null);
            if (reg != null) return "Awaiting PIN";

            reg = Enumerable.OrderBy<Entities.UserOrganisation, DateTime?>(UserOrganisations, uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo =>
                    uo.PINSentDate == null && uo.PINConfirmedDate == null && uo.Method == RegistrationMethods.Manual);
            if (reg != null) return "Awaiting Approval";

            return "No registrations";
        }

        public string GetSicSectorsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            var organisationSicCodes = GetSicCodes(maxDate);
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
        public IEnumerable<OrganisationSicCode> GetSicCodes(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = GetAccountingStartDate().AddYears(1);

            return Enumerable.Where<OrganisationSicCode>(OrganisationSicCodes, s =>
                s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value));
        }

        public SortedSet<int> GetSicCodeIds(DateTime? maxDate = null)
        {
            var organisationSicCodes = GetSicCodes(maxDate);

            var codes = new SortedSet<int>();
            foreach (var sicCode in organisationSicCodes) codes.Add(sicCode.SicCodeId);

            return codes;
        }


        public string GetSicSource(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = GetAccountingStartDate().AddYears(1);

            return Enumerable.FirstOrDefault<OrganisationSicCode>(OrganisationSicCodes, s => s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value))
                ?.Source;
        }

        public string GetSicCodeIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            return GetSicCodes(maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString(delimiter);
        }

        public string GetSicSectionIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            var organisationSicCodes = GetSicCodes(maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSectionId).UniqueI().OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        public string GetEncryptedId()
        {
            return _obfuscator.Obfuscate((string) OrganisationId.ToString());
        }

        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Entities.Return GetReturn(int year = 0)
        {
            var accountingStartDate = GetAccountingStartDate(year);
            return Enumerable.Where<Entities.Return>(Returns, r => r.Status == ReturnStatuses.Submitted && r.AccountingDate == accountingStartDate)
                .OrderByDescending(r => r.StatusDate)
                .FirstOrDefault();
        }

        //Returns the latest return for the specified accounting date or the latest ever if no accounting date specified
        public Entities.Return GetReturn(DateTime? accountingStartDate = null)
        {
            if (accountingStartDate == null || accountingStartDate.Value == DateTime.MinValue)
                accountingStartDate = GetAccountingStartDate();

            return Enumerable.FirstOrDefault<Entities.Return>(Returns, r =>
                r.Status == ReturnStatuses.Submitted && r.AccountingDate == accountingStartDate);
        }


        //Returns the latest scope for the current accounting date
        public OrganisationScope GetCurrentScope()
        {
            var accountingStartDate = GetAccountingStartDate();

            return GetScopeForYear(accountingStartDate);
        }


        //Returns the scope for the specified accounting date
        public OrganisationScope GetScopeForYear(DateTime accountingStartDate)
        {
            return GetScopeForYear(accountingStartDate.Year);
        }

        public OrganisationScope GetScopeForYear(int year)
        {
            return Enumerable.FirstOrDefault<OrganisationScope>(OrganisationScopes, s => s.Status == ScopeRowStatuses.Active && s.SnapshotDate.Year == year);
        }


        public ScopeStatuses GetScopeStatus(int year = 0)
        {
            var accountingStartDate = GetAccountingStartDate(year);
            return GetScopeStatus(accountingStartDate);
        }

        public ScopeStatuses GetScopeStatus(DateTime accountingStartDate)
        {
            var scope = GetScopeForYear(accountingStartDate);
            return scope == null ? ScopeStatuses.Unknown : scope.ScopeStatus;
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public OrganisationName GetName(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = GetAccountingStartDate().AddYears(1);

            return Enumerable.Where<OrganisationName>(OrganisationNames, n => n.Created < maxDate.Value).OrderByDescending(n => n.Created)
                .FirstOrDefault();
        }


        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        public Entities.OrganisationAddress GetAddress(DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue) maxDate = GetAccountingStartDate().AddYears(1);

            if (status == AddressStatuses.Active && LatestAddress != null &&
                maxDate == GetAccountingStartDate().AddYears(1)) return LatestAddress;

            var addressStatus = Enumerable.SelectMany<Entities.OrganisationAddress, AddressStatus>(OrganisationAddresses, a => Enumerable.Where<AddressStatus>(a.AddressStatuses, s => s.Status == status && s.StatusDate < maxDate.Value))
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefault();

            if (addressStatus != null && addressStatus.Address.Status == status) return addressStatus.Address;

            if (LatestAddress != null && LatestAddress.Status == status) return LatestAddress;

            return null;
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public string GetAddressString(DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active,
            string delimiter = ", ")
        {
            var address = GetAddress(maxDate, status);

            return address?.GetAddressString(delimiter);
        }

        public bool GetIsDissolved()
        {
            return DateOfCessation != null && DateOfCessation < GetAccountingStartDate();
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            var target = (Entities.Organisation) obj;
            return OrganisationId == target.OrganisationId;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        public static IQueryable<Entities.Organisation> Search(IQueryable<Entities.Organisation> searchData,
            string searchText,
            int records,
            int levenshteinDistance = 0)
        {
            var levenshteinRecords =
                searchData.ToList().Select(o => new
                    {distance = o.OrganisationName.LevenshteinCompute(searchText), org = o});
            var pattern = searchText?.ToLower();

            var searchResults = levenshteinRecords.AsQueryable()
                .Where(
                    data => data.org.OrganisationName.ToLower().Contains(pattern)
                            || data.org.OrganisationName.Length > levenshteinDistance &&
                            data.distance <= levenshteinDistance)
                .OrderBy(o => o.distance)
                .Take(records)
                .Select(o => o.org);
            return searchResults;
        }

        public IEnumerable<Entities.Return> GetRecentReports(int recentCount)
        {
            foreach (var year in GetRecentReportingYears(recentCount))
            {
                var defaultReturn = new Entities.Return
                {
                    Organisation = this,
                    AccountingDate = GetAccountingStartDate(year),
                    Modified = VirtualDateTime.Now
                };
                defaultReturn.IsLateSubmission = defaultReturn.CalculateIsLateSubmission();

                yield return GetReturn(year) ?? defaultReturn;
            }
        }

        public IEnumerable<int> GetRecentReportingYears(int recentCount)
        {
            var endYear = GetAccountingStartDate().Year;
            var startYear = endYear - (recentCount - 1);
            if (startYear < _snapshotDateHelper.FirstReportingYear) startYear = _snapshotDateHelper.FirstReportingYear;

            for (var year = endYear; year >= startYear; year--) yield return year;
        }

        public bool GetIsInscope(int year)
        {
            return !GetScopeStatus(year).IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.OutOfScope);
        }


        public IEnumerable<Entities.Return> GetSubmittedReports()
        {
            return Enumerable.Where<Entities.Return>(Returns, r => r.Status == ReturnStatuses.Submitted).OrderByDescending(r => r.AccountingDate);
        }

        public void RevertToLastStatus(long byUserId, string details = null)
        {
            var previousStatus = PreviousStatus
                                 ?? throw new InvalidOperationException(
                                     $"The list of Statuses for Organisation '{OrganisationName}' employerReference '{EmployerReference}' isn't long enough to perform a '{nameof(RevertToLastStatus)}' command. It needs to have at least 2 statuses so these can reverted.");

            SetStatus(previousStatus.Status, byUserId, details);
        }

        public OrganisationScope GetLatestScopeForSnapshotYear(int snapshotYear)
        {
            return Enumerable.FirstOrDefault<OrganisationScope>(OrganisationScopes, orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active
                    && orgScope.SnapshotDate.Year == snapshotYear);
        }

        public OrganisationScope GetLatestScopeForSnapshotYearOrThrow(int snapshotYear)
        {
            var organisationScope = GetLatestScopeForSnapshotYear(snapshotYear);

            if (organisationScope == null)
                throw new ArgumentOutOfRangeException(
                    $"Cannot find an scope with status 'Active' for snapshotYear '{snapshotYear}' linked to organisation '{OrganisationName}', employerReference '{EmployerReference}'.");

            return organisationScope;
        }

        public bool IsActive()
        {
            return Status == Entities.OrganisationStatuses.Active;
        }

        public bool IsPending()
        {
            return Status == Entities.OrganisationStatuses.Pending;
        }

        /// <summary>
        ///     The security code is created exclusively during setting, for all other cases (extend/expire) see method
        ///     'SetSecurityCodeExpiryDate'
        /// </summary>
        /// <param name="securityCodeExpiryDateTime"></param>
        public virtual void SetSecurityCode(DateTime securityCodeExpiryDateTime)
        {
            //Set the security token
            string newSecurityCode = null;
            do
            {
                newSecurityCode = Crypto.GeneratePasscode(SecurityCodeChars.ToCharArray(), SecurityCodeLength);
            } while (newSecurityCode == SecurityCode);

            SecurityCode = newSecurityCode;
            SetSecurityCodeExpiryDate(securityCodeExpiryDateTime);
        }

        /// <summary>
        ///     Method to modify the security code expiring information (create/extend/expire). It additionally timestamps such
        ///     change.
        /// </summary>
        /// <param name="securityCodeExpiryDateTime"></param>
        public void SetSecurityCodeExpiryDate(DateTime securityCodeExpiryDateTime)
        {
            if (SecurityCode == null)
                throw new Exception(
                    "Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null");

            SecurityCodeExpiryDateTime = securityCodeExpiryDateTime;
            SecurityCodeCreatedDateTime = VirtualDateTime.Now;
        }

        public bool HasSecurityCodeExpired()
        {
            return SecurityCodeExpiryDateTime < VirtualDateTime.Now;
        }

        public Entities.UserOrganisation GetLatestRegistration()
        {
            return Enumerable.Where<Entities.UserOrganisation>(UserOrganisations, uo => uo.PINConfirmedDate != null)
                .OrderByDescending(uo => uo.PINConfirmedDate)
                .FirstOrDefault();
        }

        public override string ToString()
        {
            return $"ref:{EmployerReference}, name:{OrganisationName}";
        }

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        public DateTime GetAccountingStartDate(int year = 0)
        {
            return _snapshotDateHelper.GetSnapshotDate(SectorType, year);
        }
    }
}