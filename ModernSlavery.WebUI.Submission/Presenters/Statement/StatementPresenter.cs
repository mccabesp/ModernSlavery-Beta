using AutoMapper;
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

            var model = await StatementBusinessLogic.GetStatementByOrganisationAndYear(organisation, viewmodel.Year);
            model = MapToModel(model, viewmodel);
            var saveResult = await StatementBusinessLogic.SaveDraftStatement(user, model);

            return actionresult;
        }

        public async Task SubmitDraftForOrganisation()
        {
            throw new NotImplementedException();
        }

        public Task ClearDraftForUser()
        {
            // Delete the draft
            // restore the backup
            return Task.CompletedTask;
        }

        #endregion

        #region Mapping

        StatementViewModel MapToVM(StatementModel model)
        {
            var result = Mapper.Map<StatementViewModel>(model);
            result.OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(model.OrganisationId);
            return result;
        }

        StatementModel MapToModel(StatementModel destination, StatementViewModel source)
        {
            var result = Mapper.Map<StatementViewModel, StatementModel>(source, destination);
            result.OrganisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(source.OrganisationIdentifier);
            return result;
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
                    return nameof(StatementController.YourOrganisation);
                case SubmissionStep.YourOrganisation:
                    return nameof(StatementController.Policies);
                case SubmissionStep.Policies:
                    return nameof(StatementController.SupplyChainRisks);
                case SubmissionStep.SupplyChainRisks:
                    return nameof(StatementController.DueDiligence);
                case SubmissionStep.DueDiligence:
                    return nameof(StatementController.Training);
                case SubmissionStep.Training:
                    return nameof(StatementController.MonitoringProgress);
                case SubmissionStep.MonitoringProgress:
                    return nameof(StatementController.Review);
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
    }

    public enum SubmissionStep : byte
    {
        Unknown = 0,
        NotStarted = 1,
        YourStatement = 2,
        Compliance = 3,
        YourOrganisation = 4,
        Policies = 5,
        SupplyChainRisks = 6,
        DueDiligence = 7,
        Training = 8,
        MonitoringProgress = 9,
        Review = 10,
        Complete = 11

    }

    public class StatementMapperProfile : Profile
    {
        public StatementMapperProfile()
        {
            CreateMap<StatementViewModel, StatementModel>()
                .ForMember(dest => dest.OrganisationId, opt => opt.Ignore())

                .ForMember(dest => dest.Training, opt => opt.Ignore())
                .ForMember(dest => dest.StatementPolicies, opt => opt.Ignore())
                .ForMember(dest => dest.Diligences, opt => opt.Ignore())
                .ForMember(dest => dest.StatementSectors, opt => opt.Ignore())
                .ForMember(dest => dest.RelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.HighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.LocationRisks, opt => opt.Ignore())
                // Fill these in appropriately
                .ForMember(dest => dest.MinStatementYears, opt => opt.Ignore())
                .ForMember(dest => dest.MaxStatementYears, opt => opt.Ignore())
                .ForMember(dest => dest.MinTurnover, opt => opt.Ignore())
                .ForMember(dest => dest.MaxTurnover, opt => opt.Ignore())
                // TODO - James/Charlotte update VM to handle these
                .ForMember(dest => dest.ForcedLabourDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SlaveryInstanceDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SlaveryInstanceRemediation, opt => opt.Ignore());

            CreateMap<StatementModel, StatementViewModel>()
                // These need obfuscating
                .ForMember(dest => dest.OrganisationIdentifier, opt => opt.Ignore())

                .ForMember(dest => dest.StatementIdentifier, opt => opt.Ignore())
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
                .ForMember(dest => dest.LastFinancialYearBudget, opt => opt.Ignore())
                .ForMember(dest => dest.NumberOfYearsOfStatements, opt => opt.Ignore())
                // New members
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.IsStatementSectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsAreasCoveredSectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsOrganisationSectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsPoliciesSectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsSupplyChainRiskAndDiligencPart1SectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsSupplyChainRiskAndDiligencPart2SectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsTrainingSectionCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.IsMonitoringProgressSectionCompleted, opt => opt.Ignore())
                // Date components are set directly from the date field
                .ForMember(dest => dest.StatementStartDay, opt => opt.Ignore())
                .ForMember(dest => dest.StatementStartMonth, opt => opt.Ignore())
                .ForMember(dest => dest.StatementStartYear, opt => opt.Ignore())
                .ForMember(dest => dest.StatementEndDay, opt => opt.Ignore())
                .ForMember(dest => dest.StatementEndMonth, opt => opt.Ignore())
                .ForMember(dest => dest.StatementEndYear, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedDay, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedMonth, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedYear, opt => opt.Ignore());
        }
    }

    public class ObfuscatedFieldResolver : IValueResolver<StatementModel, StatementViewModel, string>
    {
        readonly IObfuscator Obfuscator;

        public ObfuscatedFieldResolver(IObfuscator obfuscator)
        {
            Obfuscator = obfuscator;
        }

        public string Resolve(StatementModel source, StatementViewModel destination, string destMember, ResolutionContext context)
        {
            return Obfuscator.Obfuscate(source.OrganisationId);
        }
    }

    public class DeobfuscatedFieldResolver : IValueResolver<StatementViewModel, StatementModel, long>
    {
        readonly IObfuscator Obfuscator;

        public DeobfuscatedFieldResolver(IObfuscator obfuscator)
        {
            Obfuscator = obfuscator;
        }

        public long Resolve(StatementViewModel source, StatementModel destination, long destMember, ResolutionContext context)
        {
            return Obfuscator.DeObfuscate(source.OrganisationIdentifier);
        }
    }
}
