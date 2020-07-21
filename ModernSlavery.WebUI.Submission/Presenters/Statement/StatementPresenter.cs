using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.WebUI.Submission.Models.Statement;
using System;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    public interface IStatementPresenter
    {
        /// <summary>
        /// Returns a ViewModel populated from a StatementModel for the specified oreganisation, reporting Deadline, and user
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to return</typeparam>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>OutCome.Success with the populated ViewModel or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, TViewModel>> GetViewModelAsync<TViewModel>(string organisationIdentifier, int reportingDeadlineYear, long userId) where TViewModel : BaseViewModel;

        /// <summary>
        /// Updates a StatementModel from a ViewModel and saves a draft copy of the StatementModel for the specified oreganisation, reporting Deadline, and user
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to copy from</typeparam>
        /// <param name="viewModel">The populated ViewModel containing the changed data</param>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>OutCome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, TViewModel>> SaveViewModelAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int reportingDeadlineYear, long userId) where TViewModel : BaseViewModel;

        /// <summary>
        /// Returns a StatementModel populated for the specified oreganisation, reporting Deadline, and user.
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>OutCome.Success with the populated StatementModel or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Cancels any changes to the previous draft StatementModel
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>OutCome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Retires any previuous submitted Statement entity and creates a new Submitted Statement Entity from the Saved Draft StatementModel
        /// Also Deletes any previous Draft StatementModels and backups for this organisation, reporting year and user
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>OutCome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);
    }

    public class StatementPresenter : IStatementPresenter
    {
        readonly IStatementBusinessLogic StatementBusinessLogic;
        readonly ISharedBusinessLogic SharedBusinessLogic;

        readonly IMapper Mapper;

        public StatementPresenter(
            IMapper mapper,
            ISharedBusinessLogic sharedBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic,
            IUrlHelper urlHelper)
        {
            Mapper = mapper;
            SharedBusinessLogic = sharedBusinessLogic;
            StatementBusinessLogic = statementBusinessLogic;
        }


        /// <summary>
        /// Copies all relevant data from a StatementModel into the specified ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">The type of the desination ViewModel</typeparam>
        /// <param name="statementModel">The instance of the source StatementModel</param>
        /// <returns>A new instance of the populated ViewModel</returns>
        private TViewModel GetViewModelFromStatementModel<TViewModel>(StatementModel statementModel) where TViewModel :BaseViewModel
        {
            var viewModel = Activator.CreateInstance<TViewModel>();
            return Mapper.Map(statementModel, viewModel);
        }

        /// <summary>
        /// Copies all relevant data from a ViewModel into the specified StatementModel 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the source ViewModel</typeparam>
        /// <param name="viewModel">The instance of the source ViewModel</param>
        /// <param name="statementModel">The instance of the destination StatementModel</param>
        /// <returns>A new instance of the populated StatementModel</returns>
        private StatementModel SetViewModelToStatementModel<TViewModel>(TViewModel viewModel, StatementModel statementModel)
        {
            return Mapper.Map(viewModel, statementModel);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var openOutcome = await StatementBusinessLogic.OpenDraftStatementModel(organisationId, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            if (statementModel.UserId != userId) throw new ArgumentException(nameof(openOutcome.Result.UserId));

            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            return await StatementBusinessLogic.CancelDraftStatementModel(organisationId, reportingDeadlineYear, userId);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            return await StatementBusinessLogic.SubmitDraftStatementModel(organisationId, reportingDeadlineYear, userId);
        }

        public async Task<Outcome<StatementErrors,TViewModel>> GetViewModelAsync<TViewModel>(string organisationIdentifier, int reportingDeadlineYear, long userId) where TViewModel:BaseViewModel
        {
            var openOutcome = await OpenDraftStatementModelAsync(organisationIdentifier, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, TViewModel>(openOutcome.Errors);

            var statementModel = openOutcome.Result;
            var viewModel = GetViewModelFromStatementModel<TViewModel>(statementModel);
            return new Outcome<StatementErrors, TViewModel>(viewModel);
        }

        public async Task<Outcome<StatementErrors, TViewModel>> SaveViewModelAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int reportingDeadlineYear, long userId) where TViewModel : BaseViewModel
        {
            var openOutcome = await OpenDraftStatementModelAsync(organisationIdentifier, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, TViewModel>(openOutcome.Errors);

            var statementModel = openOutcome.Result;
            statementModel = SetViewModelToStatementModel(viewModel,statementModel);

            //Save the new statement containing the updated viewModel
            await StatementBusinessLogic.SaveDraftStatementModel(statementModel);

            return new Outcome<StatementErrors, TViewModel>(viewModel);
        }
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
                .ForMember(dest => dest.RelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.StatementSectors, opt => opt.Ignore())
                .ForMember(dest => dest.StructureDetails, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyDetails, opt => opt.Ignore())
                .ForMember(dest => dest.RisksDetails, opt => opt.Ignore())
                .ForMember(dest => dest.IncludesDueDiligence, opt => opt.Ignore())
                .ForMember(dest => dest.DueDiligenceDetails, opt => opt.Ignore())
                .ForMember(dest => dest.TrainingDetails, opt => opt.Ignore())
                .ForMember(dest => dest.GoalsDetails, opt => opt.Ignore())
                .ForMember(dest => dest.OtherSector, opt => opt.Ignore())
                .ForMember(dest => dest.OtherPolicies, opt => opt.Ignore())
                .ForMember(dest => dest.OtherRelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.OtherTraining, opt => opt.Ignore())
                .ForMember(dest => dest.MinTurnover, opt => opt.Ignore())
                .ForMember(dest => dest.MaxTurnover, opt => opt.Ignore())
                .ForMember(dest => dest.LocationRisks, opt => opt.Ignore())
                .ForMember(dest => dest.MinStatementYears, opt => opt.Ignore())
                .ForMember(dest => dest.MaxStatementYears, opt => opt.Ignore())
                .ForMember(dest => dest.HighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.OtherHighRisks, opt => opt.Ignore());


            CreateMap<StatementModel, StatementViewModel>()
                // These need obfuscating
                .ForMember(dest => dest.OrganisationIdentifier, opt => opt.Ignore())

                .ForMember(dest => dest.StatementIdentifier, opt => opt.Ignore())
                // These need to change on VM to come from DB
                .ForMember(dest => dest.Policies, opt => opt.Ignore())
                .ForMember(dest => dest.Training, opt => opt.Ignore())
                .ForMember(dest => dest.RelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.Sectors, opt => opt.Ignore())
                .ForMember(dest => dest.Diligences, opt => opt.Ignore())
                // These need mapping for correct type on vm
                .ForMember(dest => dest.RelevantRiskTypes, opt => opt.Ignore())
                .ForMember(dest => dest.DiligenceTypes, opt => opt.Ignore())
                .ForMember(dest => dest.OtherSector, opt => opt.Ignore())
                .ForMember(dest => dest.OtherPolicies, opt => opt.Ignore())
                .ForMember(dest => dest.OtherRelevantRisks, opt => opt.Ignore())
                .ForMember(dest => dest.HighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.HighRiskTypes, opt => opt.Ignore())
                .ForMember(dest => dest.OtherHighRisks, opt => opt.Ignore())
                .ForMember(dest => dest.OtherTraining, opt => opt.Ignore())
                .ForMember(dest => dest.IncludesMeasuringProgress, opt => opt.Ignore())


                // Work out storage of these
                .ForMember(dest => dest.IncludedOrganistionCount, opt => opt.Ignore())
                .ForMember(dest => dest.ExcludedOrganisationCount, opt => opt.Ignore())
                .ForMember(dest => dest.ForcedLabour, opt => opt.Ignore())
                .ForMember(dest => dest.ForcedLabourDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SlaveryInstance, opt => opt.Ignore())
                .ForMember(dest => dest.SlaveryInstanceDetails, opt => opt.Ignore())
                .ForMember(dest => dest.StatementRemediations, opt => opt.Ignore())
                .ForMember(dest => dest.SlaveryInstanceRemediation, opt => opt.Ignore())
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
}
