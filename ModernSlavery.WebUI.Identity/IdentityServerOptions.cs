using IdentityServer4.Models;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using System;
using System.Configuration;
using System.Linq;

namespace ModernSlavery.WebUI.Identity
{

    [Options("IdentityServer")]
    public class IdentityServerOptions : IOptions
    {
        private const string DefaultClientSecret = "Secret";
        private readonly SharedOptions _sharedOptions;

        public IdentityServerOptions(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public Client[] Clients { get; set; }

        public void Validate()
        {
            CheckClientSecrets();

            CheckClientUri();
        }

        private void CheckClientSecrets()
        {
            if (Clients == null) return;

            for (var i = 0; i < Clients.Length; i++)
            {
                var client = Clients[i];
                if (client.ClientSecrets == null || !client.ClientSecrets.Any())
                    throw new ConfigurationException($"No Identity Server client secret found in configuration 'IdentityServer:Clients:{i}:ClientSecrets'");

                var clientSecrets = client.ClientSecrets.ToArray();
                for (var s = 0; s < clientSecrets.Length; s++)
                {
                    var secret = clientSecrets[s];
                    if (secret == null || string.IsNullOrWhiteSpace(secret.Value))
                        throw new ConfigurationException($"No Identity Server client secret found in configuration 'IdentityServer:Clients:{i}:ClientSecrets:{s}:Value'");

                    if ((_sharedOptions.IsProduction() || _sharedOptions.IsPreProduction()) && secret.Value.ContainsI(DefaultClientSecret))
                        throw new ConfigurationException($"Identity Server client secret cannot contain '{DefaultClientSecret}' in configuration 'IdentityServer:Clients:{i}:ClientSecrets:{s}:Value'");

                    clientSecrets[s].Value = secret.Value.GetSHA256Checksum();
                }
            }
        }

        private void CheckClientUri()
        {
            if (Clients == null) return;

            for (var i = 0; i < Clients.Length; i++)
            {
                var client = Clients[i];
                if (!client.ClientUri.IsUrl()) throw new ConfigurationException($"Invalid Uri in configuration 'IdentityServer:Clients:{i}:ClientUri'");

                if (client.RedirectUris != null)
                    client.RedirectUris = client.RedirectUris.Select(uri => RootUri(uri, client.ClientUri)).ToList();

                if (client.PostLogoutRedirectUris != null)
                    client.PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(uri => RootUri(uri, client.ClientUri)).ToList();
            }
        }

        private static string RootUri(string uri, string authority)
        {
            return uri.StartsWithI("http") ? uri : $"{authority.TrimEnd("/\\".ToCharArray())}/{uri.TrimStart("/\\".ToCharArray())}";
        }
    }
}
