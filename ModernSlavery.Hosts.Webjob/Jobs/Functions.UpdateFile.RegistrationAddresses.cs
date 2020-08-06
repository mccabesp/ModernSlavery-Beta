using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        public async Task UpdateRegistrationAddressesAsync(
            [TimerTrigger(typeof(EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            var funcName = nameof(UpdateRegistrationAddressesAsync);

            try
            {
                var filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath,
                    Filenames.RegistrationAddresses);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(funcName) &&
                    await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false))
                {
                    log.LogDebug($"Skipped {funcName} at start up.");
                    return;
                }

                // Flag the UpdateRegistrationAddresses web job as started
                StartedJobs.Add(funcName);

                await UpdateRegistrationAddressesAsync(filePath, log).ConfigureAwait(false);

                log.LogDebug($"Executed {funcName}:successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {funcName}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        public async Task UpdateRegistrationAddressesAsync(string filePath, ILogger log)
        {
            var funcName = nameof(UpdateRegistrationAddressesAsync);

            // Ensure the UpdateRegistrationAddresses web job is not already running
            if (RunningJobs.Contains(funcName))
            {
                log.LogDebug($"Skipped {funcName} because already running.");
                return;
            }

            try
            {
                // Flag the UpdateRegistrationAddresses web job as running
                RunningJobs.Add(funcName);

                // Cache the latest registration addresses
                var latestRegistrationAddresses = await GetLatestRegistrationAddressesAsync().ConfigureAwait(false);

                // Write yearly records to csv files
                await WriteRecordsPerYearAsync(
                    filePath,
                    async year =>
                    {
                        foreach (var model in latestRegistrationAddresses)
                        {
                            // get organisation scope and submission per year
                            var returnByYear =
                                await _SubmissionBusinessLogic.GetLatestSubmissionBySnapshotYearAsync(
                                    model.OrganisationId, year).ConfigureAwait(false);
                            var scopeByYear =
                                await _ScopeBusinessLogic.GetLatestScopeByReportingDeadlineAsync(model.OrganisationId, returnByYear.AccountingDate).ConfigureAwait(false);

                            // update file model with year data
                            model.HasSubmitted = returnByYear == null ? "False" : "True";
                            model.ScopeStatus = scopeByYear?.ScopeStatus;
                        }

                        return latestRegistrationAddresses;
                    }).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(funcName);
            }
        }

        public async Task<List<RegistrationAddressesFileModel>> GetLatestRegistrationAddressesAsync()
        {
            // Get all the latest verified organisation registrations
            var verifiedOrgs = await _SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(uo => uo.LatestRegistration != null)
                .Include(uo => uo.LatestRegistration)
                .Include(uo => uo.LatestAddress)
                .Include(uo => uo.LatestStatement)
                .Include(uo => uo.LatestScope)
                .ToListAsync().ConfigureAwait(false);

            return verifiedOrgs.Select(
                    vo =>
                    {
                        // Read the latest address for the organisation
                        var latestAddress = vo.LatestAddress;
                        if (latestAddress == null)
                            throw new Exception(
                                $"Organisation {vo.OrganisationId} has a latest registration with no Organisation Address associated");

                        // Get the latest user for the organisation
                        var latestRegistrationUser = vo.LatestRegistration?.User;
                        if (latestAddress == null)
                            throw new Exception(
                                $"Organisation {vo.OrganisationId} has a latest registration with no User associated");

                        // Ensure the address lines don't start with null or whitespaces
                        var addressLines = new List<string>();
                        foreach (var line in new[]
                            {latestAddress.Address1, latestAddress.Address2, latestAddress.Address3})
                            if (string.IsNullOrWhiteSpace(line) == false)
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
                        var expires = _snapshotDateHelper.GetReportingStartDate(vo.SectorType).AddYears(1).AddDays(-1)
                            .ToString("d MMMM yyyy");

                        // Generate csv row
                        return new RegistrationAddressesFileModel
                        {
                            OrganisationId = vo.OrganisationId,
                            DUNSNumber = vo.DUNSNumber,
                            EmployerReference = vo.EmployerReference,
                            Sector = vo.SectorType,
                            LatestUserJobTitle = latestRegistrationUser.JobTitle,
                            LatestUserFullName = latestRegistrationUser.Fullname,
                            LatestUserStatus = latestRegistrationUser.Status.ToString(),
                            Company = vo.OrganisationName,
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