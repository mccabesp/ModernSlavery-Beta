using System;
using System.Collections.Concurrent;
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
        private readonly IDataRepository _dataRepository;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly IAuthorisationBusinessLogic _authorisationBusinessLogic;
        public ScopeBusinessLogic(IDataRepository dataRepository, IReportingDeadlineHelper reportingDeadlineHelper, IAuthorisationBusinessLogic authorisationBusinessLogic)
        {
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _authorisationBusinessLogic = authorisationBusinessLogic;
        }

        public virtual async Task<OrganisationScope> GetScopeByIdAsync(long organisationScopeId)
        {
            return await _dataRepository.GetAsync<OrganisationScope>(organisationScopeId);
        }

        #region GetScopeByReportingDeadlineOrLatest

        public virtual async Task<OrganisationScope> GetScopeByReportingDeadlineOrLatestAsync(long organisationId, DateTime? reportingDeadline = null)
        {
            var org = await _dataRepository.FirstOrDefaultAsync<Organisation>(o => o.OrganisationId == organisationId);
            if (org == null) throw new ArgumentException($"Cannot find organisation with id {organisationId}", nameof(organisationId));

            return GetScopeByReportingDeadlineOrLatestAsync(org, reportingDeadline);
        }

        public virtual OrganisationScope GetScopeByReportingDeadlineOrLatestAsync(Organisation organisation, DateTime? reportingDeadline = null)
        {
            if (reportingDeadline == null) reportingDeadline = _reportingDeadlineHelper.GetReportingDeadline(organisation.SectorType);

            var orgScope = organisation.OrganisationScopes.SingleOrDefault(s => s.SubmissionDeadline == reportingDeadline && s.Status == ScopeRowStatuses.Active);

            return orgScope;
        }

        #endregion

        #region GetScopeStatusByReportingDeadlineOrLatest
        public virtual async Task<ScopeStatuses> GetScopeStatusByReportingDeadlineOrLatestAsync(long organisationId, DateTime reportingDeadline)
        {
            var latestScope = await GetScopeByReportingDeadlineOrLatestAsync(organisationId, reportingDeadline);
            if (latestScope == null) return ScopeStatuses.Unknown;

            return latestScope.ScopeStatus;
        }

        public virtual ScopeStatuses GetScopeStatusByReportingDeadlineOrLatestAsync(Organisation org, DateTime? reportingDeadline = null)
        {
            var latestScope = GetScopeByReportingDeadlineOrLatestAsync(org, reportingDeadline);
            if (latestScope == null) return ScopeStatuses.Unknown;

            return latestScope.ScopeStatus;
        }
        #endregion

        public virtual async Task<OrganisationScope> UpdateScopeStatusAsync(long existingOrgScopeId, ScopeStatuses newStatus)
        {
            var oldScope = await _dataRepository.GetAsync<OrganisationScope>(existingOrgScopeId);

            // when OrganisationScope isn't found then throw ArgumentOutOfRangeException
            if (oldScope == null)
                throw new ArgumentOutOfRangeException(
                    nameof(existingOrgScopeId),
                    $"Cannot find organisation with OrganisationScopeId: {existingOrgScopeId}");

            var organisation = await _dataRepository.FirstOrDefaultAsync<Organisation>(o => o.OrganisationId == oldScope.OrganisationId);
            // when Organisation isn't found then throw ArgumentOutOfRangeException
            if (organisation == null)
                throw new ArgumentOutOfRangeException(
                    nameof(oldScope.OrganisationId),
                    $"Cannot find organisation with OrganisationId: {oldScope.OrganisationId}");

            // When Organisation is Found Then Save New Scope Record With New Status
            var newScope = new OrganisationScope
            {
                OrganisationId = oldScope.OrganisationId,
                ContactEmailAddress = oldScope.ContactEmailAddress,
                ContactFirstname = oldScope.ContactFirstname,
                ContactLastname = oldScope.ContactLastname,
                ContactJobTitle = oldScope.ContactJobTitle,
                ReadGuidance = oldScope.ReadGuidance,
                Reason = oldScope.Reason,
                TurnOver = oldScope.TurnOver,
                ScopeStatus = newStatus,
                ScopeStatusDate = VirtualDateTime.Now,
                RegisterStatus = oldScope.RegisterStatus,
                RegisterStatusDate = oldScope.RegisterStatusDate,
                // carry the snapshot date over
                SubmissionDeadline = oldScope.SubmissionDeadline
            };

            await SaveScopeAsync(organisation, true, newScope);
            return newScope;
        }

        public virtual async Task<CustomResult<OrganisationScope>> AddScopeAsync(Organisation organisation,
            ScopeStatuses newStatus,
            User currentUser,
            DateTime reportingDeadline,
            string comment,
            bool saveToDatabase)
        {
            var oldOrgScope = organisation.GetActiveScope(reportingDeadline);
            if (oldOrgScope == null) throw new ArgumentOutOfRangeException($"Cannot find an scope with status 'Active' for reporting deadling '{reportingDeadline}' linked to organisation '{organisation.OrganisationName}', organisationReference '{organisation.OrganisationReference}'.");

            if (oldOrgScope.ScopeStatus == newStatus)
                return new CustomResult<OrganisationScope>(InternalMessages.SameScopesCannotBeUpdated(newStatus, oldOrgScope.ScopeStatus, reportingDeadline));

            // When Organisation is Found Then Save New Scope Record With New Status
            var newScope = new OrganisationScope
            {
                OrganisationId = oldOrgScope.OrganisationId,
                Organisation = organisation,
                /* Updated by the current user */
                ContactEmailAddress = currentUser.EmailAddress,
                ContactFirstname = currentUser.Firstname,
                ContactLastname = currentUser.Lastname,
                ContactJobTitle = currentUser.JobTitle,
                ReadGuidance = oldOrgScope.ReadGuidance,
                Reason = !string.IsNullOrEmpty(comment)
                    ? comment
                    : oldOrgScope.Reason,
                TurnOver = oldOrgScope.TurnOver,
                ScopeStatus = newStatus,
                ScopeStatusDate = VirtualDateTime.Now,
                StatusDetails = _authorisationBusinessLogic.IsAdministrator(currentUser)
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
                                x.SubmissionDeadline == newScope.SubmissionDeadline)
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
                await _dataRepository.SaveChangesAsync();
            }
        }

        public virtual IEnumerable<ScopesFileModel> GetScopesFileModelByYear(int year)
        {
            var scopes = _dataRepository.GetAll<OrganisationScope>().Where(s => s.SubmissionDeadline.Year == year && s.Status == ScopeRowStatuses.Active);

#if DEBUG
            if (Debugger.IsAttached) scopes = scopes.Take(100);
#endif
            var records = scopes.Select(
                o => new ScopesFileModel
                {
                    OrganisationId = o.OrganisationId,
                    OrganisationName = o.Organisation.OrganisationName,
                    DUNSNumber = o.Organisation.DUNSNumber,
                    OrganisationReference = o.Organisation.OrganisationReference,
                    OrganisationScopeId = o.OrganisationScopeId,
                    ScopeStatus = o.ScopeStatus,
                    ScopeStatusDate = o.ScopeStatusDate,
                    RegisterStatus = o.RegisterStatus,
                    RegisterStatusDate = o.RegisterStatusDate,
                    ContactEmailAddress = o.ContactEmailAddress,
                    ContactFirstname = o.ContactFirstname,
                    ContactLastname = o.ContactLastname,
                    ContactJobTitle = o.ContactJobTitle,
                    ReadGuidance = o.ReadGuidance,
                    Reason = o.Reason,
                    TurnOver = o.TurnOver,
                    CampaignId = o.CampaignId
                });

            return records;
        }

        public async Task<HashSet<Organisation>> FixScopeRowStatusesAsync()
        {
            var lastSnapshotDate = DateTime.MinValue;
            long lastOrganisationId = -1;
            var index = -1;
            var count = 0;
            var scopes = _dataRepository.GetAll<OrganisationScope>()
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

            await _dataRepository.SaveChangesAsync();

            return changedOrgs;
        }

        public async Task<HashSet<Organisation>> SetPresumedScopesAsync()
        {
            var missingOrgs = await FindOrgsWhereScopeNotSetAsync();
            var changedOrgs = new HashSet<Organisation>();

            foreach (var org in missingOrgs)
                if (await SetPresumedScopesAsync(org.Organisation))
                    changedOrgs.Add(org.Organisation);

            if (changedOrgs.Count > 0) await _dataRepository.SaveChangesAsync();

            return changedOrgs;
        }

        public async Task<bool> SetPresumedScopesAsync(Organisation org)
        {
            var prevYearScope = ScopeStatuses.Unknown;
            var neverDeclaredScope = true;
            var changed = false;

            foreach (var reportingDeadline in _reportingDeadlineHelper.GetReportingDeadlines(org.SectorType))
            {
                var scope = org.GetActiveScope(reportingDeadline);

                // if we already have a scope then flag (prevYearScope, neverDeclaredScope) and skip this year
                if (scope != null && scope.ScopeStatus != ScopeStatuses.Unknown)
                {
                    prevYearScope = scope.ScopeStatus;
                    neverDeclaredScope = false;
                    continue;
                }

                // determine if need to presume scope
                var shouldPresumeScope = neverDeclaredScope
                                         && (prevYearScope == ScopeStatuses.PresumedOutOfScope
                                             || prevYearScope == ScopeStatuses.PresumedInScope
                                             || prevYearScope == ScopeStatuses.Unknown);

                // presumed scope from created date
                if (shouldPresumeScope)
                {
                    var createdAfterDeadlineYear = org.Created >= reportingDeadline;
                    if (createdAfterDeadlineYear)
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
                SetPresumedScopeStatus(org, prevYearScope, reportingDeadline);

                changed = true;
            }

            // set the latest scope if not set
            if (org.LatestScope == null)
            {
                org.LatestScope = org.GetLatestActiveScope();
                changed = true;
            }

            return changed;
        }

        public async Task<IList<OrganisationMissingScope>> FindOrgsWhereScopeNotSetAsync()
        {
            // get all orgs of any status
            var allOrgs = await _dataRepository.ToListAsync<Organisation>();
            allOrgs.SelectMany(o => o.OrganisationScopes).ToList();

            // find all orgs who have no scope or unknown scope statuses
            var orgsWithMissingScope = new ConcurrentBag<OrganisationMissingScope>();
            var privateDeadlines = _reportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Private);
            var publicDeadlines = _reportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Public);

            Parallel.ForEach(allOrgs, org =>
             {
                 var missingDeadlines = new List<DateTime>();

                 // for all snapshot years check if scope exists
                 var deadlines = org.SectorType == SectorTypes.Private ? privateDeadlines : publicDeadlines;
                 foreach (var reportingDeadline in deadlines)
                 {
                     var scope = org.GetActiveScope(reportingDeadline);
                     if (scope == null || scope.ScopeStatus == ScopeStatuses.Unknown) missingDeadlines.Add(reportingDeadline);
                 }

                 // collect
                 if (missingDeadlines.Count > 0)
                     orgsWithMissingScope.Add(
                         new OrganisationMissingScope { Organisation = org, MissingDeadlines = missingDeadlines });
             });

            return orgsWithMissingScope.ToList();
        }

        /// <summary>
        ///     Adds a new scope and updates the latest scope (if required)
        /// </summary>
        /// <param name="org"></param>
        /// <param name="scopeStatus"></param>
        /// <param name="reportingDeadline"></param>
        /// <param name="currentUser"></param>
        public virtual OrganisationScope SetPresumedScopeStatus(Organisation org,
            ScopeStatuses scopeStatus,
            DateTime reportingDeadline,
            User currentUser = null)
        {
            //Ensure scopestatus is presumed
            if (scopeStatus != ScopeStatuses.PresumedInScope && scopeStatus != ScopeStatuses.PresumedOutOfScope)
                throw new ArgumentOutOfRangeException(nameof(scopeStatus));

            //Check no previous scopes
            if (org.OrganisationScopes.Any(os => os.SubmissionDeadline == reportingDeadline))
                throw new ArgumentException($"A scope already exists for reporting deadline year {reportingDeadline.Year} for organisation reference '{org.OrganisationReference}'", nameof(scopeStatus));

            //Check for conflict with previous years scope
            if (reportingDeadline.Year - 1 > _reportingDeadlineHelper.FirstReportingDeadlineYear)
            {
                var previousScope = GetScopeStatusByReportingDeadlineOrLatestAsync(org, reportingDeadline.AddYears(-1));
                if (previousScope == ScopeStatuses.InScope && scopeStatus == ScopeStatuses.PresumedOutOfScope
                    || previousScope == ScopeStatuses.OutOfScope && scopeStatus == ScopeStatuses.PresumedInScope)
                    throw new ArgumentException(
                        $"Cannot set {scopeStatus} for snapshot year {reportingDeadline.Year} when previos year was {previousScope} for organisation reference '{org.OrganisationReference}'",
                        nameof(scopeStatus));
            }

            var newScope = new OrganisationScope
            {
                OrganisationId = org.OrganisationId,
                ContactEmailAddress = currentUser?.EmailAddress,
                ContactFirstname = currentUser?.Firstname,
                ContactLastname = currentUser?.Lastname,
                ContactJobTitle = currentUser?.JobTitle,
                ScopeStatus = scopeStatus,
                Status = ScopeRowStatuses.Active,
                StatusDetails = "Generated by the system",
                SubmissionDeadline = reportingDeadline
            };

            org.OrganisationScopes.Add(newScope);

            return newScope;
        }


        public virtual async Task<OrganisationScope> GetPendingScopeRegistrationAsync(string emailAddress)
        {
            var result = await _dataRepository.FirstOrDefaultByDescendingAsync<OrganisationScope, DateTime>(
                s => s.RegisterStatusDate,
                o => o.RegisterStatus == RegisterStatuses.RegisterPending && o.ContactEmailAddress == emailAddress);
            return result;
        }
    }
}