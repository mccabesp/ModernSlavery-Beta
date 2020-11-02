using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Submission
{
    public class SubmissionBusinessLogic : ISubmissionBusinessLogic
    {
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        public IAuditLogger SubmissionLog { get; }

        public SubmissionBusinessLogic(
            ISharedBusinessLogic sharedBusinessLogic,
            [KeyFilter(Filenames.SubmissionLog)] 
            IAuditLogger submissionLog)
        {
            _sharedBusinessLogic = sharedBusinessLogic;
            SubmissionLog = submissionLog;
        }


        #region Repo

        public virtual async Task<Statement> GetStatementByIdAsync(long statementId)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(o => o.StatementId == statementId);
        }

        /// <summary>
        ///     Gets the latest submitted statement for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="deadlineYear"></param>
        /// <returns></returns>
        public virtual async Task<Statement> GetLatestStatementByDeadlineYearAsync(long organisationId, int deadlineYear)
        {
            var orgSubmission = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(
                s => s.SubmissionDeadline.Year == deadlineYear
                     && s.OrganisationId == organisationId
                     && s.Status == StatementStatuses.Submitted);

            return orgSubmission;
        }

        public IEnumerable<Statement> GetAllStatementsByOrganisationIdAndReportingDeadlineYear(long organisationId,
            int deadlineYear)
        {
            return _sharedBusinessLogic.DataRepository.GetAll<Statement>().Where(s =>
                s.OrganisationId == organisationId && s.SubmissionDeadline.Year == deadlineYear);
        }

        /// <summary>
        ///     Gets a list of submissions with scopes for Submissions download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual IEnumerable<StatementsFileModel> GetStatementsFileModelByYear(int year)
        {
            var scopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .Where(os => os.SubmissionDeadline.Year == year && os.Status == ScopeRowStatuses.Active)
                .Select(os => new {os.OrganisationId, os.ScopeStatus, os.ScopeStatusDate, os.SubmissionDeadline}).ToList();

            var statements = _sharedBusinessLogic.DataRepository.GetAll<Statement>()
                .Where(r => r.SubmissionDeadline.Year == year && r.Status == StatementStatuses.Submitted).ToList();

#if DEBUG
            if (Debugger.IsAttached) statements = statements.Take(100).ToList();
#endif

            // perform left join
            var records = statements.AsQueryable().GroupJoin(
                    // join
                    scopes.AsQueryable(),
                    // on
                    // inner
                    r => new {r.OrganisationId, r.SubmissionDeadline.Year},
                    // outer
                    os => new {os.OrganisationId, os.SubmissionDeadline.Year},
                    // into
                    (r, os) => new {r, os = os.FirstOrDefault()})
                .ToList()
                .Select(
                    j => new StatementsFileModel
                    {
                        StatementId = j.r.StatementId,
                        OrganisationId = j.r.OrganisationId,
                        OrganisationName = j.r.Organisation.OrganisationName,
                        DUNSNumber = j.r.Organisation.DUNSNumber,
                        OrganisationReference = j.r.Organisation.OrganisationReference,
                        CompanyNumber = j.r.Organisation.CompanyNumber,
                        SectorType = j.r.Organisation.SectorType,
                        ScopeStatus = j.os?.ScopeStatus,
                        ScopeStatusDate = j.os?.ScopeStatusDate,
                        SubmissionDeadline = j.r.SubmissionDeadline,
                        ModifiedDate = j.r.Modified,
                        StatementUrl = j.r.StatementUrl,
                        ApprovingPerson = j.r.ApprovingPerson,
                        Turnover = j.r.GetStatementTurnover().ToString(),
                        Modifications = j.r.Modifications,
                        EHRCResponse = j.r.EHRCResponse
                    });

            return records;
        }

        /// <summary>
        ///     Gets a list of late submissions that were in scope
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<LateSubmissionsFileModel> GetLateSubmissions()
        {
            // get the snapshot dates to filter submissions by
            var curPrivateReportingDeadlineDate = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(SectorTypes.Private);
            var curPublicReportingDeadlineDate = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadline(SectorTypes.Public);
            var prevPrivateReportingDeadlineDate = curPrivateReportingDeadlineDate.AddYears(-1);
            var prevPublicReportingDeadlineDate = curPublicReportingDeadlineDate.AddYears(-1);

            // create return table query
            var lateSubmissions = _sharedBusinessLogic.DataRepository.GetAll<Statement>()
                // filter only reports for the previous sector reporting start date and modified after their previous sector reporting end date
                .Where(
                    r => r.Organisation.SectorType == SectorTypes.Private
                         && r.SubmissionDeadline == prevPrivateReportingDeadlineDate
                         && r.Modified >= curPrivateReportingDeadlineDate
                         || r.Organisation.SectorType == SectorTypes.Public
                         && r.SubmissionDeadline == prevPublicReportingDeadlineDate
                         && r.Modified >= curPublicReportingDeadlineDate)
                // ensure we only return new, modified figures or modified SRO records
                .Where(
                    r => string.IsNullOrEmpty(r.Modifications)
                         || r.Modifications.ToLower().Contains("figures")
                         || r.Modifications.ToLower().Contains("personresponsible"))
                .Where(r => r.Status == StatementStatuses.Submitted)
                .Select(
                    r => new
                    {
                        r.OrganisationId,
                        r.Organisation.OrganisationName,
                        r.Organisation.SectorType,
                        r.StatementId,
                        r.SubmissionDeadline,
                        r.LateReason,
                        r.Created,
                        r.Modified,
                        r.Modifications,
                        r.ApprovingPerson,
                        r.EHRCResponse
                    }).ToList();

            // create scope table query
            var activeScopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .Where(os =>
                    os.SubmissionDeadline.Year == prevPrivateReportingDeadlineDate.Year && os.Status == ScopeRowStatuses.Active)
                .Select(os => new {os.OrganisationId, os.ScopeStatus, os.ScopeStatusDate, os.SubmissionDeadline}).ToList();

            // perform a left join on lateSubmissions and activeScopes
            var records = lateSubmissions.AsQueryable().GroupJoin(
                    // join with
                    activeScopes.AsQueryable(),
                    // on
                    // inner
                    r => new {r.OrganisationId, r.SubmissionDeadline.Year},
                    // outer
                    os => new {os.OrganisationId, os.SubmissionDeadline.Year},
                    // into
                    (r, os) => new {r, os = os.FirstOrDefault()})
                // ensure we only have in scope returns
                .Where(
                    j => j.os == null
                         || j.os.ScopeStatus != ScopeStatuses.OutOfScope &&
                         j.os.ScopeStatus != ScopeStatuses.PresumedOutOfScope);

            return records
                .ToList()
                .Select(
                    j => new LateSubmissionsFileModel
                    {
                        OrganisationId = j.r.OrganisationId,
                        OrganisationName = j.r.OrganisationName,
                        OrganisationSectorType = j.r.SectorType,
                        ReportId = j.r.StatementId,
                        ReportingDeadline = j.r.SubmissionDeadline,
                        ReportLateReason = j.r.LateReason,
                        ReportSubmittedDate = j.r.Created,
                        ReportModifiedDate = j.r.Modified,
                        ReportModifiedFields = j.r.Modifications,
                        ReportPersonResonsible =
                            j.r.SectorType == SectorTypes.Public
                                ? "Not required"
                                : j.r.ApprovingPerson,
                        ReportEHRCResponse = j.r.EHRCResponse
                    });
        }

        public CustomResult<Statement> GetSubmissionByOrganisationAndYear(Organisation organisation, int year)
        {
            var reports = GetAllStatementsByOrganisationIdAndReportingDeadlineYear(organisation.OrganisationId, year);

            if (!reports.Any())
                return new CustomResult<Statement>(
                    InternalMessages.HttpNotFoundCausedByOrganisationReturnNotInDatabase(_sharedBusinessLogic.Obfuscator.Obfuscate(organisation.OrganisationId),
                        year));

            var result = reports.OrderByDescending(r => r.Status == StatementStatuses.Submitted)
                .ThenByDescending(r => r.StatusDate)
                .FirstOrDefault();
            if (result.Status!=StatementStatuses.Submitted)
                return new CustomResult<Statement>(
                    InternalMessages.HttpGoneCausedByReportNotHavingBeenSubmitted(result.SubmissionDeadline.Year,
                        result.Status.ToString()));

            return new CustomResult<Statement>(result);
        }

        #endregion
    }
}