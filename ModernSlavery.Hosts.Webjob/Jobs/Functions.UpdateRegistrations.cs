using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task UpdateRegistrations([TimerTrigger(typeof(Functions.EveryWorkingHourSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                string filePath = Path.Combine(_SharedBusinessLogic.SharedOptions.DownloadsPath, Filenames.Registrations);

                //Dont execute on startup if file already exists
                if (!Functions.StartedJobs.Contains(nameof(UpdateRegistrations)) && await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                {
                    return;
                }

                await UpdateRegistrationsAsync(log, filePath);

                log.LogDebug($"Executed {nameof(UpdateRegistrations)}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {nameof(UpdateRegistrations)}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
            finally
            {
                Functions.StartedJobs.Add(nameof(UpdateRegistrations));
            }
        }

        public async Task UpdateRegistrationsAsync(ILogger log, string filePath)
        {
            if (Functions.RunningJobs.Contains(nameof(UpdateRegistrations)))
            {
                log.LogDebug($"'{nameof(UpdateRegistrations)}' is already running.");
                return;
            }

            Functions.RunningJobs.Add(nameof(UpdateRegistrations));
            try
            {
                List<UserOrganisation> userOrgs = Queryable.Where<UserOrganisation>(_SharedBusinessLogic.DataRepository.GetAll<UserOrganisation>(), uo => uo.User.Status == UserStatuses.Active && uo.PINConfirmedDate != null)
                    .OrderBy(uo => uo.Organisation.OrganisationName)
                    .Include(uo => uo.Organisation.LatestScope)
                    .Include(uo => uo.User)
                    .ToList();
                var records = userOrgs.Select(
                        uo => new {
                            uo.Organisation.OrganisationId,
                            uo.Organisation.DUNSNumber,
                            uo.Organisation.EmployerReference,
                            uo.Organisation.OrganisationName,
                            CompanyNo = uo.Organisation.CompanyNumber,
                            Sector = uo.Organisation.SectorType,
                            LatestReturn = uo.Organisation?.LatestReturn?.StatusDate,
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
                await Core.Classes.Extensions.SaveCSVAsync(_SharedBusinessLogic.FileRepository, records, filePath);
            }
            finally
            {
                Functions.RunningJobs.Remove(nameof(UpdateRegistrations));
            }
        }

    }
}
