﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Submission.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    public interface IStatementPresenter
    {
        /// <summary>
        /// Returns the result of trying to get your statement.
        /// The action result will be the view.
        /// </summary>
        Task<CustomResult<StatementViewModel>> TryGetYourStatement(User user, string organisationIdentifier, int year);

        /// <summary>
        /// Save the current submission draft which is only visible to the current user.
        /// </summary>
        Task<StatementActionResult> TrySaveYourStatement(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetCompliance(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveCompliance(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetYourOrganisation(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveYourOrgansation(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetPolicies(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySavePolicies(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetSupplyChainRiskAndDueDiligence(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveSupplyChainRiskAndDueDiligence(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetTraining(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveTraining(User user, StatementViewModel model);

        Task<CustomResult<StatementViewModel>> TryGetMonitoringInProgress(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveMonitorInProgress(User user, StatementViewModel model);

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
        Task ValidateForDraft(StatementViewModel model);

        /// <summary>
        /// Validate the draft and ensure there are no empty field.
        /// </summary>
        Task ValidateForSubmission(StatementViewModel model);

        /// <summary>
        /// Gets the next action in the submission workflow.
        /// </summary>
        Task<string> GetNextRedirectAction(SubmissionStep step);

        /// <summary>
        /// Get the redirect action for cancelling.
        /// </summary>
        Task<string> GetCancelRedirection();
    }

    public class StatementPresenter : IStatementPresenter
    {
        // class will NOT provide enough uniqueness, think multiple open tabs
        // the key will have to be constructed out of parameters in the url - org and year
        const string SessionKey = "StatementPresenter";

        readonly IStatementBusinessLogic StatementBusinessLogic;

        // required for accessing session
        readonly IHttpContextAccessor HttpContextAccessor;

        readonly ISharedBusinessLogic SharedBusinessLogic;

        readonly IMapper Mapper;

        public StatementPresenter(
            IMapper mapper,
            ISharedBusinessLogic sharedBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic,
            IHttpContextAccessor httpContextAccessor)
        {
            Mapper = mapper;
            SharedBusinessLogic = sharedBusinessLogic;
            StatementBusinessLogic = statementBusinessLogic;
            HttpContextAccessor = httpContextAccessor;
        }

        #region Step 1 - Your statement

        public async Task<CustomResult<StatementViewModel>> TryGetYourStatement(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveYourStatement(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 2 - Compliance

        public async Task<CustomResult<StatementViewModel>> TryGetCompliance(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveCompliance(User user, StatementViewModel model)
        {
            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 3 - Your organisation

        public async Task<CustomResult<StatementViewModel>> TryGetYourOrganisation(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveYourOrgansation(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 4 - Policies

        public async Task<CustomResult<StatementViewModel>> TryGetPolicies(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySavePolicies(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        public async Task<CustomResult<StatementViewModel>> TryGetSupplyChainRiskAndDueDiligence(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveSupplyChainRiskAndDueDiligence(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 6 - Training

        public async Task<CustomResult<StatementViewModel>> TryGetTraining(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveTraining(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 7 - Monitoring progress

        public async Task<CustomResult<StatementViewModel>> TryGetMonitoringInProgress(User user, string organisationIdentifier, int year)
        {
            return await TryGetViewModel(user, organisationIdentifier, year);
        }

        public async Task<StatementActionResult> TrySaveMonitorInProgress(User user, StatementViewModel model)
        {
            await ValidateForDraft(model);

            return await SaveDraftForUser(user, model);
        }

        #endregion

        #region Step 8 - Review TODO
        #endregion

        private async Task<CustomResult<StatementViewModel>> TryGetViewModel(User user, string organisationIdentifier, int year)
        {
            var id = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var organisation = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(x => x.OrganisationId == id);

            var actionresult = await StatementBusinessLogic.CanAccessStatement(user, organisation, year);
            if (actionresult != StatementActionResult.Success)
                // is this the correct form of error?
                return new CustomResult<StatementViewModel>(new CustomError(System.Net.HttpStatusCode.Unauthorized, "Unauthorised access"));

            // check session stashed vm
            var sessionVm = GetSessionVM();

            if (sessionVm != null)
            {
                // check the sessionVm matches input parameters
                // if it does not, do NOT return it
                if (sessionVm.OrganisationId == organisation.OrganisationId && sessionVm.Year != year)
                    // everything is valid, return from session
                    return new CustomResult<StatementViewModel>(sessionVm);

                // session is invalid in some way and has to be cleared
                DeleteSessionVM();
            }

            // Check business logic layer
            // that should query file and DB
            var entity = await StatementBusinessLogic.GetStatementByOrganisationAndYear(organisation, year);

            if (entity == null)
            {
                return new CustomResult<StatementViewModel>(new StatementViewModel
                {
                    OrganisationIdentifier = organisationIdentifier,
                    Year = year
                });
            }

            // shouldnt need to check it for access as that was already done
            var vm = MapToVM(entity);
            return new CustomResult<StatementViewModel>(vm);
        }

        #region Draft

        async Task<StatementActionResult> SaveDraftForUser(User user, StatementViewModel viewmodel)
        {
            var id = SharedBusinessLogic.Obfuscator.DeObfuscate(viewmodel.OrganisationIdentifier);
            var organisation = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Organisation>(x => x.OrganisationId == id);

            var actionresult = await StatementBusinessLogic.CanAccessStatement(user, organisation, viewmodel.Year);
            if (actionresult != StatementActionResult.Success)
                // is this the correct form of error?
                return actionresult;

            var model = MapToModel(viewmodel);
            var saveResult = await StatementBusinessLogic.SaveStatement(user, model);

            return actionresult;
        }

        public async Task SubmitDraftForOrganisation()
        {
            throw new NotImplementedException();
        }

        public async Task ClearDraftForUser()
        {
            DeleteSessionVM();
        }

        #endregion

        #region Mapping

        StatementViewModel MapToVM(StatementModel model)
        {
            return Mapper.Map<StatementViewModel>(model);
        }

        StatementModel MapToModel(StatementViewModel viewModel)
        {
            return Mapper.Map<StatementModel>(viewModel);
        }

        #endregion

        #region Validation

        // Validation should happen at lower levels,
        // eg SubmissionService/SubmissionBusinessLogic

        public Task ValidateForDraft(StatementViewModel model)
        {
            return Task.CompletedTask;
        }

        public Task ValidateForSubmission(StatementViewModel model)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Redirection

        /// <summary>
        /// 
        /// </summary>
        public async Task<string> GetNextRedirectAction(SubmissionStep step)
        {
            switch (step)
            {
                case SubmissionStep.NotStarted:
                    return nameof(StatementController.YourStatement);
                case SubmissionStep.YourStatement:
                    return nameof(StatementController.Compliance);
                case SubmissionStep.Compliance:
                case SubmissionStep.YourOrganisation:
                case SubmissionStep.Policies:
                case SubmissionStep.SupplyChainRisksAndDueDiligence:
                case SubmissionStep.Training:
                case SubmissionStep.MentoringProcess:
                case SubmissionStep.Review:
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get the redirect location when cancelling.
        /// </summary>
        public async Task<string> GetCancelRedirection()
        {
            throw new NotImplementedException();
        }

        #endregion

        private StatementViewModel GetSessionVM()
        {
            // originally taken from BaseController.UnstashModel
            var json = HttpContextAccessor.HttpContext.Session.GetString(SessionKey + ":Model");
            var result = string.IsNullOrWhiteSpace(json)
                ? null
                : JsonConvert.DeserializeObject<StatementViewModel>(json);

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
        Policies = 5,
        SupplyChainRisksAndDueDiligence = 6,
        Training = 7,
        MentoringProcess = 8,
        Review = 9
    }

    public class StatementMapperProfile : Profile
    {
        public StatementMapperProfile()
        {
            CreateMap<StatementModel, StatementViewModel>()
                // These need obfuscating
                .ForMember(dest => dest.StatementIdentifier, opt => opt.Ignore())
                .ForMember(dest => dest.OrganisationIdentifier, opt => opt.Ignore())
                // These need to change on VM to come from DB
                .ForMember(dest => dest.StatementPolicies, opt => opt.Ignore())
                .ForMember(dest => dest.StatementTrainings, opt => opt.Ignore())
                .ForMember(dest => dest.StatementRisks, opt => opt.Ignore())
                .ForMember(dest => dest.StatementSectors, opt => opt.Ignore())
                .ForMember(dest => dest.StatementDiligences, opt => opt.Ignore())
                // These need mapping for correct type on vm
                .ForMember(dest => dest.StatementRiskTypes, opt => opt.Ignore())
                .ForMember(dest => dest.StatementDiligenceTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Countries, opt => opt.Ignore())
                // Work out storage of these
                .ForMember(dest => dest.IncludedOrganistionCount, opt => opt.Ignore())
                .ForMember(dest => dest.ExcludedOrganisationCount, opt => opt.Ignore())
                .ForMember(dest => dest.Continents, opt => opt.Ignore())
                .ForMember(dest => dest.AnyIdicatorsInSupplyChain, opt => opt.Ignore())
                .ForMember(dest => dest.IndicatorDetails, opt => opt.Ignore())
                .ForMember(dest => dest.AnyInstancesInSupplyChain, opt => opt.Ignore())
                .ForMember(dest => dest.InstanceDetails, opt => opt.Ignore())
                .ForMember(dest => dest.StatementRemediations, opt => opt.Ignore())
                .ForMember(dest => dest.OtherRemediationText, opt => opt.Ignore())
                .ForMember(dest => dest.NumberOfYearsOfStatements, opt => opt.Ignore())
                .ForMember(dest => dest.LastFinancialYearBudget, opt => opt.Ignore());
        }
    }
}
