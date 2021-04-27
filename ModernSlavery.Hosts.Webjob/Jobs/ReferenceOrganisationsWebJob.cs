using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class ReferenceOrganisationsWebJob : WebJob
    {
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private readonly ISmtpMessenger _messenger;

        public ReferenceOrganisationsWebJob(IOrganisationBusinessLogic organisationBusinessLogic, ISmtpMessenger messenger)
        {
            _organisationBusinessLogic = organisationBusinessLogic;
            _messenger = messenger;
        }

        //Ensure all organisations have a unique organisation reference
        [Disable(typeof(DisableWebJobProvider))]
        public async Task ReferenceOrganisationsAsync([TimerTrigger("%ReferenceOrganisations%")] TimerInfo timer, ILogger log)
        {
            if (WebJob.RunningJobs.Contains(nameof(ReferenceOrganisationsAsync))) return;
            RunningJobs.Add(nameof(ReferenceOrganisationsAsync));
            try
            {
                await _organisationBusinessLogic.SetUniqueOrganisationReferencesAsync().ConfigureAwait(false);
                log.LogDebug($"Executed WebJob {nameof(ReferenceOrganisationsAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(ReferenceOrganisationsAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                WebJob.RunningJobs.Remove(nameof(ReferenceOrganisationsAsync));
            }
        }
    }
}