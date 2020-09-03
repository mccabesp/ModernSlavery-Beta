using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task UpdateFileAsync(ILogger log, string filePath, string action)
        {
            var fileName = Path.GetFileName(filePath);

            switch (Filenames.GetRootFilename(fileName))
            {
                case Filenames.Organisations:
                    await UpdateOrganisationsAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.Users:
                    await UpdateUsersAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.Registrations:
                    await UpdateRegistrationsAsync(log, filePath).ConfigureAwait(false);
                    break;
                case Filenames.RegistrationAddresses:
                    await UpdateRegistrationAddressesAsync(filePath, log).ConfigureAwait(false);
                    break;
                case Filenames.UnverifiedRegistrations:
                    await UpdateUnverifiedRegistrationsAsync(log, filePath).ConfigureAwait(false);
                    break;
                case Filenames.SendInfo:
                    await UpdateUsersToSendInfoAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.AllowFeedback:
                    await UpdateUsersToContactForFeedbackAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.OrganisationScopes:
                    await UpdateScopesAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.OrganisationSubmissions:
                    await UpdateSubmissionsAsync(filePath).ConfigureAwait(false);
                    break;
                case Filenames.OrganisationLateSubmissions:
                    await UpdateOrganisationLateSubmissionsAsync(filePath, log).ConfigureAwait(false);
                    break;
                case Filenames.OrphanOrganisations:
                    await UpdateOrphanOrganisationsAsync(filePath, log).ConfigureAwait(false);
                    break;
            }
        }


        public async Task WriteRecordsPerYearAsync<T>(string filePath, Func<int, Task<List<T>>> fillRecordsAsync)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");
            var datePart = fileName.AfterLast("_", includeWhenNoSeparator: false);

            var endYear = _snapshotDateHelper.GetReportingStartDate(SectorTypes.Private).Year;
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

                filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
                if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

                await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
        }

        public async Task WriteRecordsForYearAsync<T>(string filePath, int year, Func<Task<List<T>>> fillRecordsAsync)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");

            var records = await fillRecordsAsync().ConfigureAwait(false);

            filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
            if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

            await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
        }
    }
}