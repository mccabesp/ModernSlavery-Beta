using IdentityServer4.Models;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
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

        public string PublicOrigin { get; set; }=null;
        public bool ShowPII { get; set; }
        public string DefaultSigninUri { get; set; }="/";
        public string DefaultSignoutUri { get; set; }="/";

        public Client[] Clients { get; set; }
        

        public void Validate()
        {
            var exceptions = new List<Exception>();

            if (!string.IsNullOrWhiteSpace(PublicOrigin))
            {
                PublicOrigin = PublicOrigin.TrimEnd('/', '\\',' ');
                if (!Uri.TryCreate(PublicOrigin, UriKind.Absolute, out Uri publicOriginUri))
                    exceptions.Add(new ConfigurationErrorsException($"Invalid Uri {nameof(PublicOrigin)}='{PublicOrigin}' in configuration"));
                else if (publicOriginUri.PathAndQuery!="/")
                    exceptions.Add(new ConfigurationErrorsException($"Invalid path in Uri {nameof(PublicOrigin)}='{PublicOrigin}' in configuration"));
            }
            exceptions.AddRange(CheckClientSecrets());

            exceptions.AddRange(CheckClientUri());

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

        private IEnumerable<Exception> CheckClientSecrets()
        {
            if (Clients != null)
                for (var i = 0; i < Clients.Length; i++)
                {
                    var client = Clients[i];

                    client.AllowedScopes = client.AllowedScopes?.Select(s => s?.ToLower()).ToList();
                    client.AllowedGrantTypes = client.AllowedGrantTypes?.Select(s => s?.ToLower()).ToList();

                    if (client.ClientSecrets == null || !client.ClientSecrets.Any())
                        yield return new ConfigurationErrorsException($"No Identity Server client secret found in configuration 'IdentityServer:Clients:{i}:ClientSecrets'");

                    var clientSecrets = client.ClientSecrets.ToArray();
                    for (var s = 0; s < clientSecrets.Length; s++)
                    {
                        var secret = clientSecrets[s];
                        if (secret == null || string.IsNullOrWhiteSpace(secret.Value))
                            yield return new ConfigurationErrorsException($"No Identity Server client secret found in configuration 'IdentityServer:Clients:{i}:ClientSecrets:{s}:Value'");

                        if ((_sharedOptions.IsProduction() || _sharedOptions.IsPreProduction()) && secret.Value.ContainsI(DefaultClientSecret))
                            yield return new ConfigurationErrorsException($"Identity Server client secret cannot contain '{DefaultClientSecret}' in configuration 'IdentityServer:Clients:{i}:ClientSecrets:{s}:Value'");

                        clientSecrets[s].Value = secret.Value.GetSHA256Checksum();
                    }
                }
        }

        private IEnumerable<Exception> CheckClientUri()
        {
            if (Clients != null)
                for (var i = 0; i < Clients.Length; i++)
                {

                    var client = Clients[i];
                    if (!client.ClientUri.IsUrl()) yield return new ConfigurationErrorsException($"Invalid Uri in configuration 'IdentityServer:Clients:{i}:ClientUri'");

                    if (client.RedirectUris != null)
                    {
                        if (DefaultSigninUri != null && !client.RedirectUris.Contains(DefaultSigninUri, StringComparer.OrdinalIgnoreCase))
                                client.RedirectUris.Add(DefaultSigninUri);

                        if (DefaultSignoutUri != null && !client.RedirectUris.Contains(DefaultSignoutUri, StringComparer.OrdinalIgnoreCase))
                            client.RedirectUris.Add(DefaultSignoutUri);

                        client.RedirectUris = client.RedirectUris.Select(uri => RootUri(uri, client.ClientUri)).ToList();
                    }

                    if (client.PostLogoutRedirectUris != null)
                    {
                        if (DefaultSignoutUri != null && !client.PostLogoutRedirectUris.Contains(DefaultSignoutUri, StringComparer.OrdinalIgnoreCase))
                            client.PostLogoutRedirectUris.Add(DefaultSignoutUri);

                        client.PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(uri => RootUri(uri, client.ClientUri)).ToList();
                    }

                    if (DefaultSigninUri != null) DefaultSigninUri = RootUri(DefaultSigninUri, client.ClientUri);
                    if (DefaultSignoutUri != null) DefaultSignoutUri = RootUri(DefaultSignoutUri, client.ClientUri);
                }
        }

        private static string RootUri(string uri, string authority)
        {
            return uri.StartsWithI("http") ? uri : $"{authority.TrimEnd("/\\".ToCharArray())}/{uri.TrimStart("/\\".ToCharArray())}";
        }
    }
}
