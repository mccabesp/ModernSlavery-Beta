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
}
