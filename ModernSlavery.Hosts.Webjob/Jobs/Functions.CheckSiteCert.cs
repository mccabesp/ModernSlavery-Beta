using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Disable(typeof(DisableWebjobProvider))]
        public async Task CheckSiteCertAsync([TimerTrigger("%CheckSiteCertAsync%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(CheckSiteCertAsync))) return;
            RunningJobs.Add(nameof(CheckSiteCertAsync));
            try
            {
                if (_sharedOptions.CertExpiresWarningDays > 0)
                {
                    //Get the cert thumbprint
                    var certThumprint = _sharedOptions.CertThumprint.SplitI(";")
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(certThumprint))
                    {
                        //Load the cert from the thumprint
                        var cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);

                        var expires = cert.GetExpirationDateString().ToDateTime();
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            await _messenger.SendMsuMessageAsync(
                                "GPG - WEBSITE CERTIFICATE EXPIRED",
                                $"The website certificate for '{_sharedOptions.WEBSITE_HOSTNAME}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.").ConfigureAwait(false);
                        }
                        else
                        {
                            var remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(_sharedOptions
                                .CertExpiresWarningDays))
                                await _messenger.SendMsuMessageAsync(
                                    "GPG - WEBSITE CERTIFICATE EXPIRING",
                                    $"The website certificate for '{_sharedOptions.WEBSITE_HOSTNAME}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.").ConfigureAwait(false);
                        }
                    }
                }

                log.LogDebug($"Executed {nameof(CheckSiteCertAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(CheckSiteCertAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
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