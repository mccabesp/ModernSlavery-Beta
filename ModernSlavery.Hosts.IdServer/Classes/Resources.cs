using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Hosts.IdServer.Classes
{
    public interface IResources
    {
        IEnumerable<IdentityResource> GetIdentityResources();
        IEnumerable<ApiResource> GetApiResources();
    }

    public class Resources : IResources
    {
        private readonly SharedOptions _sharedOptions;

        public Resources(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource {Name = "roles", UserClaims = new List<string> {ClaimTypes.Role}}
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = _sharedOptions.IdentityApiScope,
                    DisplayName = "GPG API",
                    Description = "Access to a GPG API",
                    UserClaims = new List<string> {ClaimTypes.Role},
                    ApiSecrets = new List<Secret> {new Secret(_sharedOptions.IdServerSecret.GetSHA256Checksum())},
                    Scopes = new List<Scope> {new Scope("customAPI.read"), new Scope("customAPI.write")}
                }
            };
        }
    }
}