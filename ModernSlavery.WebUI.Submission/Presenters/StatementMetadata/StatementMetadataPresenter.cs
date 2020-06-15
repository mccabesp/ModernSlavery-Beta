using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    public interface IStatementMetadataPresenter
    {
        /// <summary>
        /// Returns the result of trying to get your statement.
        /// The action result will be the view.
        /// </summary>
        Task<CustomResult<StatementMetadataViewModel>> TryGetYourStatement(User user, string organisationIdentifier, int year);

        /// <summary>
        /// Save the current submission draft which is only visible to the current user.
        /// </summary>
        Task<CustomResult<StatementMetadataViewModel>> TrySaveYourStatement(User user, StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetCompliance(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySaveCompliance(User user, StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetYourOrganisation(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySaveYourOrgansation(User user, StatementMetadataViewModel model);
 
        Task<CustomResult<StatementMetadataViewModel>> TryGetPolicies(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySavePolicies(User user, StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetSupplyChainRiskAndDueDiligence(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySaveSupplyChainRiskAndDueDiligence(User user, StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetTraining(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySaveTraining(User user, StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetMonitoringInProgress(User user, string organisationIdentifier, int year);

        Task<CustomResult<StatementMetadataViewModel>> TrySaveMonitorInProgress(User user, StatementMetadataViewModel model);

        /// <summary>
        /// Save and then submit the users current draft for the organisation
        /// </summary>
        Task SubmitDraftForOrganisation();

        /// <summary>
        /// Clear the draft that is saved for the current user.
        /// </summary>
        Task ClearDraftForUser();

        /// <summary>
        /// Validate the draft, allowing empty entries.
        /// </summary>
        Task ValidateForDraft(StatementMetadataViewModel model);

        /// <summary>
        /// Validate the draft and ensure there are no empty field.
        /// </summary>
        Task ValidateForSubmission(StatementMetadataViewModel model);

        /// <summary>
        /// Gets the next action in the submission workflow.
        /// </summary>
        Task<string> GetNextRedirection(SubmissionStep step);

        /// <summary>
        /// Get the redirect action for cancelling.
        /// </summary>
        Task<string> GetCancelRedirection();
    }

    public class StatementMetadataPresenter : IStatementMetadataPresenter
    {
        // something that should be unique to this class only
        const string SessionKey = "StatementMetadataPresenter";

        readonly IStatementMetadataBusinessLogic StatementMetadataBusinessLogic;

        // required for accessing session
        readonly IHttpContextAccessor HttpContextAccessor;

        readonly ISharedBusinessLogic SharedBusinessLogic;

        public StatementMetadataPresenter(
            ISharedBusinessLogic sharedBusinessLogic,
            IStatementMetadataBusinessLogic statementMetadataBusinessLogic,
            IHttpContextAccessor httpContextAccessor)
        {
            SharedBusinessLogic = sharedBusinessLogic;
            StatementMetadataBusinessLogic = statementMetadataBusinessLogic;
            HttpContextAccessor = httpContextAccessor;
        }

        #region Step 1 - Your statement

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetYourStatement(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveYourStatement(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 2 - Compliance

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetCompliance(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveCompliance(User user, StatementMetadataViewModel model)
        {
            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 3 - Your organisation

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetYourOrganisation(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveYourOrgansation(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 4 - Policies

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetPolicies(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySavePolicies(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetSupplyChainRiskAndDueDiligence(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveSupplyChainRiskAndDueDiligence(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 6 - Training

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetTraining(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveTraining(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 7 - Monitoring progress

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetMonitoringInProgress(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveMonitorInProgress(User user, StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 8 - Review TODO
        #endregion

        private async Task<CustomResult<StatementMetadataViewModel>> TryGetViewModel(User user, string organisationIdentifier, int year)
        {
            var id = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var organisation = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(x => x.OrganisationId == id);

            var actionresult = await StatementMetadataBusinessLogic.CanAccessStatementMetadata(user, organisation, year);
            if (actionresult != StatementActionResult.Success)
                // is this the correct form of error?
                return new CustomResult<StatementMetadataViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            // check session stashed vm
            var sessionVm = GetSessionVM();

            if (sessionVm != null)
            {
                // check the sessionVm matches input parameters
                // if it does not, do NOT return it
                if (sessionVm.OrganisationId == organisation.OrganisationId && sessionVm.Year != year)
                    // everything is valid, return from session
                    return new CustomResult<StatementMetadataViewModel>(sessionVm);

                // session is invalid in some way and has to be cleared
                DeleteSessionVM();
            }

            // Check business logic layer
            // that should query file and DB
            var entity = await StatementMetadataBusinessLogic.GetStatementMetadataByOrganisationAndYear(organisation, year);

            if (entity != null)
            {
                // shouldnt need to check it for access as that was already done
                var vm = MapToVM(entity);
                return new CustomResult<StatementMetadataViewModel>(vm);
            }

            // everything else was empty
            // return a new instance of the viewmodel
            return new CustomResult<StatementMetadataViewModel>(new StatementMetadataViewModel
            {
            });
        }

        #region Draft

        async Task<CustomResult<StatementMetadataViewModel>> SaveDraftForUser(User user, StatementMetadataViewModel model)
        {
            var organisation = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(x => x.OrganisationId == model.OrganisationId);
            var actionresult = await StatementMetadataBusinessLogic.CanAccessStatementMetadata(user, organisation, model.Year);

            if (actionresult != StatementActionResult.Success)
                // is this the correct form of error?
                return new CustomResult<StatementMetadataViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            var entity = MapToEntity(model);
            await StatementMetadataBusinessLogic.SaveStatementMetadata(user, entity);

            throw new NotImplementedException();
        }

        public async Task SubmitDraftForOrganisation()
        {
            throw new NotImplementedException();
        }

        public async Task ClearDraftForUser()
        {
            throw new NotImplementedException();
            DeleteSessionVM();
        }

        #endregion

        #region Mapping

        StatementMetadataViewModel MapToVM(StatementMetadata entity)
        {
            throw new NotImplementedException();
        }

        StatementMetadata MapToEntity(StatementMetadataViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Validation

        // Validation should happen at lower levels,
        // eg SubmissionService/SubmissionBusinessLogic

        public async Task ValidateForDraft(StatementMetadataViewModel model)
        {
            // properties with no data should not be validated

            throw new NotImplementedException();
        }

        public async Task ValidateForSubmission(StatementMetadataViewModel model)
        {
            // Validate everything

            throw new NotImplementedException();
        }

        #endregion

        #region Redirection

        /// <summary>
        /// 
        /// </summary>
        public async Task<string> GetNextRedirection(SubmissionStep step)
        {
            switch (step)
            {
                case SubmissionStep.NotStarted:
                    break;
                case SubmissionStep.YourStatement:
                    break;
                case SubmissionStep.Compliance:
                    break;
                case SubmissionStep.YourOrganisation:
                    break;
                case SubmissionStep.SupplyChainRisks:
                    break;
                case SubmissionStep.Policies:
                    break;
                case SubmissionStep.DueDiligence:
                    break;
                case SubmissionStep.Training:
                    break;
                case SubmissionStep.MentoringProcess:
                    break;
                case SubmissionStep.Review:
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the redirect location when cancelling.
        /// </summary>
        public async Task<string> GetCancelRedirection()
        {
            throw new NotImplementedException();
        }

        #endregion

        private StatementMetadataViewModel GetSessionVM()
        {
            // originally taken from BaseController.UnstashModel
            var json = HttpContextAccessor.HttpContext.Session.GetString(SessionKey + ":Model");
            var result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonConvert.DeserializeObject<StatementMetadataViewModel>(json);

            return result;
        }

        private void DeleteSessionVM()
        {
            HttpContextAccessor.HttpContext.Session.Remove(SessionKey + ":Model");
        }
    }

    public enum SubmissionStep : byte
    {
        Unknown = 0,
        NotStarted = 1,
        YourStatement = 2,
        Compliance = 3,
        YourOrganisation = 4,
        SupplyChainRisks = 5,
        Policies = 6,
        DueDiligence = 7,
        Training = 8,
        MentoringProcess = 9,
        Review = 10
    }
}
