using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {

        public async Task CheckSiteCertAsync([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                if (_CommonBusinessLogic.GlobalOptions.CertExpiresWarningDays > 0)
                {
                    //Get the cert thumbprint
                    string certThumprint = _CommonBusinessLogic.GlobalOptions.WEBSITE_LOAD_CERTIFICATES.SplitI(";").FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(certThumprint))certThumprint = _CommonBusinessLogic.GlobalOptions.CertThumprint.SplitI(";").FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(certThumprint))
                    {
                        //Load the cert from the thumprint
                        X509Certificate2 cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);

                        DateTime expires = cert.GetExpirationDateString().ToDateTime();
                        if (expires < VirtualDateTime.UtcNow)
                        {
                            await _Messenger.SendGeoMessageAsync(
                                "GPG - WEBSITE CERTIFICATE EXPIRED",
                                $"The website certificate for '{_CommonBusinessLogic.GlobalOptions.EXTERNAL_HOST}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                        }
                        else
                        {
                            TimeSpan remainingTime = expires - VirtualDateTime.Now;

                            if (expires < VirtualDateTime.UtcNow.AddDays(_CommonBusinessLogic.GlobalOptions.CertExpiresWarningDays))
                            {
                                await _Messenger.SendGeoMessageAsync(
                                    "GPG - WEBSITE CERTIFICATE EXPIRING",
                                    $"The website certificate for '{_CommonBusinessLogic.GlobalOptions.EXTERNAL_HOST}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
                            }
                        }
                    }
                }

                log.LogDebug($"Executed {nameof(CheckSiteCertAsync)} successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(CheckSiteCertAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

    }
}
