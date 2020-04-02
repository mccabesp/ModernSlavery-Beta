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
                    await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    log.LogDebug($"Skipped {funcName} at start up.");
                    return;
                }

                // Flag the UpdateRegistrationAddresses web job as started
                StartedJobs.Add(funcName);

                await UpdateRegistrationAddressesAsync(filePath, log);

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
                var latestRegistrationAddresses = await GetLatestRegistrationAddressesAsync();

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
                                    model.OrganisationId, year);
                            var scopeByYear =
                                await _ScopeBusinessLogic.GetLatestScopeBySnapshotYearAsync(model.OrganisationId, year);

                            // update file model with year data
                            model.HasSubmitted = returnByYear == null ? "False" : "True";
                            model.ScopeStatus = scopeByYear?.ScopeStatus;
                        }

                        return latestRegistrationAddresses;
                    });
            }
            finally
            {
                RunningJobs.Remove(funcName);
            }
        }

        public async Task<List<RegistrationAddressesFileModel>> GetLatestRegistrationAddressesAsync()
        {
            // Load the DnBOrgs file from storage"
            var dnbOrgsPath = Path.Combine(_SharedBusinessLogic.SharedOptions.DataPath, Filenames.DnBOrganisations());
            var AllDnBOrgs = await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(dnbOrgsPath)
                ? await _SharedBusinessLogic.FileRepository.ReadCSVAsync<DnBOrgsModel>(dnbOrgsPath)
                : new List<DnBOrgsModel>();
            AllDnBOrgs = AllDnBOrgs.OrderBy(o => o.OrganisationName).ToList();

            // Extract the DUNSNumber, JobTitle and FullName from the DnBOrgs
            var dnbUserOrgs = AllDnBOrgs
                .Select(dnbOrg => new
                    {dnbOrg.DUNSNumber, DnBJobTitle = dnbOrg.ContactJobtitle, DnBFullName = dnbOrg.GetContactName()})
                .ToList();

            // Get all the latest verified organisation registrations
            var verifiedOrgs = await _SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                .Where(uo => uo.LatestRegistration != null)
                .Include(uo => uo.LatestRegistration)
                .Include(uo => uo.LatestAddress)
                .Include(uo => uo.LatestReturn)
                .Include(uo => uo.LatestScope)
                .ToListAsync();

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

                        // Get the linked dnb record using the DUNSNumber
                        var dnbOrg = dnbUserOrgs.FirstOrDefault(dnbo => dnbo.DUNSNumber == vo.DUNSNumber);

                        // If (DnBJobTile and DnBFullName) is null or empty then DnbFullName = "Chief Executive"
                        var dnbJobTitle = dnbOrg?.DnBJobTitle;
                        var dnbFullName = dnbOrg?.DnBFullName;
                        if (string.IsNullOrWhiteSpace(dnbJobTitle) && string.IsNullOrWhiteSpace(dnbFullName))
                            dnbFullName = "Chief Executive";

                        // Retrieve the SectorType reporting snapshot date (d MMMM yyyy)
                        var expires = _snapshotDateHelper.GetSnapshotDate(vo.SectorType).AddYears(1).AddDays(-1)
                            .ToString("d MMMM yyyy");

                        // Generate csv row
                        return new RegistrationAddressesFileModel
                        {
                            OrganisationId = vo.OrganisationId,
                            DUNSNumber = vo.DUNSNumber,
                            EmployerReference = vo.EmployerReference,
                            Sector = vo.SectorType,
                            DnBJobTitle = dnbJobTitle,
                            DnBFullName = dnbFullName,
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