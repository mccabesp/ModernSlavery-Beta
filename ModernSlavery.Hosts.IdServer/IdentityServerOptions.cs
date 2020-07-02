using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.IdServer
{

    [Options("IdentityServer")]
    public class IdentityServerOptions : IOptions
    {
        private const string DefaultClientSecret = "Secret";
        private readonly SharedOptions _sharedOptions;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string,string> _configurationDictionary;

        public IdentityServerOptions(SharedOptions sharedOptions, IConfiguration configuration)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _configurationDictionary = _configuration.ToDictionary();
        }

        public Client[] Clients { get; set; }

        public void Validate()
        {
            CheckClientSecrets();

            CheckClientUri();
        }

        private void CheckClientSecrets()
        {
            if (Clients==null) return;

            for(var i=0;i<Clients.Length;i++)
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
                }
            }
        }

        private void CheckClientUri()
        {
            if (Clients == null) return;

            for (var i = 0; i < Clients.Length; i++)
            {
                var client = Clients[i];
                if (string.IsNullOrWhiteSpace(client.ClientUri)) client.ClientUri=_sharedOptions.SiteAuthority;

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
