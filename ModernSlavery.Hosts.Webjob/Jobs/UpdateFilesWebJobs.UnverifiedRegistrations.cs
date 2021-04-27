using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using Extensions = ModernSlavery.Core.Classes.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class UpdateFilesWebJobs
    {
        [Disable(typeof(DisableWebJobProvider))]
        public async Task UpdateUnverifiedRegistrationsAsync(
            [TimerTrigger(typeof(EveryWorkingHourSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                var filePath = Path.Combine(_sharedOptions.DownloadsPath,
                    Filenames.UnverifiedRegistrations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(nameof(UpdateUnverifiedRegistrationsAsync))
                    && await _fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false))
                    return;

                await UpdateUnverifiedRegistrationsAsync(log, filePath).ConfigureAwait(false);
                log.LogDebug($"Executed WebJob {nameof(UpdateUnverifiedRegistrationsAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed {nameof(UpdateUnverifiedRegistrationsAsync)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                StartedJobs.Add(nameof(UpdateUnverifiedRegistrationsAsync));
            }
        }

        /// <summary>
        ///     Generates a csv file of all unverified user registrations from the database
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="log"></param>
        public async Task UpdateUnverifiedRegistrationsAsync(ILogger log, string filePath)
        {
            if (RunningJobs.Contains(nameof(UpdateUnverifiedRegistrationsAsync)))
            {
                log.LogWarning($"'{nameof(UpdateUnverifiedRegistrationsAsync)}' is already running.");
                return;
            }

            RunningJobs.Add(nameof(UpdateUnverifiedRegistrationsAsync));
            try
            {
                var userOrgs = _dataRepository.GetAll<UserOrganisation>()
                    .Where(uo => uo.PINConfirmedDate == null)
                    .OrderBy(uo => uo.Organisation.OrganisationName)
                    .Include(uo => uo.Organisation.LatestScope)
                    .Include(uo => uo.User)
                    .ToList();
                var records = userOrgs.Select(
                        uo => new
                        {
                            uo.Organisation.OrganisationId,
                            uo.Organisation.DUNSNumber,
                            uo.Organisation.OrganisationReference,
                            uo.Organisation.OrganisationName,
                            CompanyNo = uo.Organisation.CompanyNumber,
                            Sector = uo.Organisation.SectorType,
                            LatestStatement = uo.Organisation?.LatestStatement?.StatusDate,
                            uo.Method,
                            uo.Organisation.LatestScope?.ScopeStatus,
                            ScopeDate = uo.Organisation.LatestScope?.ScopeStatusDate,
                            uo.User.Fullname,
                            uo.User.JobTitle,
                            uo.User.EmailAddress,
                            uo.User.ContactFirstName,
                            uo.User.ContactLastName,
                            uo.User.ContactJobTitle,
                            uo.User.ContactEmailAddress,
                            uo.User.ContactPhoneNumber,
                            uo.User.ContactOrganisation,
                            uo.PINSentDate,
                            uo.PINConfirmedDate,
                            uo.Created,
                            Address = uo.Address?.GetAddressString()
                        })
                    .ToList();

                if (records.Any())await Extensions.SaveCSVAsync(_fileRepository, records, filePath).ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateUnverifiedRegistrationsAsync));
            }
        }
    }
}