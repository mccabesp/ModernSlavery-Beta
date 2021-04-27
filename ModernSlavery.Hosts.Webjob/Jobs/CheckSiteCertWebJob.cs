using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Hosts.Webjob.Classes;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class CheckSiteCertWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly SharedOptions _sharedOptions;
        #endregion

        public CheckSiteCertWebJob(
            ISmtpMessenger messenger,
            SharedOptions sharedOptions)
        {
            _messenger = messenger;
            _sharedOptions = sharedOptions;
        }

        [Disable(typeof(DisableWebJobProvider))]
        public async Task CheckSiteCertAsync([TimerTrigger("%CheckSiteCert%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(CheckSiteCertAsync))) return;
            RunningJobs.Add(nameof(CheckSiteCertAsync));
            try
            {
                if (_sharedOptions.CertExpiresWarningDays > 0)
                {
                    //Get the cert thumbprint
                    var certThumbprint = _sharedOptions.CertThumbprint.SplitI(';')
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(certThumbprint))
                    {
                        //Load the cert from the thumprint
                        var cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumbprint);

                        var expires = cert.GetExpirationDateString().ToDateTime();
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            await _messenger.SendMsuMessageAsync(
                                "MSU - WEBSITE CERTIFICATE EXPIRED",
                                $"The website certificate for '{_sharedOptions.EXTERNAL_HOSTNAME}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.").ConfigureAwait(false);
                        }
                        else
                        {
                            var remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(_sharedOptions
                                .CertExpiresWarningDays))
                                await _messenger.SendMsuMessageAsync(
                                    "MSU - WEBSITE CERTIFICATE EXPIRING",
                                    $"The website certificate for '{_sharedOptions.EXTERNAL_HOSTNAME}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.").ConfigureAwait(false);
                        }
                    }
                }

                log.LogDebug($"Executed WebJob {nameof(CheckSiteCertAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(CheckSiteCertAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(CheckSiteCertAsync));
            }

        }
    }
}