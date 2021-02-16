using System;
using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace ModernSlavery.Infrastructure.Azure
{
    public class AzureManager
    {
        private readonly AzureOptions _azureOptions;
        public AzureManager(AzureOptions azureOptions)
        {
            _azureOptions = azureOptions;
        }

        public const string ConfigSubscriptionId = "SubscriptionId";
        public const string ConfigTenantId = "TenantId";
        public const string ConfigClientId = "ClientId";
        public const string ConfigClientSecret = "ClientSecret";
        public IAzure Azure = null;
        public string AccessToken = null;
        public AuthenticationHeaderValue AuthenticationHeader;


        private AzureCredentials GetAppRegistrationCredentials()
        {
            if (string.IsNullOrWhiteSpace(_azureOptions.ClientId)) throw new ArgumentNullException(nameof(_azureOptions.ClientId), $"You must provide a '{nameof(_azureOptions.ClientId)}' or specify '{ConfigClientId}' as an environment variable");
            if (string.IsNullOrWhiteSpace(_azureOptions.ClientSecret)) throw new ArgumentNullException(nameof(_azureOptions.ClientSecret), $"You must provide a '{nameof(_azureOptions.ClientSecret)}' or specify '{ConfigClientSecret}' as an environment variable");
            if (string.IsNullOrWhiteSpace(_azureOptions.TenantId)) throw new ArgumentNullException(nameof(_azureOptions.TenantId), $"You must provide a '{nameof(_azureOptions.TenantId)}' or specify '{ConfigTenantId}' as an environment variable");

            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(_azureOptions.ClientId, _azureOptions.ClientSecret, _azureOptions.TenantId, AzureEnvironment.AzureGlobalCloud);
        }

        private AzureCredentials GetAppServiceCredentials()
        {
            return SdkContext.AzureCredentialsFactory.FromSystemAssignedManagedServiceIdentity(MSIResourceType.AppService, AzureEnvironment.AzureGlobalCloud);
        }

        private ClientCredential GetClientCredential()
        {
            if (string.IsNullOrWhiteSpace(_azureOptions.ClientId)) throw new ArgumentNullException(nameof(_azureOptions.ClientId), $"You must provide a '{nameof(_azureOptions.ClientId)}' or specify '{ConfigClientId}' as an environment variable");

            if (string.IsNullOrWhiteSpace(_azureOptions.ClientSecret)) throw new ArgumentNullException(nameof(_azureOptions.ClientSecret), $"You must provide a '{nameof(_azureOptions.ClientSecret)}' or specify '{ConfigClientSecret}' as an environment variable");

            return new ClientCredential(_azureOptions.ClientId,_azureOptions.ClientSecret);
        }

        public IAzure Authenticate()
        {
            if (Azure == null)
            {
                //=================================================================
                // Authenticate
                var credentials = _azureOptions.HasCredentials() ? GetAppRegistrationCredentials() : GetAppServiceCredentials();

                Azure = Authenticate(credentials);
            }
            return Azure;
        }

        public IAzure AuthenticateAppService()
        {
            if (Azure == null)
            {
                //=================================================================
                // Authenticate
                var credentials = GetAppServiceCredentials();

                Azure = Authenticate(credentials);
            }
            return Azure;
        }

        public IAzure Authenticate(AzureCredentials credentials)
        {
            if (Azure == null)
            {
                if (!string.IsNullOrWhiteSpace(_azureOptions.SubscriptionId))
                    Azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithSubscription(_azureOptions.SubscriptionId);
                else
                    Azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithDefaultSubscription();
            }
            return Azure;
        }

        private AuthenticationContext GetAuthenticationContext()
        {
            AuthenticationContext ctx = null;
            if (_azureOptions.TenantId != null)
                ctx = new AuthenticationContext("https://login.microsoftonline.com/" + _azureOptions.TenantId);
            else
            {
                ctx = new AuthenticationContext("https://login.windows.net/common");
                if (ctx.TokenCache.Count > 0)
                {
                    string homeTenant = ctx.TokenCache.ReadItems().First().TenantId;
                    ctx = new AuthenticationContext("https://login.microsoftonline.com/" + homeTenant);
                }
            }

            return ctx;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(AccessToken)) return AccessToken;

            var clientCredential = GetClientCredential();
            var authenticationContext = GetAuthenticationContext();

            var result = await authenticationContext.AcquireTokenAsync("https://management.azure.com/", clientCredential).ConfigureAwait(false);

            if (result == null)throw new InvalidOperationException("Failed to obtain the JWT token");
            return result.AccessToken;
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
        {
            return AuthenticationHeader ?? (AuthenticationHeader = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync().ConfigureAwait(false)));
        }
        

    }
}
