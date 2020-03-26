using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
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
                    await UpdateOrganisationsAsync(filePath);
                    break;
                case Filenames.Users:
                    await UpdateUsersAsync(filePath);
                    break;
                case Filenames.Registrations:
                    await UpdateRegistrationsAsync(log, filePath);
                    break;
                case Filenames.RegistrationAddresses:
                    await UpdateRegistrationAddressesAsync(filePath, log);
                    break;
                case Filenames.UnverifiedRegistrations:
                    await UpdateUnverifiedRegistrationsAsync(log, filePath);
                    break;
                case Filenames.SendInfo:
                    await UpdateUsersToSendInfoAsync(filePath);
                    break;
                case Filenames.AllowFeedback:
                    await UpdateUsersToContactForFeedbackAsync(filePath);
                    break;
                case Filenames.OrganisationScopes:
                    await UpdateScopesAsync(filePath);
                    break;
                case Filenames.OrganisationSubmissions:
                    await UpdateSubmissionsAsync(filePath);
                    break;
                case Filenames.OrganisationLateSubmissions:
                    await UpdateOrganisationLateSubmissionsAsync(filePath, log);
                    break;
                case Filenames.OrphanOrganisations:
                    await UpdateOrphanOrganisationsAsync(filePath, log);
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

            var endYear = _snapshotDateHelper.GetSnapshotDate(SectorTypes.Private).Year;
            var startYear = _SharedBusinessLogic.SharedOptions.FirstReportingYear;
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
                var records = await fillRecordsAsync(year);

                filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
                if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

                await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
            }
        }

        public async Task WriteRecordsForYearAsync<T>(string filePath, int year, Func<Task<List<T>>> fillRecordsAsync)
        {
            var path = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var prefix = fileName.BeforeFirst("_");

            var records = await fillRecordsAsync();

            filePath = $"{prefix}_{year}-{(year + 1).ToTwoDigitYear()}{extension}";
            if (!string.IsNullOrWhiteSpace(path)) filePath = Path.Combine(path, filePath);

            await Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
        }
    }
}