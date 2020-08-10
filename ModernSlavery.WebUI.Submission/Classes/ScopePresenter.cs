using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public interface IScopePresenter
    {
        // inteface
        ScopingViewModel CreateScopingViewModel(Organisation org, User currentUser);
        Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes, User currentUser);
        Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> snapshotYears);
        Task SavePresumedScopeAsync(ScopingViewModel model, int reportingStartYear);
    }

    public class ScopePresenter : IScopePresenter
    {
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic; 
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        private readonly ISharedBusinessLogic _sharedBusinessLogic;

        public ScopePresenter(IScopeBusinessLogic scopeBL,
            IDataRepository dataRepo,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic,
            ISharedBusinessLogic sharedBusinessLogic)
        {
            ScopeBusinessLogic = scopeBL;
            DataRepository = dataRepo;
            _organisationBusinessLogic = organisationBusinessLogic;
            _searchBusinessLogic = searchBusinessLogic;
            _sharedBusinessLogic = sharedBusinessLogic;
        }

        public IScopeBusinessLogic ScopeBusinessLogic { get; }
        private IDataRepository DataRepository { get; }

        public virtual async Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes,
            User currentUser)
        {
            // when NonStarterOrg doesn't exist then return
            return null;
        }

        public virtual ScopingViewModel CreateScopingViewModel(Organisation org, User currentUser)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));

            var model = new ScopingViewModel
            {
                OrganisationId = org.OrganisationId,
                DUNSNumber = org.DUNSNumber,
                OrganisationName = org.OrganisationName,
                OrganisationAddress = org.LatestAddress?.GetAddressString(),
                AccountingDate = _sharedBusinessLogic.GetReportingStartDate(org.SectorType)
            };
            model.EnterCodes.EmployerReference = org.EmployerReference;

            // get the scope info for this year
            var scope = ScopeBusinessLogic.GetScopeByReportingDeadlineOrLatestAsync(org, model.AccountingDate.AddYears(1));
            if (scope != null)
                model.ThisScope = new ScopeViewModel
                {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    SnapshotDate = scope.SubmissionDeadline
                };

            // get the scope info for last year
            scope = ScopeBusinessLogic.GetScopeByReportingDeadlineOrLatestAsync(org, model.AccountingDate);
            if (scope != null)
                model.LastScope = new ScopeViewModel
                {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    SnapshotDate = scope.SubmissionDeadline
                };

            //Check if the user is registered for this organisation
            model.UserIsRegistered =
                currentUser != null && org.UserOrganisations.Any(uo => uo.UserId == currentUser.UserId);

            return model;
        }

        public virtual async Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> snapshotYears)
        {
            if (string.IsNullOrWhiteSpace(model.EnterCodes.EmployerReference))
                throw new ArgumentNullException(nameof(model.EnterCodes.EmployerReference));

            if (model.IsSecurityCodeExpired) throw new ArgumentOutOfRangeException(nameof(model.IsSecurityCodeExpired));

            if (!snapshotYears.Any()) throw new ArgumentNullException(nameof(snapshotYears));

            //Get the organisation with this employer reference
            var org = model.OrganisationId == 0
                ? null
                : await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                    o.OrganisationId == model.OrganisationId);
            if (org == null)
                throw new ArgumentOutOfRangeException(
                    nameof(model.OrganisationId),
                    $"Cannot find organisation with Id: {model.OrganisationId} in the database");

            var newScopes = new List<OrganisationScope>();

            foreach (var snapshotYear in snapshotYears.OrderByDescending(y => y))
            {
                var scope = new OrganisationScope
                {
                    OrganisationId = org.OrganisationId,
                    ContactEmailAddress = model.EnterAnswers.EmailAddress,
                    ContactFirstname = model.EnterAnswers.FirstName,
                    ContactLastname = model.EnterAnswers.LastName,
                    ReadGuidance = model.EnterAnswers.HasReadGuidance(),
                    Reason = model.EnterAnswers.Reason != "Other"
                        ? model.EnterAnswers.Reason
                        : model.EnterAnswers.OtherReason,
                    ScopeStatus = model.IsOutOfScopeJourney ? ScopeStatuses.OutOfScope : ScopeStatuses.InScope,
                    CampaignId = model.CampaignId,
                    // set the snapshot date according to sector
                    SubmissionDeadline = _sharedBusinessLogic.GetReportingStartDate(org.SectorType, snapshotYear)
                };
                newScopes.Add(scope);
            }

            await ScopeBusinessLogic.SaveScopesAsync(org, newScopes);
            await _searchBusinessLogic.UpdateSearchIndexAsync(org);
        }

        public virtual async Task SavePresumedScopeAsync(ScopingViewModel model, int reportingDeadlineYear)
        {
            if (string.IsNullOrWhiteSpace(model.EnterCodes.EmployerReference))
                throw new ArgumentNullException(nameof(model.EnterCodes.EmployerReference));

            if (model.IsSecurityCodeExpired) throw new ArgumentOutOfRangeException(nameof(model.IsSecurityCodeExpired));

            // get the organisation by EmployerReference
            var org = await GetOrgByEmployerReferenceAsync(model.EnterCodes.EmployerReference);
            if (org == null)
                throw new ArgumentOutOfRangeException(
                    nameof(model.EnterCodes.EmployerReference),
                    $"Cannot find organisation with EmployerReference: {model.EnterCodes.EmployerReference} in the database");

            // can only save a presumed scope in the prev or current snapshot year
            var currentReportingDeadline = _sharedBusinessLogic.GetReportingDeadline(org.SectorType);
            var reportingDeadline = _sharedBusinessLogic.GetReportingDeadline(org.SectorType,reportingDeadlineYear);

            if (reportingDeadline.Year > currentReportingDeadline.Year || reportingDeadline.Year < currentReportingDeadline.Year - 1)
                throw new ArgumentOutOfRangeException(nameof(reportingDeadline));

            // skip saving a presumed scope when an active scope already exists for the snapshot year
            if (await ScopeBusinessLogic.GetScopeByReportingDeadlineOrLatestAsync(org.OrganisationId, reportingDeadline) !=
                null) return;

            // create the new OrganisationScope
            var newScope = new OrganisationScope
            {
                OrganisationId = org.OrganisationId,
                ContactEmailAddress = model.EnterAnswers.EmailAddress,
                ContactFirstname = model.EnterAnswers.FirstName,
                ContactLastname = model.EnterAnswers.LastName,
                ReadGuidance = model.EnterAnswers.HasReadGuidance(),
                Reason = "",
                ScopeStatus = model.IsOutOfScopeJourney
                    ? ScopeStatuses.PresumedOutOfScope
                    : ScopeStatuses.PresumedInScope,
                CampaignId = model.CampaignId,
                // set the snapshot date according to sector
                SubmissionDeadline = _sharedBusinessLogic.GetReportingStartDate(org.SectorType, reportingDeadlineYear),
                StatusDetails = "Generated by the system"
            };

            // save the presumed scope
            await ScopeBusinessLogic.SaveScopeAsync(org, true, newScope);
        }

        public async Task<Organisation> GetOrgByEmployerReferenceAsync(string employerReference)
        {
            var org = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.EmployerReference == employerReference);
            return org;
        }
    }
}