﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISubmissionBusinessLogic
    {
        // Submission
        IAuditLogger SubmissionLog { get; }
        
        /// <summary>
        ///     Gets the latest submitted return for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        /// <returns></returns>
        Task<Return> GetLatestStatementBySnapshotYearAsync(long organisationId, int snapshotYear);

        IEnumerable<Return> GetAllStatementsByOrganisationIdAndSnapshotYear(long organisationId,
            int snapshotYear);

        /// <summary>
        ///     Gets a list of submissions with scopes for Submissions download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        IEnumerable<StatementsFileModel> GetStatementsFileModelByYear(int year);

        /// <summary>
        ///     Gets a list of late submissions that were in scope
        /// </summary>
        /// <returns></returns>
        IEnumerable<LateSubmissionsFileModel> GetLateSubmissions();

        ReturnViewModel ConvertStatementToReturnViewModel(Statement statementToConvert);
        CustomResult<Return> GetSubmissionByOrganisationAndYear(Organisation organisation, int year);
    }
}