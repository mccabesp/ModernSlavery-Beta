﻿using System;
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
            try
            {
                if (_SharedBusinessLogic.SharedOptions.CertExpiresWarningDays > 0)
                {
                    //Get the cert thumbprint
                    var certThumprint = _SharedBusinessLogic.SharedOptions.CertThumprint.SplitI(";")
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(certThumprint))
                    {
                        //Load the cert from the thumprint
                        var cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);

                        var expires = cert.GetExpirationDateString().ToDateTime();
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            await _Messenger.SendGeoMessageAsync(
                                "GPG - WEBSITE CERTIFICATE EXPIRED",
                                $"The website certificate for '{_SharedBusinessLogic.SharedOptions.WEBSITE_HOSTNAME}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.").ConfigureAwait(false);
                        }
                        else
                        {
                            var remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(_SharedBusinessLogic.SharedOptions
                                .CertExpiresWarningDays))
                                await _Messenger.SendGeoMessageAsync(
                                    "GPG - WEBSITE CERTIFICATE EXPIRING",
                                    $"The website certificate for '{_SharedBusinessLogic.SharedOptions.WEBSITE_HOSTNAME}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.").ConfigureAwait(false);
                        }
                    }
                }

                log.LogDebug($"Executed {nameof(CheckSiteCertAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(CheckSiteCertAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }
    }
}