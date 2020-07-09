using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using System;
using System.Configuration;
using System.Collections.Generic;

namespace ModernSlavery.Hosts.Web
{

    [Options("IdentityClient")]
    public class IdentityClientOptions : IOptions
    {
        private readonly SharedOptions _sharedOptions;
        public IdentityClientOptions(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public string IssuerUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SignOutUri { get; set; }
        public bool AllowInvalidServerCertificates { get; set; }

        public void Validate()
        {
            var exceptions = new List<Exception>();

            IssuerUri = IssuerUri?.TrimEnd('/');

            if (!IssuerUri.IsUrl()) exceptions.Add(new ConfigurationErrorsException($"Invalid Uri in configuration 'IdentityClient:IssuerUri'"));

            if (string.IsNullOrWhiteSpace(ClientId)) exceptions.Add(new ConfigurationErrorsException($"Missing CliendId in configuration 'IdentityClient:CliendId'"));

            if (string.IsNullOrWhiteSpace(ClientSecret)) exceptions.Add(new ConfigurationErrorsException($"Missing ClientSecret in configuration 'IdentityClient:ClientSecret'"));

            if (AllowInvalidServerCertificates && _sharedOptions.IsProduction()) exceptions.Add(new ConfigurationErrorsException($"IdentityClient:AllowInvalidServerCertificates=true is not allowed in production environment'"));

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }

            ClientSecret = ClientSecret.GetSHA256Checksum();
        }
    }
}
