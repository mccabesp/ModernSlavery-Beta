﻿using System.Collections.Generic;
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

        public virtual async Task<Return> GetSubmissionByReturnIdAsync(long returnId)
        {
            return await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Return>(o => o.ReturnId == returnId);
        }

        /// <summary>
        ///     Gets the latest submitted return for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        /// <returns></returns>
        public virtual async Task<Return> GetLatestSubmissionBySnapshotYearAsync(long organisationId, int snapshotYear)
        {
            var orgSubmission = await _sharedBusinessLogic.DataRepository.FirstOrDefaultAsync<Return>(
                s => s.AccountingDate.Year == snapshotYear
                     && s.OrganisationId == organisationId
                     && s.Status == StatementStatuses.Submitted);

            return orgSubmission;
        }

        public IEnumerable<Return> GetAllSubmissionsByOrganisationIdAndSnapshotYear(long organisationId,
            int snapshotYear)
        {
            return _sharedBusinessLogic.DataRepository.GetAll<Return>().Where(s =>
                s.OrganisationId == organisationId && s.AccountingDate.Year == snapshotYear);
        }

        /// <summary>
        ///     Gets a list of submissions with scopes for Submissions download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual IEnumerable<SubmissionsFileModel> GetSubmissionsFileModelByYear(int year)
        {
            var scopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .Where(os => os.SubmissionDeadline.Year == year && os.Status == ScopeRowStatuses.Active)
                .Select(os => new {os.OrganisationId, os.ScopeStatus, os.ScopeStatusDate, os.SubmissionDeadline}).ToList();

            var returns = _sharedBusinessLogic.DataRepository.GetAll<Return>()
                .Where(r => r.AccountingDate.Year == year && r.Status == StatementStatuses.Submitted).ToList();

#if DEBUG
            if (Debugger.IsAttached) returns = returns.Take(100).ToList();
#endif

            // perform left join
            var records = returns.AsQueryable().GroupJoin(
                    // join
                    scopes.AsQueryable(),
                    // on
                    // inner
                    r => new {r.OrganisationId, r.AccountingDate.Year},
                    // outer
                    os => new {os.OrganisationId, os.SubmissionDeadline.Year},
                    // into
                    (r, os) => new {r, os = os.FirstOrDefault()})
                .ToList()
                .Select(
                    j => new SubmissionsFileModel
                    {
                        ReturnId = j.r.ReturnId,
                        OrganisationId = j.r.OrganisationId,
                        OrganisationName = j.r.Organisation.OrganisationName,
                        DUNSNumber = j.r.Organisation.DUNSNumber,
                        EmployerReference = j.r.Organisation.EmployerReference,
                        CompanyNumber = j.r.Organisation.CompanyNumber,
                        SectorType = j.r.Organisation.SectorType,
                        ScopeStatus = j.os?.ScopeStatus,
                        ScopeStatusDate = j.os?.ScopeStatusDate,
                        AccountingDate = j.r.AccountingDate,
                        ModifiedDate = j.r.Modified,
                        DiffMeanBonusPercent = j.r.DiffMeanBonusPercent,
                        DiffMeanHourlyPayPercent = j.r.DiffMeanHourlyPayPercent,
                        DiffMedianBonusPercent = j.r.DiffMedianBonusPercent,
                        DiffMedianHourlyPercent = j.r.DiffMedianHourlyPercent,
                        FemaleLowerPayBand = j.r.FemaleLowerPayBand,
                        FemaleMedianBonusPayPercent = j.r.FemaleMedianBonusPayPercent,
                        FemaleMiddlePayBand = j.r.FemaleMiddlePayBand,
                        FemaleUpperPayBand = j.r.FemaleUpperPayBand,
                        FemaleUpperQuartilePayBand = j.r.FemaleUpperQuartilePayBand,
                        MaleLowerPayBand = j.r.MaleLowerPayBand,
                        MaleMedianBonusPayPercent = j.r.MaleMedianBonusPayPercent,
                        MaleMiddlePayBand = j.r.MaleMiddlePayBand,
                        MaleUpperPayBand = j.r.MaleUpperPayBand,
                        MaleUpperQuartilePayBand = j.r.MaleUpperQuartilePayBand,
                        CompanyLink = j.r.CompanyLinkToGPGInfo,
                        ResponsiblePerson = j.r.ResponsiblePerson,
                        OrganisationSize = j.r.OrganisationSize.GetAttribute<DisplayAttribute>().Name,
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
            var curPrivateSnapshotDate = _sharedBusinessLogic.GetReportingStartDate(SectorTypes.Private);
            var curPublicSnapshotDate = _sharedBusinessLogic.GetReportingStartDate(SectorTypes.Public);
            var prevPrivateSnapshotDate = curPrivateSnapshotDate.AddYears(-1);
            var prevPublicSnapshotDate = curPublicSnapshotDate.AddYears(-1);

            // create return table query
            var lateSubmissions = _sharedBusinessLogic.DataRepository.GetAll<Return>()
                // filter only reports for the previous sector reporting start date and modified after their previous sector reporting end date
                .Where(
                    r => r.Organisation.SectorType == SectorTypes.Private
                         && r.AccountingDate == prevPrivateSnapshotDate
                         && r.Modified >= curPrivateSnapshotDate
                         || r.Organisation.SectorType == SectorTypes.Public
                         && r.AccountingDate == prevPublicSnapshotDate
                         && r.Modified >= curPublicSnapshotDate)
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
                        r.ReturnId,
                        r.AccountingDate,
                        r.LateReason,
                        r.Created,
                        r.Modified,
                        r.Modifications,
                        r.FirstName,
                        r.LastName,
                        r.JobTitle,
                        r.EHRCResponse
                    }).ToList();

            // create scope table query
            var activeScopes = _sharedBusinessLogic.DataRepository.GetAll<OrganisationScope>()
                .Where(os =>
                    os.SubmissionDeadline.Year == prevPrivateSnapshotDate.Year && os.Status == ScopeRowStatuses.Active)
                .Select(os => new {os.OrganisationId, os.ScopeStatus, os.ScopeStatusDate, os.SubmissionDeadline}).ToList();

            // perform a left join on lateSubmissions and activeScopes
            var records = lateSubmissions.AsQueryable().GroupJoin(
                    // join with
                    activeScopes.AsQueryable(),
                    // on
                    // inner
                    r => new {r.OrganisationId, r.AccountingDate.Year},
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
                        ReportId = j.r.ReturnId,
                        ReportSnapshotDate = j.r.AccountingDate,
                        ReportLateReason = j.r.LateReason,
                        ReportSubmittedDate = j.r.Created,
                        ReportModifiedDate = j.r.Modified,
                        ReportModifiedFields = j.r.Modifications,
                        ReportPersonResonsible =
                            j.r.SectorType == SectorTypes.Public
                                ? "Not required"
                                : $"{j.r.FirstName} {j.r.LastName} ({j.r.JobTitle})",
                        ReportEHRCResponse = j.r.EHRCResponse
                    });
        }

        public ReturnViewModel ConvertSubmissionReportToReturnViewModel(Return reportToConvert)
        {
            var model = new ReturnViewModel
            {
                SectorType = reportToConvert.Organisation.SectorType,
                ReturnId = reportToConvert.ReturnId,
                OrganisationId = reportToConvert.OrganisationId,
                DiffMeanBonusPercent = reportToConvert.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = reportToConvert.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = reportToConvert.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = reportToConvert.DiffMedianHourlyPercent,
                FemaleLowerPayBand = reportToConvert.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = reportToConvert.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = reportToConvert.FemaleMiddlePayBand,
                FemaleUpperPayBand = reportToConvert.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = reportToConvert.FemaleUpperQuartilePayBand,
                MaleLowerPayBand = reportToConvert.MaleLowerPayBand,
                MaleMedianBonusPayPercent = reportToConvert.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = reportToConvert.MaleMiddlePayBand,
                MaleUpperPayBand = reportToConvert.MaleUpperPayBand,
                MaleUpperQuartilePayBand = reportToConvert.MaleUpperQuartilePayBand,
                JobTitle = reportToConvert.JobTitle,
                FirstName = reportToConvert.FirstName,
                LastName = reportToConvert.LastName,
                CompanyLinkToGPGInfo = reportToConvert.CompanyLinkToGPGInfo,
                AccountingDate = reportToConvert.AccountingDate,
                Address = reportToConvert.Organisation.GetAddressString(reportToConvert.StatusDate),
                LatestAddress = reportToConvert.Organisation.LatestAddress?.GetAddressString(),
                EHRCResponse = reportToConvert.EHRCResponse.ToString(),
                IsVoluntarySubmission = reportToConvert.IsVoluntarySubmission(),
                IsLateSubmission = reportToConvert.IsLateSubmission
            };

            if (model.Address.EqualsI(model.LatestAddress)) model.LatestAddress = null;

            model.OrganisationName = reportToConvert.Organisation.GetName(reportToConvert.StatusDate)?.Name
                                     ?? reportToConvert.Organisation.OrganisationName;
            model.LatestOrganisationName = reportToConvert.Organisation.OrganisationName;

            model.Sector = reportToConvert.Organisation.GetSicSectorsString(reportToConvert.StatusDate);
            model.LatestSector = reportToConvert.Organisation.GetLatestSicSectorsString();

            model.OrganisationSize = reportToConvert.OrganisationSize;
            model.Modified = reportToConvert.Modified;

            model.IsInScopeForThisReportYear =
                reportToConvert.Organisation.GetIsInscope(reportToConvert.AccountingDate);

            return model;
        }

        public CustomResult<Return> GetSubmissionByOrganisationAndYear(Organisation organisation, int year)
        {
            var reports = GetAllSubmissionsByOrganisationIdAndSnapshotYear(organisation.OrganisationId, year);

            if (!reports.Any())
                return new CustomResult<Return>(
                    InternalMessages.HttpNotFoundCausedByOrganisationReturnNotInDatabase(_sharedBusinessLogic.Obfuscator.Obfuscate(organisation.OrganisationId),
                        year));

            var result = reports.OrderByDescending(r => r.Status == StatementStatuses.Submitted)
                .ThenByDescending(r => r.StatusDate)
                .FirstOrDefault();
            if (!result.IsSubmitted())
                return new CustomResult<Return>(
                    InternalMessages.HttpGoneCausedByReportNotHavingBeenSubmitted(result.AccountingDate.Year,
                        result.Status.ToString()));

            return new CustomResult<Return>(result);
        }

        #endregion

        #region Entities

        #endregion
    }
}