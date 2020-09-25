﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Viewing.Models;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IViewingPresenter
    {
        IObfuscator Obfuscator { get; }
        Task<SearchViewModel> SearchAsync(SearchQueryModel searchQuery);
        Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        #region Dependencies
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly IViewingService _viewingService;
        public IObfuscator Obfuscator { get; }
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly SectorTypeIndex _sectorTypes;
        #endregion

        #region Constructor
        public ViewingPresenter(IViewingService viewingService,
            IStatementBusinessLogic statementBusinessLogic,
            ISharedBusinessLogic sharedBusinessLogic,
            IObfuscator obfuscator,
            IServiceProvider serviceProvider,
            IMapper mapper,
            ISearchBusinessLogic searchBusinessLogic,
            SectorTypeIndex sectorTypes)
        {
            _viewingService = viewingService;
            _statementBusinessLogic = statementBusinessLogic;
            _sharedBusinessLogic = sharedBusinessLogic;
            Obfuscator = obfuscator;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _sectorTypes = sectorTypes;
            _searchBusinessLogic = searchBusinessLogic;
        }
        #endregion

        #region Search methods
        public async Task<SearchViewModel> SearchAsync(SearchQueryModel searchQuery)
        {
            //Execute the search
            var searchResults = await _viewingService.SearchBusinessLogic.SearchOrganisationsAsync(
                searchQuery.Keywords,
                searchQuery.Turnovers,
                searchQuery.Sectors,
                searchQuery.Years,
                false,
                false,
                false,
                searchQuery.PageNumber,
                searchQuery.PageSize);

            // build the result view model
            return new SearchViewModel
            {
                TurnoverOptions = GetTurnoverOptions(searchQuery.Turnovers),
                SectorOptions = GetSectorOptions(searchQuery.Sectors),
                ReportingYearOptions = GetReportingYearOptions(searchQuery.Years),
                Organisations = searchResults,
                Keywords = searchQuery.Keywords,
                PageNumber = searchQuery.PageNumber,
                Sectors = searchQuery.Sectors,
                Turnovers = searchQuery.Turnovers,
                Years = searchQuery.Years
            };
        }

        #endregion

        #region Filter methods
        public List<OptionSelect> GetTurnoverOptions(IEnumerable<byte> filterTurnoverRanges)
        {
            var allRanges = Enums.GetValues<StatementTurnovers>();

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (var range in allRanges)
            {
                if (range == StatementTurnovers.NotProvided) continue;
                var id = (byte)range;
                var label = range.GetDisplayDescription();
                var isChecked = filterTurnoverRanges != null && filterTurnoverRanges.Contains(id);
                results.Add(
                    new OptionSelect
                    {
                        Id = $"Turnover{id}",
                        Label = label,
                        Value = id.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }

        public List<OptionSelect> GetSectorOptions(IEnumerable<short> filterSectorTypeIds)
        {
            // setup the filters
            var sources = new List<OptionSelect>();
            foreach (var sectorType in _sectorTypes)
            {
                sources.Add(
                    new OptionSelect
                    {
                        Id = sectorType.Id.ToString(),
                        Label = sectorType.Description.TrimEnd('\r', '\n'),
                        Value = sectorType.Id.ToString(),
                        Checked = filterSectorTypeIds != null && filterSectorTypeIds.Any(x => x == sectorType.Id)
                    });
            }

            return sources;
        }

        public List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            var reportingDeadlines = _sharedBusinessLogic.GetReportingDeadlines(SectorTypes.Public);
            var sources = new List<OptionSelect>();
            foreach (var reportingDeadline in reportingDeadlines)
            {
                var isChecked = filterSnapshotYears != null && filterSnapshotYears.Any(x => x == reportingDeadline.Year);
                sources.Add(
                    new OptionSelect
                    {
                        Id = reportingDeadline.Year.ToString(),
                        Label = $"{reportingDeadline.Year - 1} to {reportingDeadline.Year}",
                        Value = reportingDeadline.Year.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }
        #endregion

        #region Statement methods
        public async Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = _sharedBusinessLogic.GetReportingDeadline(organisationId, reportingDeadlineYear);
            var openOutcome = await _statementBusinessLogic.GetLatestSubmittedStatementModelAsync(organisationId, reportingDeadline);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementViewModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            //Copy the statement properties to the view model
            var viewModel = ActivatorUtilities.CreateInstance<StatementViewModel>(_serviceProvider);
            var statementViewModel = _mapper.Map(statementModel, viewModel);

            //Return the view model
            return new Outcome<StatementErrors, StatementViewModel>(statementViewModel);
        }

        #endregion
    }
}