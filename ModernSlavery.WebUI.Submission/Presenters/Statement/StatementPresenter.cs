using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Models.Statement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    public interface IStatementPresenter
    {
        JsonSerializerSettings JsonSettings { get; }

        /// <summary>
        /// Copies all relevant data from a StatementModel into the specified ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">The type of the desination ViewModel</typeparam>
        /// <param name="statementModel">The instance of the source StatementModel</param>
        /// <param name="Arguments">Any additional arguments required to populate the ViewModel</param>
        /// <returns>A new instance of the populated ViewModel</returns>
        TViewModel GetViewModelFromStatementModel<TViewModel>(StatementModel statementModel, params object[] arguments) where TViewModel : BaseStatementViewModel;

        /// <summary>
        /// Copies all relevant data from a ViewModel into the specified StatementModel 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the source ViewModel</typeparam>
        /// <param name="viewModel">The instance of the source ViewModel</param>
        /// <param name="statementModel">The instance of the destination StatementModel</param>
        /// <param name="Arguments">Any additional arguments required to update the StatementModel</param>
        /// <returns>A new instance of the populated StatementModel</returns>
        StatementModel SetViewModelToStatementModel<TViewModel>(TViewModel viewModel, StatementModel statementModel, params object[] arguments);

        /// <summary>
        /// Returns list of modifications between the specified statementModel and the draft backup)
        /// </summary>
        /// <param name="newStatementModel">The new statement</param>
        /// <returns>A list of modifications, null if no differences, or all changes from empty if no backup</returns>
        Task<IList<AutoMap.Diff>> CompareToDraftBackupStatement(StatementModel newStatementModel);

        /// <summary>
        /// Returns list of modifications between the specified statementModel and the latest submitted statement
        /// </summary>
        /// <param name="newStatementModel">The new statement</param>
        /// <returns>A list of modifications, null if no differences, or all changes from empty if no submitted statement</returns>
        Task<IList<AutoMap.Diff>> CompareToSubmittedStatement(StatementModel newStatementModel);

        /// <summary>
        /// Returns a ViewModel populated from a StatementModel for the specified oreganisation, reporting Deadline, and user
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to return</typeparam>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <param name="Arguments">Any additional arguments required to populate the ViewModel</param>
        /// <returns>Outcome.Success with the populated ViewModel or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, TViewModel>> GetViewModelAsync<TViewModel>(string organisationIdentifier, int reportingDeadlineYear, long userId, params object[] arguments) where TViewModel : BaseStatementViewModel;

        /// <summary>
        /// Updates a StatementModel from a ViewModel and saves a draft copy of the StatementModel for the specified oreganisation, reporting Deadline, and user
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to copy from</typeparam>
        /// <param name="viewModel">The populated ViewModel containing the changed data</param>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <param name="Arguments">Any additional arguments required to update the StatementModel</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, TViewModel>> SaveViewModelAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int reportingDeadlineYear, long userId, params object[] arguments) where TViewModel : BaseStatementViewModel;

        /// <summary>
        /// Returns a StatementModel populated for the specified oreganisation, reporting Deadline, and user.
        /// Each Call relocks the Draft StatementModel to the user 
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>Outcome.Success with the populated StatementModel or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Closes a previously opened Draft statement model and releases the lock from the current user
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who currently editting the Statement data</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Cancels any changes to the previous draft StatementModel
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

        /// <summary>
        /// Retires any previuous submitted Statement entity and creates a new Submitted Statement Entity from the Saved Draft StatementModel
        /// Also Deletes any previous Draft StatementModels and backups for this organisation, reporting year and user
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who wishes to edit the Statement data</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId, string ip, string summaryUrl);

        /// <summary>
        /// Save any changes to the statement model
        /// </summary>
        /// <param name="statementModel">The statement model to save</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task SaveStatementModelAsync(StatementModel statementModel);

        /// <summary>
        /// Search companies house and database for organisations
        /// </summary>
        /// <param name="groupSearchViewModel">The GroupSearchViewModel containing the search paramaters and search results</param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> SearchGroupOrganisationsAsync(GroupSearchViewModel groupSearchViewModel, User user);

        /// <summary>
        /// Include selected group organisation 
        /// </summary>
        /// <param name="groupSearchViewModel"></param>
        /// <param name="addIndex"></param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> IncludeGroupOrganisationAsync(GroupSearchViewModel groupSearchViewModel, int addIndex = 0);

        /// <summary>
        /// Add a manually entered group organisation 
        /// </summary>
        /// <param name="groupAddViewModel"></param>
        /// <returns>Outcome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> AddGroupOrganisationAsync(GroupAddViewModel groupAddViewModel);

        /// <summary>
        /// Get all the information on group organisation submissions
        /// </summary>
        /// <param name="groupOrganisationsViewModel"></param>
        /// <param name="reportingDeadline"></param>
        /// <returns></returns>
        Task GetOtherSubmissionsInformationAsync(GroupOrganisationsViewModel groupOrganisationsViewModel, int reportingDeadlineYear);
    }

    public class StatementPresenter : IStatementPresenter
    {
        private readonly ILogger<StatementPresenter> _logger;
        private readonly IHttpSession _session;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IMapper _mapper;
        public JsonSerializerSettings JsonSettings { get; }
        private readonly IServiceProvider _serviceProvider;
        private readonly IPagedRepository<OrganisationRecord> _organisationRepository;
        public StatementPresenter(
            ILogger<StatementPresenter> logger,
            IHttpSession session,
            IMapper mapper,
            DependencyContractResolver dependencyContractResolver,
            ISharedBusinessLogic sharedBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic,
            IServiceProvider serviceProvider,
            [KeyFilter("PrivateAndPublic")] IPagedRepository<OrganisationRecord> organisationRepository)
        {
            _logger = logger;
            _session = session;
            _mapper = mapper;
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = dependencyContractResolver
            };
            _sharedBusinessLogic = sharedBusinessLogic;
            _statementBusinessLogic = statementBusinessLogic;
            _serviceProvider = serviceProvider;
            _organisationRepository = organisationRepository;
        }
        private int CompaniesHouseFailures
        {
            get => _session["CompaniesHouseFailures"].ToInt32();
            set
            {
                _session.Remove("CompaniesHouseFailures");
                if (value > 0) _session["CompaniesHouseFailures"] = value;
            }
        }
        private int LastOrganisationSearchRemoteTotal => _session["LastOrganisationSearchRemoteTotal"].ToInt32();

        public TViewModel GetViewModelFromStatementModel<TViewModel>(StatementModel statementModel, params object[] arguments) where TViewModel : BaseStatementViewModel
        {
            //Instantiate the ViewModel with arguments
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider,arguments);
            return _mapper.Map(statementModel, viewModel);
        }

        public StatementModel SetViewModelToStatementModel<TViewModel>(TViewModel viewModel, StatementModel statementModel, params object[] arguments)
        {
            return _mapper.Map(viewModel, statementModel);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var openOutcome = await _statementBusinessLogic.OpenDraftStatementModelAsync(organisationId, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            if (statementModel.EditorUserId != userId) throw new ArgumentException(nameof(openOutcome.Result.EditorUserId));

            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            return await _statementBusinessLogic.CloseDraftStatementModelAsync(organisationId, reportingDeadlineYear, userId);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            return await _statementBusinessLogic.CancelDraftStatementModelAsync(organisationId, reportingDeadlineYear, userId);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId, string ip, string summaryUrl)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            return await _statementBusinessLogic.SubmitDraftStatementModelAsync(organisationId, reportingDeadlineYear, userId, ip, summaryUrl);
        }

        public async Task<Outcome<StatementErrors, TViewModel>> GetViewModelAsync<TViewModel>(string organisationIdentifier, int reportingDeadlineYear, long userId, params object[] arguments) where TViewModel : BaseStatementViewModel
        {
            var openOutcome = await OpenDraftStatementModelAsync(organisationIdentifier, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, TViewModel>(openOutcome.Errors);

            var statementModel = openOutcome.Result;
            var viewModel = GetViewModelFromStatementModel<TViewModel>(statementModel,arguments);
            return new Outcome<StatementErrors, TViewModel>(viewModel);
        }

        public async Task<Outcome<StatementErrors, TViewModel>> SaveViewModelAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int reportingDeadlineYear, long userId, params object[] arguments) where TViewModel : BaseStatementViewModel
        {
            var openOutcome = await OpenDraftStatementModelAsync(organisationIdentifier, reportingDeadlineYear, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, TViewModel>(openOutcome.Errors);

            var statementModel = openOutcome.Result;
            statementModel = SetViewModelToStatementModel(viewModel, statementModel,arguments);

            //Remove any child organisations when single submission
            if (statementModel.GroupSubmission != true) statementModel.StatementOrganisations.Clear();

            //Save the new statement containing the updated viewModel
            await _statementBusinessLogic.SaveDraftStatementModelAsync(statementModel);

            return new Outcome<StatementErrors, TViewModel>(viewModel);
        }

        public async Task SaveStatementModelAsync(StatementModel statementModel)
        {
            //Save the new statement containing the updated viewModel
            await _statementBusinessLogic.SaveDraftStatementModelAsync(statementModel);
        }


        public async Task<IList<AutoMap.Diff>> CompareToDraftBackupStatement(StatementModel newStatementModel)
        {
            return await _statementBusinessLogic.CompareToDraftBackupStatement(newStatementModel);
        }
        public async Task<IList<AutoMap.Diff>> CompareToSubmittedStatement(StatementModel newStatementModel)
        {
            return await _statementBusinessLogic.CompareToSubmittedStatement(newStatementModel);
        }

        public async Task<Outcome<StatementErrors>> SearchGroupOrganisationsAsync(GroupSearchViewModel groupSearchViewModel, User user)
        {
            if (groupSearchViewModel == null) throw new ArgumentNullException(nameof(GroupSearchViewModel));
            if (string.IsNullOrWhiteSpace(groupSearchViewModel.SearchKeywords)) throw new ArgumentNullException(nameof(groupSearchViewModel.SearchKeywords));

            try
            {
                groupSearchViewModel.ResultsPage = await _organisationRepository.SearchAsync(groupSearchViewModel.SearchKeywords,groupSearchViewModel.ResultsPage.CurrentPage,_sharedBusinessLogic.SharedOptions.OrganisationPageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                CompaniesHouseFailures++;
                if (CompaniesHouseFailures < 3)
                {
                    groupSearchViewModel.ResultsPage?.Results?.Clear();
                    return new Outcome<StatementErrors>(StatementErrors.CoHoTransientError);
                }

                await _sharedBusinessLogic.SendEmailService.SendMsuMessageAsync("MSU - COMPANIES HOUSE ERROR", $"Cant search using Companies House API for query '{groupSearchViewModel.SearchKeywords}' page:'1' due to following error:\n\n{ex.GetDetailsText()}");
                return new Outcome<StatementErrors>(StatementErrors.CoHoPermanentError);
            }

            if (LastOrganisationSearchRemoteTotal == -1)
            {
                CompaniesHouseFailures++;
                if (CompaniesHouseFailures >= 3)
                    return new Outcome<StatementErrors>(StatementErrors.CoHoPermanentError);
            }
            else
            {
                CompaniesHouseFailures = 0;
            }
            return new Outcome<StatementErrors>();
        }

        public async Task<Outcome<StatementErrors>> IncludeGroupOrganisationAsync(GroupSearchViewModel groupSearchViewModel, int addIndex = 0)
        {
            if (groupSearchViewModel == null) throw new ArgumentNullException(nameof(GroupSearchViewModel));
            if (groupSearchViewModel.ResultsPage == null) throw new ArgumentNullException(nameof(groupSearchViewModel.ResultsPage));
            if (addIndex < 0 || addIndex > groupSearchViewModel.ResultsPage.Results.Count - 1) throw new ArgumentOutOfRangeException(nameof(addIndex), $"Index is outside bounds of search results");

            var newStatementOrganisationViewModel = new GroupOrganisationsViewModel.StatementOrganisationViewModel() { Included = true };

            var groupOrganisation = groupSearchViewModel.ResultsPage.Results[addIndex];
            newStatementOrganisationViewModel.Address = groupOrganisation.ToAddressModel();
            newStatementOrganisationViewModel.OrganisationId = groupOrganisation.OrganisationId > 0 ? (int?)groupOrganisation.OrganisationId : default;
            newStatementOrganisationViewModel.OrganisationName = groupOrganisation.OrganisationName;
            newStatementOrganisationViewModel.CompanyNumber = groupOrganisation.CompanyNumber;
            newStatementOrganisationViewModel.DateOfCessation = groupOrganisation.DateOfCessation;

            //Check the organisation is not already included
            if (groupSearchViewModel.StatementOrganisations.Any(o => o.OrganisationName.EqualsI(newStatementOrganisationViewModel.OrganisationName)))
                return new Outcome<StatementErrors>(StatementErrors.DuplicateName, newStatementOrganisationViewModel.OrganisationName);

            //Include the new organisation
            groupSearchViewModel.StatementOrganisations.Add(newStatementOrganisationViewModel);
            return new Outcome<StatementErrors>();
        }

        public async Task<Outcome<StatementErrors>> AddGroupOrganisationAsync(GroupAddViewModel groupAddViewModel)
        {
            if (groupAddViewModel == null) throw new ArgumentNullException(nameof(GroupAddViewModel));
            if (string.IsNullOrWhiteSpace(groupAddViewModel.NewOrganisationName)) throw new ArgumentNullException(nameof(groupAddViewModel.NewOrganisationName));

            //Check the organisation is not already included
            if (groupAddViewModel.StatementOrganisations.Any(o => o.OrganisationName.EqualsI(groupAddViewModel.NewOrganisationName)))
                return new Outcome<StatementErrors>(StatementErrors.DuplicateName, groupAddViewModel.NewOrganisationName);

            //Add the new organisation manually
            var newStatementOrganisationViewModel = new GroupOrganisationsViewModel.StatementOrganisationViewModel()
            {
                Included = true,
                OrganisationName = groupAddViewModel.NewOrganisationName
            };

            //Include the new organisation
            groupAddViewModel.StatementOrganisations.Add(newStatementOrganisationViewModel);
            return new Outcome<StatementErrors>();
        }

        public async Task GetOtherSubmissionsInformationAsync(GroupOrganisationsViewModel groupOrganisationsViewModel, int reportingDeadlineYear)
        {
            foreach (var statementOrganisation in groupOrganisationsViewModel.StatementOrganisations)
            {
                if (statementOrganisation.OtherSubmissionsInformation != null) continue;
                if (statementOrganisation.OrganisationId == null || statementOrganisation.OrganisationId.Value <= 0)
                    statementOrganisation.OtherSubmissionsInformation = new List<string>();
                else
                    statementOrganisation.OtherSubmissionsInformation = await _statementBusinessLogic.GetExistingStatementInformationAsync(statementOrganisation.OrganisationId.Value, reportingDeadlineYear);

            }
        }
    }
}
