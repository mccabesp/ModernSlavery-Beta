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
    public interface INewSubmissionPresenter
    {
        /// <summary>
        /// Returns the result of trying to get your statement.
        /// The action result will be the view.
        /// </summary>
        Task<CustomResult<NewSubmissionViewModel>> TryGetYourStatement();

        /// <summary>
        /// Save the current submission draft which is only visible to the current user.
        /// </summary>
        Task<CustomResult<NewSubmissionViewModel>> TrySaveYourStatement(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetCompliance();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveCompliance(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetYourOrganisation();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveYourOrgansation(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetSupplyChainRisk();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveSupplyChainRisk(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetPolicies();

        Task<CustomResult<NewSubmissionViewModel>> TrySavePolicies(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetDueDiligence();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveDueDiligence(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetTraining();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveTraining(NewSubmissionViewModel model);

        Task<CustomResult<NewSubmissionViewModel>> TryGetMonitoringInProgress();

        Task<CustomResult<NewSubmissionViewModel>> TrySaveMonitorInProgress(NewSubmissionViewModel model);

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
        Task ValidateForDraft(NewSubmissionViewModel model);

        /// <summary>
        /// Validate the draft and ensure there are no empty field.
        /// </summary>
        Task ValidateForSubmission(NewSubmissionViewModel model);

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

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetYourStatement()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveYourStatement(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 2 - Compliance

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetCompliance()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveCompliance(NewSubmissionViewModel model)
        {
            if (!(await CanAccessDraft()))
                return new CustomResult<NewSubmissionViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 3 - Your organisation

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetYourOrganisation()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveYourOrgansation(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 4 - Supply chain risks

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetSupplyChainRisk()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveSupplyChainRisk(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 5 - Policies

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetPolicies()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySavePolicies(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 6 - Due diligence

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetDueDiligence()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveDueDiligence(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 7 - Training

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetTraining()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveTraining(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 8 - Monitoring progress

        public async Task<CustomResult<NewSubmissionViewModel>> TryGetMonitoringInProgress()
        {
            return await TryGetViewModel();
        }

        public async Task<CustomResult<NewSubmissionViewModel>> TrySaveMonitorInProgress(NewSubmissionViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(model);
        }

        #endregion

        #region Step 9 - Review TODO
        #endregion

        private async Task<bool> CanAccessDraft()
        {
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

        private async Task<CustomResult<NewSubmissionViewModel>> TryGetViewModel()
        {
            if (!(await CanAccessDraft()))
                return new CustomResult<NewSubmissionViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            // using current user, organisation, reporting year
            // decide which repo to query: DB or File or session ?
            // or create new viewmodel

            throw new NotImplementedException();
        }

        #region Draft

        async Task<CustomResult<NewSubmissionViewModel>> SaveDraftForUser(NewSubmissionViewModel model)
        {
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

        public async Task ValidateForDraft(NewSubmissionViewModel model)
        {
            // properties with no data should not be validated

            throw new NotImplementedException();
        }

        public async Task ValidateForSubmission(NewSubmissionViewModel model)
        {
            // Validate all the things!

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
