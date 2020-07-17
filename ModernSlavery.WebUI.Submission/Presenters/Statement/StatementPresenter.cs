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
using ModernSlavery.WebUI.Submission.Models;
using ModernSlavery.WebUI.Submission.Models.Statement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        Task<YourStatementPageViewModel> GetYourStatementAsync(User user, string organisationIdentifier, int year);

        /// <summary>
        /// Save the current submission draft which is only visible to the current user.
        /// </summary>
        Task<StatementActionResult> TrySaveYourStatement(User user, StatementViewModel model);
        Task SaveYourStatementAsync(User user, YourStatementPageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetCompliance(User user, string organisationIdentifier, int year);
        Task<CompliancePageViewModel> GetComplianceAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveCompliance(User user, StatementViewModel model);
        Task SaveComplianceAsync(User user, CompliancePageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetYourOrganisation(User user, string organisationIdentifier, int year);
        Task<OrganisationPageViewModel> GetYourOrganisationAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveYourOrgansation(User user, StatementViewModel model);
        Task SaveYourOrgansationAsync(User user, OrganisationPageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetPolicies(User user, string organisationIdentifier, int year);
        Task<PoliciesPageViewModel> GetPoliciesAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySavePolicies(User user, StatementViewModel model);
        Task SavePoliciesAsync(User user, PoliciesPageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetSupplyChainRiskAndDueDiligence(User user, string organisationIdentifier, int year);
        Task<RisksPageViewModel> GetRisksAsync(User user, string organisationIdentifier, int year);
        Task<DueDiligencePageViewModel> GetDueDiligenceAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveSupplyChainRiskAndDueDiligence(User user, StatementViewModel model);
        Task SaveRisksAsync(User user, RisksPageViewModel viewModel);
        Task SaveDueDiligenceAsync(User user, DueDiligencePageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetTraining(User user, string organisationIdentifier, int year);
        Task<TrainingPageViewModel> GetTrainingAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveTraining(User user, StatementViewModel model);
        Task SaveTrainingAsync(User user, TrainingPageViewModel viewModel);

        Task<CustomResult<StatementViewModel>> TryGetMonitoringInProgress(User user, string organisationIdentifier, int year);
        Task<ProgressPageViewModel> GetProgressAsync(User user, string organisationIdentifier, int year);

        Task<StatementActionResult> TrySaveMonitorInProgress(User user, StatementViewModel model);
        Task SaveProgressAsync(User user, ProgressPageViewModel viewModel);

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

        public async Task<YourStatementPageViewModel> GetYourStatementAsync(User user, string organisationIdentifier, int year)
        {
            // to keep it simple, throw exceptions
            // these should really be handled with custom result type class rather than exceptions

            var model = await GetStatementModelAsync(user, organisationIdentifier, year);

            var vm = new YourStatementPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                StatementUrl = model.StatementUrl,
                StatementStartDate = model.StatementStartDate,
                StatementEndDate = model.StatementEndDate,
                ApproverFirstName = model.ApproverFirstName,
                ApproverLastName = model.ApproverLastName,
                ApproverJobTitle = model.ApproverLastName,
                ApprovedDate = model.ApprovedDate,
            };

            return vm;
        }

        public async Task SaveYourStatementAsync(User user, YourStatementPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.StatementUrl = viewModel.StatementUrl;
            model.StatementStartDate = viewModel.StatementStartDate;
            model.StatementEndDate = viewModel.StatementEndDate;
            model.ApproverFirstName = viewModel.ApproverFirstName;
            model.ApproverLastName = viewModel.ApproverLastName;
            model.ApproverJobTitle = viewModel.ApproverLastName;
            model.ApprovedDate = viewModel.ApprovedDate;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
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

        public async Task<CompliancePageViewModel> GetComplianceAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);

            var vm = new CompliancePageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                IncludesStructure = model.IncludesStructure,
                StructureDetails = model.StructureDetails,
                IncludesPolicies = model.IncludesPolicies,
                PolicyDetails = model.PolicyDetails,
                IncludesRisks = model.IncludesRisks,
                RisksDetails = model.RisksDetails,
                IncludesDueDiligence = model.IncludesDueDiligence,
                DueDiligenceDetails = model.DueDiligenceDetails,
                IncludesTraining = model.IncludesTraining,
                TrainingDetails = model.TrainingDetails,
                IncludesGoals = model.IncludesGoals,
                GoalsDetails = model.GoalsDetails,
            };

            return vm;
        }

        public async Task SaveComplianceAsync(User user, CompliancePageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.IncludesStructure = viewModel.IncludesStructure;
            model.StructureDetails = viewModel.StructureDetails;
            model.IncludesPolicies = viewModel.IncludesPolicies;
            model.PolicyDetails = viewModel.PolicyDetails;
            model.IncludesRisks = viewModel.IncludesRisks;
            model.RisksDetails = viewModel.RisksDetails;
            model.IncludesDueDiligence = viewModel.IncludesDueDiligence;
            model.DueDiligenceDetails = viewModel.DueDiligenceDetails;
            model.IncludesTraining = viewModel.IncludesTraining;
            model.TrainingDetails = viewModel.TrainingDetails;
            model.IncludesGoals = viewModel.IncludesGoals;
            model.GoalsDetails = viewModel.GoalsDetails;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
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

        public async Task<OrganisationPageViewModel> GetYourOrganisationAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);
            var sectors = SharedBusinessLogic.DataRepository
                .GetAll<StatementSectorType>()
                .Select(t => new OrganisationPageViewModel.SectorViewModel
                {
                    Id = t.StatementSectorTypeId,
                    Description = t.Description,
                    IsSelected = model.StatementSectors.Contains(t.StatementSectorTypeId)
                });

            var vm = new OrganisationPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                Sectors = sectors.ToList(),
                Turnover = ParseTurnover(model.MinTurnover, model.MaxTurnover),
            };

            return vm;
        }

        private LastFinancialYearBudget? ParseTurnover(decimal? min, decimal? max)
        {
            if (!min.HasValue && !max.HasValue)
                return null;

            if (min >= 500_000_000)
                return LastFinancialYearBudget.From500MillionUpwards;

            else if (max <= 500_000_000 && min >= 100_000_000)
                return LastFinancialYearBudget.From100MillionTo500Million;

            else if (max <= 100_000_000 && min >= 60_000_000)
                return LastFinancialYearBudget.From60MillionTo100Million;

            else if (max <= 60_000_000 && min >= 36_000_000)
                return LastFinancialYearBudget.From60MillionTo100Million;

            else
                return LastFinancialYearBudget.Under36Million;
        }

        public async Task SaveYourOrgansationAsync(User user, OrganisationPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);
            var turnover = GetTurnoverRange(viewModel.Turnover);

            model.StatementSectors = viewModel.Sectors.Where(s => s.IsSelected).Select(s => s.Id).ToList();
            model.MinTurnover = turnover.Item1;
            model.MaxTurnover = turnover.Item2;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
        }

        private Tuple<int?, int?> GetTurnoverRange(LastFinancialYearBudget? turnover)
        {
            if (!turnover.HasValue)
                return new Tuple<int?, int?>(null, null);

            switch (turnover.Value)
            {
                case LastFinancialYearBudget.Under36Million:
                    return new Tuple<int?, int?>(0, 36_000_000);
                case LastFinancialYearBudget.From36MillionTo60Million:
                    return new Tuple<int?, int?>(36_000_000, 60_000_000);
                case LastFinancialYearBudget.From60MillionTo100Million:
                    return new Tuple<int?, int?>(60_000_000, 100_000_000);
                case LastFinancialYearBudget.From100MillionTo500Million:
                    return new Tuple<int?, int?>(100_000_000, 500_000_000);
                case LastFinancialYearBudget.From500MillionUpwards:
                    return new Tuple<int?, int?>(500_000_000, null);
                default:
                    return new Tuple<int?, int?>(null, null);
            }
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

        public async Task<PoliciesPageViewModel> GetPoliciesAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);
            var policies = SharedBusinessLogic.DataRepository
                .GetAll<StatementPolicyType>()
                .Select(t => new PoliciesPageViewModel.PolicyViewModel
                {
                    Id = t.StatementPolicyTypeId,
                    Description = t.Description,
                    IsSelected = model.StatementPolicies.Contains(t.StatementPolicyTypeId)
                });

            var vm = new PoliciesPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                Policies = policies.ToList(),
                OtherPolicies = model.OtherPolicies
            };

            return vm;
        }

        public async Task SavePoliciesAsync(User user, PoliciesPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.StatementPolicies = viewModel.Policies.Where(p => p.IsSelected).Select(s => s.Id).ToList();
            model.OtherPolicies = model.OtherPolicies;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
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

        #region Step 5 - Supply chain risks and due diligence part 1

        public async Task<RisksPageViewModel> GetRisksAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);

            var releventRisks = SharedBusinessLogic.DataRepository
                .GetAll<StatementRiskType>()
                .Where(t => t.Category == RiskCategories.RiskArea)
                .Select(t => new RisksPageViewModel.RiskViewModel
                {
                    Id = t.StatementRiskTypeId,
                    ParentId = t.ParentRiskTypeId,
                    Description = t.Description,
                    IsSelected = model.RelevantRisks.Contains(t.StatementRiskTypeId)
                });

            var highRisks = SharedBusinessLogic.DataRepository
                .GetAll<StatementRiskType>()
                .Where(t => t.Category == RiskCategories.RiskArea)
                .Select(t => new RisksPageViewModel.RiskViewModel
                {
                    Id = t.StatementRiskTypeId,
                    ParentId = t.ParentRiskTypeId,
                    Description = t.Description,
                    IsSelected = model.HighRisks.Contains(t.StatementRiskTypeId)
                });

            var locationRisks = SharedBusinessLogic.DataRepository
                .GetAll<StatementRiskType>()
                .Where(t => t.Category == RiskCategories.Location)
                .Select(t => new RisksPageViewModel.RiskViewModel
                {
                    Id = t.StatementRiskTypeId,
                    ParentId = t.ParentRiskTypeId,
                    Description = t.Description,
                    IsSelected = model.LocationRisks.Contains(t.StatementRiskTypeId)
                });


            var vm = new RisksPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                RelevantRisks = releventRisks.ToList(),
                OtherRelevantRisks = model.OtherRelevantRisks,
                HighRisks = highRisks.ToList(),
                OtherHighRisks = model.OtherHighRisks,
                LocationRisks = locationRisks.ToList(),
            };

            return vm;
        }

        public async Task SaveRisksAsync(User user, RisksPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.RelevantRisks = viewModel.RelevantRisks.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            model.OtherRelevantRisks = viewModel.OtherRelevantRisks;
            model.HighRisks = viewModel.HighRisks.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            model.OtherHighRisks = viewModel.OtherHighRisks;
            model.LocationRisks = viewModel.LocationRisks.Where(r => r.IsSelected).Select(r => r.Id).ToList();

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence part 2

        public async Task<DueDiligencePageViewModel> GetDueDiligenceAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);
            var diligences = SharedBusinessLogic.DataRepository
                .GetAll<StatementDiligenceType>()
                .Select(t => new DueDiligencePageViewModel.DueDiligenceViewModel
                {
                    Id = t.StatementDiligenceTypeId,
                    ParentId = t.ParentDiligenceTypeId,
                    Description = t.Description,
                    IsSelected = model.Diligences.Contains(t.StatementDiligenceTypeId)
                });

            var vm = new DueDiligencePageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                DueDiligences = diligences.ToList(),
                HasForceLabour = string.IsNullOrEmpty(model.ForcedLabourDetails),
                ForcedLabourDetails = model.ForcedLabourDetails,
                HasSlaveryInstance = string.IsNullOrEmpty(model.SlaveryInstanceDetails),
                SlaveryInstanceDetails = model.SlaveryInstanceDetails,
                SlaveryInstanceRemediation = ParseRemediation(model.SlaveryInstanceRemediation).ToList(),
            };

            return vm;
        }

        private IEnumerable<StatementRemediation> ParseRemediation(string slaveryInstanceRemediation)
        {
            var items = slaveryInstanceRemediation.Split(Environment.NewLine);

            return items.Select(r => (StatementRemediation)Enum.Parse(typeof(StatementRemediation), r));
        }

        public async Task SaveDueDiligenceAsync(User user, DueDiligencePageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.Diligences = viewModel.DueDiligences.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            if (viewModel.HasForceLabour)
                model.ForcedLabourDetails = viewModel.ForcedLabourDetails;
            else
                model.ForcedLabourDetails = null;
            if (viewModel.HasSlaveryInstance)
            {
                model.SlaveryInstanceDetails = viewModel.SlaveryInstanceDetails;
                model.SlaveryInstanceRemediation = string.Join(Environment.NewLine, viewModel.SlaveryInstanceRemediation.Select(r => Enum.GetName(typeof(StatementRemediation), r)));
            }
            else
            {
                model.SlaveryInstanceDetails = null;
                model.SlaveryInstanceRemediation = null;
            }

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
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

        public async Task<TrainingPageViewModel> GetTrainingAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);
            var training = SharedBusinessLogic.DataRepository
                .GetAll<StatementTrainingType>()
                .Select(t => new TrainingPageViewModel.TrainingViewModel
                {
                    Id = t.StatementTrainingTypeId,
                    Description = t.Description,
                    IsSelected = model.Training.Contains(t.StatementTrainingTypeId)
                });

            var vm = new TrainingPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                Training = training.ToList(),
                OtherTraining = model.OtherTraining,
            };

            return vm;
        }

        public async Task SaveTrainingAsync(User user, TrainingPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);

            model.Training = viewModel.Training.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            model.OtherTraining = viewModel.OtherTraining;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
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
        public async Task<ProgressPageViewModel> GetProgressAsync(User user, string organisationIdentifier, int year)
        {
            var model = await GetStatementModelAsync(user, organisationIdentifier, year);
            var training = SharedBusinessLogic.DataRepository
                .GetAll<StatementTrainingType>()
                .Select(t => new TrainingPageViewModel.TrainingViewModel
                {
                    Id = t.StatementTrainingTypeId,
                    Description = t.Description,
                    IsSelected = model.Training.Contains(t.StatementTrainingTypeId)
                });

            var vm = new ProgressPageViewModel
            {
                Year = year,
                OrganisationIdentifier = organisationIdentifier,
                IncludesMeasuringProgress = model.IncludesMeasuringProgress,
                ProgressMeasures = model.ProgressMeasures,
                KeyAchievements = model.KeyAchievements,
                NumberOfYearsOfStatements = ParseYears(model.MinStatementYears, model.MaxStatementYears),
            };

            return vm;
        }

        private NumberOfYearsOfStatements? ParseYears(decimal? min, decimal? max)
        {
            if (!min.HasValue && !max.HasValue)
                return null;

            if (min >= 5)
                return NumberOfYearsOfStatements.moreThan5Years;
            else if (max <= 5 && min >= 1)
                return NumberOfYearsOfStatements.from1To5Years;
            else
                return NumberOfYearsOfStatements.thisIsTheFirstTime;
        }

        public async Task SaveProgressAsync(User user, ProgressPageViewModel viewModel)
        {
            var model = await GetStatementModelAsync(user, viewModel.OrganisationIdentifier, viewModel.Year);
            var yearsRange = GetYearsRange(viewModel.NumberOfYearsOfStatements);

            model.IncludesMeasuringProgress = viewModel.IncludesMeasuringProgress;
            model.ProgressMeasures = viewModel.ProgressMeasures;
            model.KeyAchievements = viewModel.KeyAchievements;
            model.MinStatementYears = yearsRange.Item1;
            model.MaxStatementYears = yearsRange.Item2;

            var result = await StatementBusinessLogic.SaveDraftStatement(user, model);

            if (result != StatementActionResult.Success)
                throw new ValidationException("Saving failed");
        }

        private Tuple<decimal?, decimal?> GetYearsRange(NumberOfYearsOfStatements? years)
        {
            if (!years.HasValue)
                return new Tuple<decimal?, decimal?>(null, null);

            switch (years.Value)
            {
                case NumberOfYearsOfStatements.thisIsTheFirstTime:
                    return new Tuple<decimal?, decimal?>(0, 1);
                case NumberOfYearsOfStatements.from1To5Years:
                    return new Tuple<decimal?, decimal?>(1, 5);
                case NumberOfYearsOfStatements.moreThan5Years:
                    return new Tuple<decimal?, decimal?>(5, null);
                default:
                    return new Tuple<decimal?, decimal?>(null, null);
            }
        }

        #endregion

        #region Step 8 - Review TODO
        #endregion

        private async Task<StatementModel> GetStatementModelAsync(User user, string organisationIdentifier, int year)
        {
            var id = SharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(id);
            var actionresult = await StatementBusinessLogic.CanAccessStatement(user, organisation, year);
            if (actionresult != StatementActionResult.Success)
                throw new ValidationException("You can not access this statement");

            var model = await StatementBusinessLogic.GetStatementByOrganisationAndYear(organisation, year);
            return model;
        }

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
