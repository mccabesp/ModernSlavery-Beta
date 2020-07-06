using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using System.Configuration;

namespace ModernSlavery.Hosts.Web
{

    [Options("IdentityClient")]
    public class IdentityClientOptions : IOptions
    {
        public string IssuerUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SignOutUri { get; set; }

        public void Validate()
        {
            IssuerUri = IssuerUri?.TrimEnd('/');

            if (!IssuerUri.IsUrl()) throw new ConfigurationErrorsException($"Invalid Uri in configuration 'IdentityClient:IssuerUri'");

            if (string.IsNullOrWhiteSpace(ClientId)) throw new ConfigurationErrorsException($"Missing CliendId in configuration 'IdentityClient:CliendId'");

            if (string.IsNullOrWhiteSpace(ClientSecret)) throw new ConfigurationErrorsException($"Missing ClientSecret in configuration 'IdentityClient:ClientSecret'");

        }
    }
}
