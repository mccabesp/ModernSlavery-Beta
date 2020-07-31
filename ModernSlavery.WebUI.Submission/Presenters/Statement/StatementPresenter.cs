﻿using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Submission.Models.Statement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// <returns>A new instance of the populated ViewModel</returns>
        TViewModel GetViewModelFromStatementModel<TViewModel>(StatementModel statementModel) where TViewModel : BaseViewModel;

        /// <summary>
        /// Copies all relevant data from a ViewModel into the specified StatementModel 
        /// </summary>
        /// <typeparam name="TViewModel">The type of the source ViewModel</typeparam>
        /// <param name="viewModel">The instance of the source ViewModel</param>
        /// <param name="statementModel">The instance of the destination StatementModel</param>
        /// <returns>A new instance of the populated StatementModel</returns>
        StatementModel SetViewModelToStatementModel<TViewModel>(TViewModel viewModel, StatementModel statementModel);

        /// <summary>
        /// Returns a Json object showing modifications between the specified statementModel and the original (draft backup or submitted statements)
        /// </summary>
        /// <param name="statementModel">The current statement</param>
        /// <returns>A Json list of modifications or null if no differences</returns>
        Task<IList<AutoMap.Diff>> GetDraftModifications(StatementModel statementModel);

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
        /// Closes a previously opened Draft statement model and releases the lock from the current user
        /// </summary>
        /// <param name="organisationIdentifier">The unique obfuscated identifier of the organisation who owns the statement data</param>
        /// <param name="reportingDeadlineYear">The year of the reporting deadlien to which the statement data relates</param>
        /// <param name="userId">The unique Id of the user who currently editting the Statement data</param>
        /// <returns>OutCome.Success or Outcome.Fail with a list of StatementErrors</returns>
        Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId);

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
        
        /// <summary>
        /// Save any changes to the statement model
        /// </summary>
        /// <param name="statementModel">The statement model to save</param>
        /// <returns></returns>
        Task SaveStatementModelAsync(StatementModel statementModel);
    }

    public class StatementPresenter : IStatementPresenter
    {
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IMapper _mapper;
        public JsonSerializerSettings JsonSettings { get; }
        private readonly IServiceProvider _serviceProvider;
        public StatementPresenter(
            IMapper mapper,
            DependencyContractResolver dependencyContractResolver,
            ISharedBusinessLogic sharedBusinessLogic,
            IStatementBusinessLogic statementBusinessLogic,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = dependencyContractResolver
            };
            _sharedBusinessLogic = sharedBusinessLogic;
            _statementBusinessLogic = statementBusinessLogic;
            _serviceProvider = serviceProvider;
        }



        private DateTime GetReportingDeadline(long organisationId, int year)
        {
            var organisation = _sharedBusinessLogic.DataRepository.Get<Organisation>(organisationId);
            if (organisation == null) throw new ArgumentOutOfRangeException(nameof(organisationId));
            return _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType, year);
        }

        public TViewModel GetViewModelFromStatementModel<TViewModel>(StatementModel statementModel) where TViewModel : BaseViewModel
        {
            //Instantiate the ViewModel
            var viewModel = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider);

            //Copy the StatementModel data to the viewModel
            return _mapper.Map(statementModel, viewModel);
        }

        public StatementModel SetViewModelToStatementModel<TViewModel>(TViewModel viewModel, StatementModel statementModel)
        {
            return _mapper.Map(viewModel, statementModel);
        }

        public async Task<Outcome<StatementErrors, StatementModel>> OpenDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = GetReportingDeadline(organisationId, reportingDeadlineYear);
            var openOutcome = await _statementBusinessLogic.OpenDraftStatementModelAsync(organisationId, reportingDeadline, userId);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            if (statementModel.EditorUserId != userId) throw new ArgumentException(nameof(openOutcome.Result.EditorUserId));

            return new Outcome<StatementErrors, StatementModel>(statementModel);
        }

        public async Task<Outcome<StatementErrors>> CloseDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = GetReportingDeadline(organisationId, reportingDeadlineYear);
            return await _statementBusinessLogic.CloseDraftStatementModelAsync(organisationId, reportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors>> CancelDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = GetReportingDeadline(organisationId, reportingDeadlineYear);
            return await _statementBusinessLogic.CancelDraftStatementModelAsync(organisationId, reportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors>> SubmitDraftStatementModelAsync(string organisationIdentifier, int reportingDeadlineYear, long userId)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = GetReportingDeadline(organisationId, reportingDeadlineYear);
            return await _statementBusinessLogic.SubmitDraftStatementModelAsync(organisationId, reportingDeadline, userId);
        }

        public async Task<Outcome<StatementErrors, TViewModel>> GetViewModelAsync<TViewModel>(string organisationIdentifier, int reportingDeadlineYear, long userId) where TViewModel : BaseViewModel
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
            statementModel = SetViewModelToStatementModel(viewModel, statementModel);

            //Save the new statement containing the updated viewModel
            await _statementBusinessLogic.SaveDraftStatementModelAsync(statementModel);

            return new Outcome<StatementErrors, TViewModel>(viewModel);
        }

        public async Task SaveStatementModelAsync(StatementModel statementModel) 
        {
            //Save the new statement containing the updated viewModel
            await _statementBusinessLogic.SaveDraftStatementModelAsync(statementModel);
        }


        public async Task<IList<AutoMap.Diff>> GetDraftModifications(StatementModel statementModel)
        {
            return await _statementBusinessLogic.GetDraftModifications(statementModel);
        }


    }
}