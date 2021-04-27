using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.BusinessDomain.Submission
{
    public class SubmissionBusinessLogic : ISubmissionBusinessLogic
    {
        private readonly ISharedBusinessLogic _sharedBusinessLogic;

        public SubmissionBusinessLogic(
            ISharedBusinessLogic sharedBusinessLogic)
        {
            _sharedBusinessLogic = sharedBusinessLogic;
        }


        #region Repo

        public virtual async Task<Statement> GetStatementByIdAsync(long statementId)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Statement>(o => o.StatementId == statementId).ConfigureAwait(false);
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
                     && s.Status == StatementStatuses.Submitted).ConfigureAwait(false);

            return orgSubmission;
        }

        /// <summary>
        ///     Gets a list of submissions with scopes for Submissions download file
        /// </summary>
        /// <param name="deadlineYear"></param>
        /// <returns></returns>
        public virtual IEnumerable<StatementsFileModel> GetStatementsFileModelByYear(int deadlineYear)
        {
            var statements = _sharedBusinessLogic.DataRepository.GetAll<Statement>().Where(r => r.SubmissionDeadline.Year == deadlineYear && r.Status == StatementStatuses.Submitted).ToList();
            statements.SelectMany(s => s.Organisation.OrganisationScopes);
            statements.SelectMany(s => s.Statuses);

#if DEBUG || DEBUGLOCAL
            if (Debugger.IsAttached) statements = statements.Take(100).ToList();
#endif
            foreach (var statement in statements)
            {
                var organisationIdentifier = _sharedBusinessLogic.Obfuscator.Obfuscate(statement.OrganisationId);
                var statementSummaryUrl = $"/statement-summary/{organisationIdentifier}/{deadlineYear}";
                yield return StatementsFileModel.Create(statement, statementSummaryUrl);
            }
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
                        r.ApprovingPerson
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
                                : j.r.ApprovingPerson
                    });
        }
        #endregion
    }
}