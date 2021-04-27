using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Classes
{
    public interface IScopePresenter
    {
        Task<ScopingViewModel> CreateScopingViewModelAsync(Organisation org, User currentUser, int reportingDeadlineYear);
        Task<ScopingViewModel> CreateScopingViewModelAsync(Organisation org, User currentUser, DateTime reportingDeadline);
        Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes, User currentUser);
        Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> years);
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

        public virtual async Task<ScopingViewModel> CreateScopingViewModelAsync(EnterCodesViewModel enterCodes,User currentUser)
        {
            // when NonStarterOrg doesn't exist then return null
            var org = await _organisationBusinessLogic.GetOrganisationByOrganisationReferenceAndSecurityCodeAsync(enterCodes.OrganisationReference, enterCodes.SecurityToken);
            if (org == null) return null;

            var reportingDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(org.SectorType);
            var scope = await CreateScopingViewModelAsync(org, currentUser, reportingDeadline);
            //TODO: clarify if we should be showing this security token when navigate back to page or if it's a security issue?
            scope.EnterCodes.SecurityToken = org.SecurityCode;
            scope.IsSecurityCodeExpired = org.HasSecurityCodeExpired();

            return scope;
        }

        public virtual async Task<ScopingViewModel> CreateScopingViewModelAsync(Organisation org, User currentUser, int reportingDeadlineYear)
        {
            var reportingDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(org.SectorType, reportingDeadlineYear);
            return await CreateScopingViewModelAsync(org, currentUser, reportingDeadline);
        }

        public virtual async Task<ScopingViewModel> CreateScopingViewModelAsync(Organisation org, User currentUser, DateTime reportingDeadline)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));

            var model = new ScopingViewModel
            {
                OrganisationId = org.OrganisationId,
                DUNSNumber = org.DUNSNumber,
                OrganisationName = org.OrganisationName,
                OrganisationAddress = org.LatestAddress?.GetAddressString(),
                DeadlineDate = reportingDeadline
            };
            model.EnterCodes.OrganisationReference = org.OrganisationReference;

            //Ensure presumed scopes are set
            if (!org.OrganisationScopes.Any())
                await ScopeBusinessLogic.SetPresumedScopesAsync(org);

            // get the scope info for this year
            var scope = await ScopeBusinessLogic.GetScopeByReportingDeadlineOrLatestAsync(org, model.DeadlineDate);

            if (scope != null)
                model.ThisScope = new ScopeViewModel
                {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    DeadlineDate = scope.SubmissionDeadline
                };

            // get the scope info for last year
            scope = await ScopeBusinessLogic.GetScopeByReportingDeadlineOrLatestAsync(org, model.DeadlineDate.AddYears(-1));
            if (scope != null)
                model.LastScope = new ScopeViewModel
                {
                    OrganisationScopeId = scope.OrganisationScopeId,
                    ScopeStatus = scope.ScopeStatus,
                    StatusDate = scope.ScopeStatusDate,
                    RegisterStatus = scope.RegisterStatus,
                    DeadlineDate = scope.SubmissionDeadline
                };

            //Check if the user is registered for this organisation
            model.UserIsRegistered =
                currentUser != null && org.UserOrganisations.Any(uo => uo.UserId == currentUser.UserId);

            return model;
        }

        public virtual async Task SaveScopesAsync(ScopingViewModel model, IEnumerable<int> years)
        {
            if (string.IsNullOrWhiteSpace(model.EnterCodes.OrganisationReference))
                throw new ArgumentNullException(nameof(model.EnterCodes.OrganisationReference));

            if (model.IsSecurityCodeExpired) throw new ArgumentOutOfRangeException(nameof(model.IsSecurityCodeExpired));

            if (!years.Any()) throw new ArgumentNullException(nameof(years));

            //Get the organisation with this organisation reference
            var org = model.OrganisationId == 0
                ? null
                : await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                    o.OrganisationId == model.OrganisationId);
            if (org == null)
                throw new ArgumentOutOfRangeException(
                    nameof(model.OrganisationId),
                    $"Cannot find organisation with Id: {model.OrganisationId} in the database");

            var newScopes = new List<OrganisationScope>();

            foreach (var year in years.OrderByDescending(y => y))
            {
                var scope = new OrganisationScope
                {
                    OrganisationId = org.OrganisationId,
                    ContactEmailAddress = model.EnterAnswers.EmailAddress,
                    ContactFirstname = model.EnterAnswers.FirstName,
                    ContactLastname = model.EnterAnswers.LastName,
                    ContactJobTitle = model.EnterAnswers.JobTitle,
                    Reason = model.EnterAnswers.Reason,
                    TurnOver = model.EnterAnswers.TurnOver,
                    ScopeStatus = model.IsOutOfScopeJourney ? ScopeStatuses.OutOfScope : ScopeStatuses.InScope,
                    CampaignId = model.CampaignId,
                    // set the deadline date according to sector
                    SubmissionDeadline = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(org.SectorType, year)
                };
                newScopes.Add(scope);
            }

            await ScopeBusinessLogic.SaveScopesAsync(org, newScopes);
            await _searchBusinessLogic.RefreshSearchDocumentsAsync(org);
        }

        public async Task<Organisation> GetOrgByOrganisationReferenceAsync(string organisationReference)
        {
            var org = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(o =>
                o.OrganisationReference == organisationReference);
            return org;
        }
    }
}