﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.BusinessDomain.Submission
{
    public class ScopeBusinessLogic : IScopeBusinessLogic
    {
        private readonly ISharedBusinessLogic _sharedBusinessLogic; 
        public ScopeBusinessLogic(
            ISharedBusinessLogic sharedBusinessLogic)
        {
            _sharedBusinessLogic = sharedBusinessLogic;
        }

        /// <summary>
        ///     Returns the latest scope status for an organisation and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual async Task<ScopeStatuses> GetLatestScopeStatusForSnapshotYearAsync(long organisationId,
            int snapshotYear)
        {
            var latestScope = await GetLatestScopeBySnapshotYearAsync(organisationId, snapshotYear);
            if (latestScope == null) return ScopeStatuses.Unknown;

            return latestScope.ScopeStatus;
        }

        /// <summary>
        ///     Returns the latest scope status for an organisation and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual ScopeStatuses GetLatestScopeStatusForSnapshotYear(Organisation org, int snapshotYear = 0)
        {
            var latestScope = GetLatestScopeBySnapshotYear(org, snapshotYear);
            if (latestScope == null) return ScopeStatuses.Unknown;

            return latestScope.ScopeStatus;
        }

        /// <summary>
        ///     Returns the latest scope for an organisation
        /// </summary>
        /// <param name="employerReference"></param>
        public virtual async Task<OrganisationScope> GetScopeByEmployerReferenceAsync(string employerReference,
            int snapshotYear = 0)
        {
            var org = await GetOrgByEmployerReferenceAsync(employerReference);
            if (org == null) return null;

            return GetLatestScopeBySnapshotYear(org, snapshotYear);
        }

        /// <summary>
        ///     Creates a new scope record using an existing scope and applies a new status
        /// </summary>
        /// <param name="existingOrgScopeId"></param>
        /// <param name="newStatus"></param>
        public virtual async Task<OrganisationScope> UpdateScopeStatusAsync(long existingOrgScopeId,
            ScopeStatuses newStatus)
        {
            var oldOrgScope =
                await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<OrganisationScope>(os =>
                    os.OrganisationScopeId == existingOrgScopeId);

            // when OrganisationScope isn't found then throw ArgumentOutOfRangeException
            if (oldOrgScope == null)
                throw new ArgumentOutOfRangeException(
                    nameof(existingOrgScopeId),
                    $"Cannot find organisation with OrganisationScopeId: {existingOrgScopeId}");

            var org = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationId == oldOrgScope.OrganisationId);
            // when Organisation isn't found then throw ArgumentOutOfRangeException
            if (org == null)
                throw new ArgumentOutOfRangeException(
                    nameof(oldOrgScope.OrganisationId),
                    $"Cannot find organisation with OrganisationId: {oldOrgScope.OrganisationId}");

            // When Organisation is Found Then Save New Scope Record With New Status
            var newScope = new OrganisationScope
            {
                OrganisationId = oldOrgScope.OrganisationId,
                ContactEmailAddress = oldOrgScope.ContactEmailAddress,
                ContactFirstname = oldOrgScope.ContactFirstname,
                ContactLastname = oldOrgScope.ContactLastname,
                ReadGuidance = oldOrgScope.ReadGuidance,
                Reason = oldOrgScope.Reason,
                ScopeStatus = newStatus,
                ScopeStatusDate = VirtualDateTime.Now,
                RegisterStatus = oldOrgScope.RegisterStatus,
                RegisterStatusDate = oldOrgScope.RegisterStatusDate,
                // carry the snapshot date over
                SubmissionDeadline = oldOrgScope.SubmissionDeadline
            };

            await SaveScopeAsync(org, true, newScope);
            return newScope;
        }

        public virtual async Task<CustomResult<OrganisationScope>> AddScopeAsync(Organisation organisation,
            ScopeStatuses newStatus,
            User currentUser,
            int snapshotYear,
            string comment,
            bool saveToDatabase)
        {
            snapshotYear = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType,snapshotYear)
                .Year;

            var oldOrgScope = organisation.GetScopeOrThrow(snapshotYear);

            if (oldOrgScope.ScopeStatus == newStatus)
                return new CustomResult<OrganisationScope>(
                    InternalMessages.SameScopesCannotBeUpdated(newStatus, oldOrgScope.ScopeStatus, snapshotYear));

            // When Organisation is Found Then Save New Scope Record With New Status
            var newScope = new OrganisationScope
            {
                OrganisationId = oldOrgScope.OrganisationId,
                Organisation = organisation,
                /* Updated by the current user */
                ContactEmailAddress = currentUser.EmailAddress,
                ContactFirstname = currentUser.Firstname,
                ContactLastname = currentUser.Lastname,
                ReadGuidance = oldOrgScope.ReadGuidance,
                Reason = !string.IsNullOrEmpty(comment)
                    ? comment
                    : oldOrgScope.Reason,
                ScopeStatus = newStatus,
                ScopeStatusDate = VirtualDateTime.Now,
                StatusDetails = _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(currentUser)
                    ? "Changed by Admin"
                    : null,
                RegisterStatus = oldOrgScope.RegisterStatus,
                RegisterStatusDate = oldOrgScope.RegisterStatusDate,
                // carry the snapshot date over
                SubmissionDeadline = oldOrgScope.SubmissionDeadline
            };

            await SaveScopeAsync(organisation, saveToDatabase, newScope);

            return new CustomResult<OrganisationScope>(newScope);
        }

        public virtual async Task SaveScopeAsync(Organisation org, bool saveToDatabase = true,
            params OrganisationScope[] newScopes)
        {
            await SaveScopesAsync(org, newScopes, saveToDatabase);
        }

        public virtual async Task SaveScopesAsync(Organisation org, IEnumerable<OrganisationScope> newScopes,
            bool saveToDatabase = true)
        {
            foreach (var newScope in newScopes.OrderBy(s => s.SubmissionDeadline).ThenBy(s => s.ScopeStatusDate))
            {
                // find any prev submitted scopes in the same snapshot year year and retire them
                org.OrganisationScopes
                    .Where(x => x.Status == ScopeRowStatuses.Active &&
                                x.SubmissionDeadline.Year == newScope.SubmissionDeadline.Year)
                    .ToList()
                    .ForEach(x => x.Status = ScopeRowStatuses.Retired);

                // add the new scope
                newScope.Status = ScopeRowStatuses.Active;
                org.OrganisationScopes.Add(newScope);
                if (org.LatestScope == null
                    || newScope.SubmissionDeadline > org.LatestScope.SubmissionDeadline
                    || newScope.SubmissionDeadline == org.LatestScope.SubmissionDeadline &&
                    newScope.ScopeStatusDate >= org.LatestScope.ScopeStatusDate)
                    org.LatestScope = newScope;
            }

            // save to db
            if (saveToDatabase)
            {
                await _sharedBusinessLogic.DataRepository.SaveChangesAsync();
            }
        }

        public virtual IEnumerable<ScopesFileModel> GetScopesFileModelByYear(int year)
        {
            var scopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .Where(s => s.SubmissionDeadline.Year == year && s.Status == ScopeRowStatuses.Active);

#if DEBUG
            if (Debugger.IsAttached) scopes = scopes.Take(100);
#endif
            var records = scopes.Select(
                o => new ScopesFileModel
                {
                    OrganisationId = o.OrganisationId,
                    OrganisationName = o.Organisation.OrganisationName,
                    DUNSNumber = o.Organisation.DUNSNumber,
                    EmployerReference = o.Organisation.EmployerReference,
                    OrganisationScopeId = o.OrganisationScopeId,
                    ScopeStatus = o.ScopeStatus,
                    ScopeStatusDate = o.ScopeStatusDate,
                    RegisterStatus = o.RegisterStatus,
                    RegisterStatusDate = o.RegisterStatusDate,
                    ContactEmailAddress = o.ContactEmailAddress,
                    ContactFirstname = o.ContactFirstname,
                    ContactLastname = o.ContactLastname,
                    ReadGuidance = o.ReadGuidance,
                    Reason = o.Reason,
                    CampaignId = o.CampaignId
                });

            return records;
        }

        public async Task<HashSet<Organisation>> SetScopeStatusesAsync()
        {
            var lastSnapshotDate = DateTime.MinValue;
            long lastOrganisationId = -1;
            var index = -1;
            var count = 0;
            var scopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .OrderBy(os => os.SubmissionDeadline)
                .ThenBy(os => os.OrganisationId)
                .ThenByDescending(os => os.ScopeStatusDate);
            var changedOrgs = new HashSet<Organisation>();
            foreach (var scope in scopes)
            {
                count++;
                if (lastSnapshotDate != scope.SubmissionDeadline || lastOrganisationId != scope.OrganisationId)
                    index = 0;
                else
                    index++;

                //Set the status
                var newStatus = index == 0 ? ScopeRowStatuses.Active : ScopeRowStatuses.Retired;
                if (scope.Status != newStatus)
                {
                    scope.Status = newStatus;
                    changedOrgs.Add(scope.Organisation);
                }

                lastSnapshotDate = scope.SubmissionDeadline;
                lastOrganisationId = scope.OrganisationId;
            }

            await _sharedBusinessLogic.DataRepository.SaveChangesAsync();

            return changedOrgs;
        }

        public async Task<HashSet<Organisation>> SetPresumedScopesAsync()
        {
            var missingOrgs = await FindOrgsWhereScopeNotSetAsync();
            var changedOrgs = new HashSet<Organisation>();

            foreach (var org in missingOrgs)
                if (FillMissingScopes(org.Organisation))
                    changedOrgs.Add(org.Organisation);

            if (changedOrgs.Count > 0) await _sharedBusinessLogic.DataRepository.SaveChangesAsync();

            return changedOrgs;
        }

        public bool FillMissingScopes(Organisation org)
        {
            var firstYear = _sharedBusinessLogic.SharedOptions.FirstReportingYear;
            var currentSnapshotDate = _sharedBusinessLogic.GetReportingStartDate(org.SectorType);
            var currentSnapshotYear = currentSnapshotDate.Year;
            var prevYearScope = ScopeStatuses.Unknown;
            var neverDeclaredScope = true;
            var changed = false;

            for (var snapshotYear = firstYear; snapshotYear <= currentSnapshotYear; snapshotYear++)
            {
                var scope = org.GetScope(snapshotYear);

                // if we already have a scope then flag (prevYearScope, neverDeclaredScope) and skip this year
                if (scope != null && scope.ScopeStatus != ScopeStatuses.Unknown)
                {
                    prevYearScope = scope.ScopeStatus;
                    neverDeclaredScope = false;
                    continue;
                }

                // determine the snapshot date from year
                var snapshotDate = new DateTime(snapshotYear, currentSnapshotDate.Month, currentSnapshotDate.Day);

                // determine if need to presume scope
                var shouldPresumeScope = neverDeclaredScope
                                         && (prevYearScope == ScopeStatuses.PresumedOutOfScope
                                             || prevYearScope == ScopeStatuses.PresumedInScope
                                             || prevYearScope == ScopeStatuses.Unknown);

                // presumed scope from created date
                if (shouldPresumeScope)
                {
                    var createdAfterSnapshotYear = org.Created >= snapshotDate.AddYears(1);
                    if (createdAfterSnapshotYear)
                        prevYearScope = ScopeStatuses.PresumedOutOfScope;
                    else
                        prevYearScope = ScopeStatuses.PresumedInScope;
                }
                // otherwise presume scope from declared scope
                else if (prevYearScope == ScopeStatuses.InScope)
                {
                    prevYearScope = ScopeStatuses.PresumedInScope;
                }
                else if (prevYearScope == ScopeStatuses.OutOfScope)
                {
                    prevYearScope = ScopeStatuses.PresumedOutOfScope;
                }

                // update the scope status
                SetPresumedScope(org, prevYearScope, snapshotDate);

                changed = true;
            }

            // set the latest scope if not set
            if (org.LatestScope == null)
            {
                org.LatestScope = org.GetLatestScope();
                changed = true;
            }

            return changed;
        }

        public async Task<HashSet<OrganisationMissingScope>> FindOrgsWhereScopeNotSetAsync()
        {
            // get all orgs of any status
            var allOrgs = await _sharedBusinessLogic.DataRepository.ToListAsync<Organisation>();

            var firstYear = _sharedBusinessLogic.SharedOptions.FirstReportingYear;

            // find all orgs who have no scope or unknown scope statuses
            var orgsWithMissingScope = new HashSet<OrganisationMissingScope>();
            foreach (var org in allOrgs)
            {
                var currentSnapshotDate = _sharedBusinessLogic.GetReportingStartDate(org.SectorType);
                var currentYear = currentSnapshotDate.Year;
                var missingSnapshotYears = new List<int>();

                // for all snapshot years check if scope exists
                for (var year = firstYear; year <= currentYear; year++)
                {
                    var scope = org.GetScope(year);
                    if (scope == null || scope.ScopeStatus == ScopeStatuses.Unknown) missingSnapshotYears.Add(year);
                }

                // collect
                if (missingSnapshotYears.Count > 0)
                    orgsWithMissingScope.Add(
                        new OrganisationMissingScope {Organisation = org, MissingSnapshotYears = missingSnapshotYears});
            }

            return orgsWithMissingScope;
        }

        /// <summary>
        ///     Adds a new scope and updates the latest scope (if required)
        /// </summary>
        /// <param name="org"></param>
        /// <param name="scopeStatus"></param>
        /// <param name="snapshotDate"></param>
        /// <param name="currentUser"></param>
        public virtual OrganisationScope SetPresumedScope(Organisation org,
            ScopeStatuses scopeStatus,
            DateTime snapshotDate,
            User currentUser = null)
        {
            //Ensure scopestatus is presumed
            if (scopeStatus != ScopeStatuses.PresumedInScope && scopeStatus != ScopeStatuses.PresumedOutOfScope)
                throw new ArgumentOutOfRangeException(nameof(scopeStatus));

            //Check no previous scopes
            if (org.OrganisationScopes.Any(os => os.SubmissionDeadline == snapshotDate))
                throw new ArgumentException(
                    $"A scope already exists for snapshot year {snapshotDate.Year} for organisation employer reference '{org.EmployerReference}'",
                    nameof(scopeStatus));

            //Check for conflict with previous years scope
            if (snapshotDate.Year > _sharedBusinessLogic.SharedOptions.FirstReportingYear)
            {
                var previousScope = GetLatestScopeStatusForSnapshotYear(org, snapshotDate.Year - 1);
                if (previousScope == ScopeStatuses.InScope && scopeStatus == ScopeStatuses.PresumedOutOfScope
                    || previousScope == ScopeStatuses.OutOfScope && scopeStatus == ScopeStatuses.PresumedInScope)
                    throw new ArgumentException(
                        $"Cannot set {scopeStatus} for snapshot year {snapshotDate.Year} when previos year was {previousScope} for organisation employer reference '{org.EmployerReference}'",
                        nameof(scopeStatus));
            }

            var newScope = new OrganisationScope
            {
                OrganisationId = org.OrganisationId,
                ContactEmailAddress = currentUser?.EmailAddress,
                ContactFirstname = currentUser?.Firstname,
                ContactLastname = currentUser?.Lastname,
                ScopeStatus = scopeStatus,
                Status = ScopeRowStatuses.Active,
                StatusDetails = "Generated by the system",
                SubmissionDeadline = snapshotDate
            };

            org.OrganisationScopes.Add(newScope);

            return newScope;
        }

        public async Task<Organisation> GetOrgByEmployerReferenceAsync(string employerReference)
        {
            var org = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference == employerReference);
            return org;
        }

        #region Repo

        public virtual async Task<OrganisationScope> GetScopeByIdAsync(long organisationScopeId)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<OrganisationScope>(o =>
                o.OrganisationScopeId == organisationScopeId);
        }

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual async Task<OrganisationScope> GetLatestScopeBySnapshotYearAsync(long organisationId,
            int snapshotYear = 0)
        {
            var org = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o => o.OrganisationId == organisationId);
            if (org == null)
                throw new ArgumentException($"Cannot find organisation with id {organisationId}",
                    nameof(organisationId));

            return GetLatestScopeBySnapshotYear(org, snapshotYear);
        }

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual OrganisationScope GetLatestScopeBySnapshotYear(Organisation organisation, int snapshotYear = 0)
        {
            if (snapshotYear == 0)
                snapshotYear = _sharedBusinessLogic.GetReportingStartDate(organisation.SectorType).Year;

            var orgScope = organisation.OrganisationScopes
                .SingleOrDefault(s => s.SubmissionDeadline.Year == snapshotYear && s.Status == ScopeRowStatuses.Active);

            return orgScope;
        }

        public virtual async Task<OrganisationScope> GetPendingScopeRegistrationAsync(string emailAddress)
        {
            var result = await _sharedBusinessLogic.DataRepository.FirstOrDefaultByDescendingAsync<OrganisationScope, DateTime>(
                s => s.RegisterStatusDate,
                o => o.RegisterStatus == RegisterStatuses.RegisterPending && o.ContactEmailAddress == emailAddress);
            return result;
        }

        #endregion
    }
}