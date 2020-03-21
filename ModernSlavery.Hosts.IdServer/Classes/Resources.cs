using System;
using System.Collections.Generic;
using System.Security.Claims;
using ModernSlavery.Extensions;
using IdentityServer4.Models;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.IdentityServer4.Classes
{
    public interface IResources
    {
        IEnumerable<IdentityResource> GetIdentityResources();
        IEnumerable<ApiResource> GetApiResources();
    }

    public class Resources : IResources
    {
        private readonly GlobalOptions _globalOptions;
        public Resources(GlobalOptions globalOptions)
        {
            _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
        }
        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource> {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource {Name = "roles", UserClaims = new List<string> {ClaimTypes.Role}}
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> {
                new ApiResource {
                    Name = _globalOptions.IdentityApiScope,
                    DisplayName = "GPG API",
                    Description = "Access to a GPG API",
                    UserClaims = new List<string> {ClaimTypes.Role},
                    ApiSecrets = new List<Secret> {new Secret(_globalOptions.AuthSecret.GetSHA256Checksum())},
                    Scopes = new List<Scope> {new Scope("customAPI.read"), new Scope("customAPI.write")}
                }
            };
        }

    }
}
