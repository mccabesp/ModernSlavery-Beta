using System;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Hosts.IdServer.Classes
{
    public interface IClients
    {
        IEnumerable<Client> Get();
    }

    public class Clients : IClients
    {
        private readonly SharedOptions _sharedOptions;

        public Clients(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public IEnumerable<Client> Get()
        {
            if ((_sharedOptions.IsProduction() || _sharedOptions.IsPreProduction()) &&
                _sharedOptions.AuthSecret.EqualsI("secret", "", null))
                throw new Exception(
                    "Invalid ClientSecret for IdentityServer. You must set 'AuthSecret' to a unique key");

            return new[]
            {
                new Client
                {
                    ClientName = "Modern Slavery reporting service",
                    ClientId = "ModernSlaveryServiceWebsite",
                    ClientSecrets = new List<Secret> {new Secret(_sharedOptions.AuthSecret.GetSHA256Checksum())},
                    ClientUri = _sharedOptions.SiteAuthority,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =
                        new List<string>
                        {
                            _sharedOptions.SiteAuthority,
                            _sharedOptions.SiteAuthority + "signin-oidc",
                            _sharedOptions.SiteAuthority + "manage-organisations",
                            _sharedOptions.DoneUrl
                        },
                    PostLogoutRedirectUris =
                        new List<string>
                        {
                            _sharedOptions.SiteAuthority,
                            _sharedOptions.SiteAuthority + "signout-callback-oidc",
                            _sharedOptions.SiteAuthority + "manage-organisations",
                            _sharedOptions.SiteAuthority + "manage-account/complete-change-email",
                            _sharedOptions.SiteAuthority + "manage-account/close-account-completed",
                            _sharedOptions.DoneUrl
                        },
                    AllowedScopes =
                        new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile, "roles"
                        },
                    Properties = new Dictionary<string, string> {{"AutomaticRedirectAfterSignOut", "true"}}
                }
            };
        }
    }
}