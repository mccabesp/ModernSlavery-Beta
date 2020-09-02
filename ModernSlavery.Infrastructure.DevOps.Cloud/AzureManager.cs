using System;
using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ModernSlavery.Infrastructure.Azure
{
    public class AzureManager
    {

        public const string ConfigSubscriptionId = "SubscriptionId";
        public const string ConfigTenantId = "TenantId";
        public const string ConfigClientId = "ClientId";
        public const string ConfigClientSecret = "ClientSecret";
        public static IAzure Azure = null;

        private AzureCredentials GetCredentials(string clientId = null, string clientSecret = null, string tenantId = null)
        {
            if (string.IsNullOrWhiteSpace(clientId)) clientId = Environment.GetEnvironmentVariable(ConfigClientId);
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientId), $"You must provide a '{nameof(clientId)}' or specify '{ConfigClientId}' as an environment variable");

            if (string.IsNullOrWhiteSpace(clientSecret)) clientSecret = Environment.GetEnvironmentVariable(ConfigClientSecret);
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentNullException(nameof(clientSecret), $"You must provide a '{nameof(clientSecret)}' or specify '{ConfigClientSecret}' as an environment variable");

            if (string.IsNullOrWhiteSpace(tenantId)) tenantId = Environment.GetEnvironmentVariable(ConfigTenantId);
            if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId), $"You must provide a '{nameof(tenantId)}' or specify '{ConfigTenantId}' as an environment variable");

            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        public IAzure Authenticate(AzureOptions azureOptions)
        {
            if (Azure == null)
            {
                //=================================================================
                // Authenticate
                var credentials = GetCredentials(azureOptions.ClientId, azureOptions.ClientSecret, azureOptions.TenantId);

                Azure = Authenticate(credentials, azureOptions.SubscriptionId);
            }
            return Azure;
        }

        public IAzure Authenticate(string clientId, string clientSecret, string tenantId = null, string subscriptionId = null)
        {
            if (Azure == null)
            {
                //=================================================================
                // Authenticate
                var credentials = GetCredentials(clientId, clientSecret, tenantId);

                Azure = Authenticate(credentials, subscriptionId);
            }
            return Azure;
        }

        public IAzure Authenticate(AzureCredentials credentials, string subscriptionId = null)
        {
            if (Azure == null)
            {
                if (string.IsNullOrWhiteSpace(subscriptionId)) subscriptionId = Environment.GetEnvironmentVariable(ConfigSubscriptionId);
                if (!string.IsNullOrWhiteSpace(subscriptionId))
                    Azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithSubscription(subscriptionId);
                else
                    Azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithDefaultSubscription();
            }
            return Azure;
        }

        private static AuthenticationContext GetAuthenticationContext(string tenantId = null)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) tenantId = Environment.GetEnvironmentVariable(ConfigTenantId);

            AuthenticationContext ctx = null;
            if (tenantId != null)
                ctx = new AuthenticationContext("https://login.microsoftonline.com/" + tenantId);
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
    }
}
