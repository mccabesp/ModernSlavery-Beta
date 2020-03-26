using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task UpdateOrphanOrganisationsAsync([TimerTrigger(typeof(MidnightSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            var funcName = nameof(UpdateOrphanOrganisationsAsync);

            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.OrphanOrganisations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(funcName) &&
                    await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    log.LogDebug($"Skipped {funcName} at start up.");
                    return;
                }

                // Flag the UpdateUnregisteredOrganisations web job as started
                StartedJobs.Add(funcName);

                await UpdateOrphanOrganisationsAsync(filePath, log);

                log.LogDebug($"Executed {funcName}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {funcName}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        public async Task UpdateOrphanOrganisationsAsync(string filePath, ILogger log)
        {
            var funcName = nameof(UpdateOrphanOrganisationsAsync);

            // Ensure the UpdateUnregisteredOrganisations web job is not already running
            if (RunningJobs.Contains(funcName))
            {
                log.LogDebug($"Skipped {funcName} because already running.");
                return;
            }

            try
            {
                // Flag the UpdateUnregisteredOrganisations web job as running
                RunningJobs.Add(funcName);

                // Cache the latest unregistered organisations
                var unregisteredOrganisations = await GetOrphanOrganisationsAsync();

                var year = _snapshotDateHelper.GetSnapshotDate(SectorTypes.Private).Year;

                // Write yearly records to csv files
                await WriteRecordsForYearAsync(
                    filePath,
                    year,
                    async () =>
                    {
                        foreach (var model in unregisteredOrganisations)
                        {
                            // get organisation scope and submission per year
                            var returnByYear =
                                await _SubmissionBusinessLogic.GetLatestSubmissionBySnapshotYearAsync(
                                    model.OrganisationId, year);
                            var scopeByYear =
                                await _ScopeBusinessLogic.GetLatestScopeBySnapshotYearAsync(model.OrganisationId, year);

                            // update file model with year data
                            model.HasSubmitted = returnByYear == null ? "False" : "True";
                            model.ScopeStatus = scopeByYear?.ScopeStatus;
                        }

                        return unregisteredOrganisations;
                    });
            }
            finally
            {
                RunningJobs.Remove(funcName);
            }
        }

        private async Task<List<UnregisteredOrganisationsFileModel>> GetOrphanOrganisationsAsync()
        {
            // Get all the latest organisations with no registrations
            var pinExpiresDate = _SharedBusinessLogic.SharedOptions.PinExpiresDate;
            var unregisteredOrgs = await _SharedBusinessLogic.DataRepository.GetAll<Organisation>().Where(o =>
                    o.Status == OrganisationStatuses.Active
                    && (o.LatestScope.ScopeStatus == ScopeStatuses.InScope
                        || o.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                    && (o.UserOrganisations == null
                        || !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null
                                                          || uo.Method == RegistrationMethods.Manual
                                                          || uo.Method == RegistrationMethods.PinInPost
                                                          && uo.PINSentDate.HasValue
                                                          && uo.PINSentDate.Value > pinExpiresDate)))
                .Include(o => o.LatestAddress)
                .ToListAsync();

            return unregisteredOrgs.Select(
                    org =>
                    {
                        // Read the latest address for the organisation
                        var latestAddress = org.LatestAddress;
                        if (latestAddress == null)
                            throw new ArgumentException(
                                $"Organisation {org.OrganisationId} has no latest address associated");

                        // Ensure the address lines don't start with null or whitespaces
                        var addressLines = new List<string>();
                        foreach (var line in new[]
                            {latestAddress.Address1, latestAddress.Address2, latestAddress.Address3})
                            if (!string.IsNullOrWhiteSpace(line))
                                addressLines.Add(line);

                        for (var i = addressLines.Count; i < 3; i++) addressLines.Add(string.Empty);

                        // Format post code with the po boxes
                        var postCode = latestAddress.PostCode;
                        if (!string.IsNullOrWhiteSpace(postCode) && !string.IsNullOrWhiteSpace(latestAddress.PoBox))
                            postCode = latestAddress.PoBox + ", " + postCode;
                        else if (!string.IsNullOrWhiteSpace(latestAddress.PoBox) && string.IsNullOrWhiteSpace(postCode))
                            postCode = latestAddress.PoBox;

                        // Convert two letter country codes to full country names
                        var countryCode = Country.FindTwoLetterCode(latestAddress.Country);

                        // Retrieve the SectorType reporting snapshot date (d MMMM yyyy)
                        var expires = _snapshotDateHelper.GetSnapshotDate(org.SectorType).AddYears(1).AddDays(-1)
                            .ToString("d MMMM yyyy");

                        // Generate csv row
                        return new UnregisteredOrganisationsFileModel
                        {
                            OrganisationId = org.OrganisationId,
                            DUNSNumber = org.DUNSNumber,
                            EmployerReference = org.EmployerReference,
                            Sector = org.SectorType,
                            Company = org.OrganisationName,
                            Address1 = addressLines[0],
                            Address2 = addressLines[1],
                            Address3 = addressLines[2],
                            City = latestAddress.TownCity,
                            Postcode = postCode,
                            County = latestAddress.County,
                            Country = string.IsNullOrWhiteSpace(countryCode) || countryCode.EqualsI("GB")
                                ? latestAddress.Country
                                : null,
                            CreatedByUserId = latestAddress.CreatedByUserId,
                            Expires = expires
                        };
                    })
                .OrderBy(model => model.Company)
                .ToList();
        }
    }
}