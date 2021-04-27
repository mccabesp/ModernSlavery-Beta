using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Extensions = ModernSlavery.Core.Classes.Extensions;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class UpdateFilesWebJobs : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IFileRepository _fileRepository;
        private readonly IDataRepository _dataRepository;
        private readonly IScopeBusinessLogic _scopeBusinessLogic;
        private readonly ISubmissionBusinessLogic _submissionBusinessLogic;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly IAuthorisationBusinessLogic _authorisationBusinessLogic;
        #endregion

        public UpdateFilesWebJobs(
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IFileRepository fileRepository,
            IDataRepository dataRepository,
            IReportingDeadlineHelper reportingDeadlineHelper,
            IScopeBusinessLogic scopeBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IOrganisationBusinessLogic organisationBusinessLogic,
            IAuthorisationBusinessLogic authorisationBusinessLogic)
        {
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _fileRepository = fileRepository;
            _dataRepository = dataRepository;
            _reportingDeadlineHelper = reportingDeadlineHelper;
            _scopeBusinessLogic = scopeBusinessLogic;
            _submissionBusinessLogic = submissionBusinessLogic;
            _organisationBusinessLogic = organisationBusinessLogic;
            _authorisationBusinessLogic = authorisationBusinessLogic;
        }

        public async Task WriteRecordsPerYearAsync<T>(string filePath, Func<int, Task<List<T>>> fillRecordsAsync)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");
            var datePart = fileName.AfterLast("_", includeWhenNoSeparator: false);

            var endYear = _reportingDeadlineHelper.GetReportingDeadline(SectorTypes.Private).Year;
            var startYear = _sharedOptions.FirstReportingDeadlineYear;
            if (!string.IsNullOrWhiteSpace(datePart))
            {
                var start = datePart.BeforeFirst("-").ToInt32().ToFourDigitYear();
                if (start > startYear && start <= endYear) startYear = start;

                endYear = startYear;
            }

            //Make sure start and end are in correct order
            if (startYear > endYear) (startYear, endYear) = (endYear, startYear);

            for (var year = endYear; year >= startYear; year--)
            {
                var records = await fillRecordsAsync(year).ConfigureAwait(false);

                filePath = $"{prefix}_{year}{extension}";
                if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

                if (records.Count>0)await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
        }

        public async Task WriteRecordsForYearAsync<T>(string filePath, int year, Func<Task<List<T>>> fillRecordsAsync)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");

            var records = await fillRecordsAsync().ConfigureAwait(false);

            filePath = $"{prefix}_{year}{extension}";
            if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

            if (records.Count>0)await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
        }
    }
}