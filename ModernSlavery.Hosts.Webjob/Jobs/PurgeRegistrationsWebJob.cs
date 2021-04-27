using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class PurgeRegistrationsWebJob : WebJob
    {
        #region Dependencies
        private readonly IAuditLogger _manualChangeLog;
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        #endregion

        public PurgeRegistrationsWebJob(
            [KeyFilter(Filenames.ManualChangeLog)] IAuditLogger manualChangeLog,
            ISmtpMessenger messenger,
            SharedOptions sharedOptions,
            IDataRepository dataRepository)
        {
            _manualChangeLog = manualChangeLog;
            _messenger = messenger;
            _sharedOptions = sharedOptions;
            _dataRepository = dataRepository;
        }

        //Remove any incomplete registrations
        [Disable(typeof(DisableWebJobProvider))]
        public async Task PurgeRegistrationsAsync([TimerTrigger("%PurgeRegistrations%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(PurgeRegistrationsAsync))) return;
            RunningJobs.Add(nameof(PurgeRegistrationsAsync));
            try
            {
                var deletedCount = 0;

                var deadline = VirtualDateTime.Now.AddDays(0 - _sharedOptions.PurgeUnconfirmedPinDays);
                var registrations = await _dataRepository.GetAll<UserOrganisation>()
                    .Where(u => u.PINConfirmedDate == null && u.PINSentDate != null && u.PINSentDate.Value < deadline)
                    .ToListAsync().ConfigureAwait(false);
                foreach (var registration in registrations)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeRegistrationsAsync),
                        ManualActions.Delete,
                        $"TABLE {nameof(UserOrganisation)}",
                        $"{nameof(UserOrganisation.OrganisationId)}:{nameof(UserOrganisation.UserId)}",
                        $"{registration.OrganisationId}:{registration.UserId}",
                        null,
                        Json.SerializeObject(
                            new
                            {
                                registration.UserId,
                                registration.User.EmailAddress,
                                registration.OrganisationId,
                                registration.Organisation.OrganisationReference,
                                registration.Organisation.OrganisationName,
                                registration.Method,
                                registration.PINSentDate,
                                registration.PINConfirmedDate
                            }),
                        null);
                    _dataRepository.Delete(registration);
                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                    deletedCount++;

                    await _manualChangeLog.WriteAsync(logItem).ConfigureAwait(false);
                }

                log.LogDebug($"Executed WebJob {nameof(PurgeRegistrationsAsync)} successfully. Deleted: {deletedCount}.");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(PurgeRegistrationsAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(PurgeRegistrationsAsync));
            }

        }
    }
}