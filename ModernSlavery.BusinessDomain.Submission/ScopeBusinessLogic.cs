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

            var orgScope = organisation.GetActiveScope(reportingDeadline.Value);

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
            if (oldOrgScope == null) throw new ArgumentOutOfRangeException($"Cannot find an scope with status 'Active' for reporting deadline '{reportingDeadline}' linked to organisation '{organisation.OrganisationName}', organisationReference '{organisation.OrganisationReference}'.");

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
            var allOrgs = await _dataRepository.ToListAsync<Organisation>();
            allOrgs.SelectMany(o => o.OrganisationScopes).ToList();

            var changedOrgs = new ConcurrentBag<Organisation>();
            var privateDeadlines = _reportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Private);
            var publicDeadlines = _reportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Public);

            Parallel.ForEach(allOrgs, async org =>
            {
                if (await org.SetPresumedScopesAsync(org.SectorType== SectorTypes.Public ? publicDeadlines : privateDeadlines))
                    changedOrgs.Add(org);
            });

            if (changedOrgs.Count>0) await _dataRepository.SaveChangesAsync();

            return changedOrgs.ToHashSet();
        }
    }
}