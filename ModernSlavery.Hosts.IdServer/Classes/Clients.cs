using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Extensions;
using IdentityServer4;
using IdentityServer4.Models;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.IdentityServer4.Classes
{
    public interface IClients
    {
        IEnumerable<Client> Get();
    }

    public class Clients : IClients
    {
        private readonly GlobalOptions _globalOptions;
        public Clients(GlobalOptions globalOptions)
        {
            _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
        }

        public IEnumerable<Client> Get()
        {
            if ((_globalOptions.IsProduction() || _globalOptions.IsPreProduction()) && _globalOptions.AuthSecret.EqualsI("secret", "", null))
            {
                throw new Exception("Invalid ClientSecret for IdentityServer. You must set 'AuthSecret' to a unique key");
            }

            return new[] {
                new Client {
                    ClientName = "Modern Slavery reporting service",
                    ClientId = "ModernSlaveryServiceWebsite",
                    ClientSecrets = new List<Secret> {new Secret(_globalOptions.AuthSecret.GetSHA256Checksum())},
                    ClientUri = _globalOptions.SiteAuthority,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris =
                        new List<string> {
                            _globalOptions.SiteAuthority,
                            _globalOptions.SiteAuthority + "signin-oidc",
                            _globalOptions.SiteAuthority + "manage-organisations",
                            _globalOptions.DoneUrl
                        },
                    PostLogoutRedirectUris =
                        new List<string> {
                            _globalOptions.SiteAuthority,
                            _globalOptions.SiteAuthority + "signout-callback-oidc",
                            _globalOptions.SiteAuthority + "manage-organisations",
                            _globalOptions.SiteAuthority + "manage-account/complete-change-email",
                            _globalOptions.SiteAuthority + "manage-account/close-account-completed",
                            _globalOptions.DoneUrl
                        },
                    AllowedScopes =
                        new List<string> {
                            IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile, "roles"
                        },
                    Properties = new Dictionary<string, string> {{"AutomaticRedirectAfterSignOut", "true"}}
                }
            };
        }
    }
}
