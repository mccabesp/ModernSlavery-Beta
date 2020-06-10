using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    // STATEMENTMETADATA

    public interface INewSubmissionPresenter
    {
        /// <summary>
        /// Returns the result of trying to get your statement.
        /// The action result will be the view.
        /// </summary>
        Task<CustomResult<StatementMetadataViewModel>> TryGetYourStatement();

        /// <summary>
        /// Save the current submission draft which is only visible to the current user.
        /// </summary>
        Task<CustomResult<StatementMetadataViewModel>> TrySaveYourStatement(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetCompliance();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveCompliance(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetYourOrganisation();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveYourOrgansation(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetSupplyChainRisk();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveSupplyChainRisk(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetPolicies();

        Task<CustomResult<StatementMetadataViewModel>> TrySavePolicies(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetDueDiligence();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveDueDiligence(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetTraining();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveTraining(StatementMetadataViewModel model);

        Task<CustomResult<StatementMetadataViewModel>> TryGetMonitoringInProgress();

        Task<CustomResult<StatementMetadataViewModel>> TrySaveMonitorInProgress(StatementMetadataViewModel model);

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

    public class NewSubmissionPresenter : INewSubmissionPresenter
    {
        public NewSubmissionPresenter()
        {
        }

        #region Step 1 - Your statement

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetYourStatement()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveYourStatement(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 2 - Compliance

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetCompliance()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveCompliance(StatementMetadataViewModel model)
        {
            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 3 - Your organisation

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetYourOrganisation()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveYourOrgansation(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 4 - Supply chain risks

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetSupplyChainRisk()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveSupplyChainRisk(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 5 - Policies

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetPolicies()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySavePolicies(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 6 - Due diligence

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetDueDiligence()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveDueDiligence(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 7 - Training

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetTraining()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveTraining(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 8 - Monitoring progress

        public async Task<CustomResult<StatementMetadataViewModel>> TryGetMonitoringInProgress()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<StatementMetadataViewModel>> TrySaveMonitorInProgress(StatementMetadataViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 9 - Review TODO
        #endregion

        private async Task<bool> CanAccessStatementMetadata()
        {
            // This goes down to the business logic layer

            #region Failure 0: Another person has this statement metadata locked
            #endregion

            #region Failure 1: can the user view your statement
            #endregion

            #region Failure 2: can the user edit the current statement for this organisation
            #endregion

            #region Failure 3: is the statement in a state that can be edited, eg not submitted
            #endregion

            #region Failure 4: should the user be redirected
            #endregion

            throw new NotImplementedException();
        }

        private async Task<CustomResult<StatementMetadataViewModel>> TryGetViewModel()
        {
            if (!(await CanAccessStatementMetadata()))
                return new CustomResult<StatementMetadataViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            // using current user, organisation, reporting year
            // decide which repo to query: DB or File or session ?
            // or create new viewmodel

            throw new NotImplementedException();
        }

        #region Draft

        async Task<CustomResult<StatementMetadataViewModel>> SaveDraftForUser(StatementMetadataViewModel model)
        {
            if (!(await CanAccessStatementMetadata()))
                return new CustomResult<StatementMetadataViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            // how to handle multiple validation errors?
            await ValidateForDraft(model);

            throw new NotImplementedException();
        }

        public async Task SubmitDraftForOrganisation()
        {
            throw new NotImplementedException();
        }

        public async Task ClearDraftForUser()
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

        #region REVIEW

        /// <summary>
        /// Gets the first page of the workflow for an appropriate redirect.
        /// Will return null when there is no need to redirect.
        /// </summary>
        public string GetStartingPage()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    // Is this needed?
    enum ExpectedErrorCodes
    {

    }

    public enum SubmissionStep
    {
        NotStarted,
        YourStatement,
        Compliance,
        YourOrganisation,
        SupplyChainRisks,
        Policies,
        DueDiligence,
        Training,
        MentoringProcess,
        Review
    }
}
