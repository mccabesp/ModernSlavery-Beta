using System;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureResourceManagers
{
    public class AuthenticationManager    {

        public const string ConfigSubscriptionId = "SubscriptionId";
        public const string ConfigTenantId = "TenantId";
        public const string ConfigClientId = "ClientId";
        public const string ConfigClientSecret = "ClientSecret";


        private AzureCredentials GetCredentials(string clientId = null, string clientSecret = null, string tenantId = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))clientId = Environment.GetEnvironmentVariable(ConfigClientId);
            if (string.IsNullOrWhiteSpace(clientId))throw new ArgumentNullException(nameof(clientId),$"You must provide a '{nameof(clientId)}' or specify '{ConfigClientId}' as an environment variable");

            if (string.IsNullOrWhiteSpace(clientSecret))clientSecret = Environment.GetEnvironmentVariable(ConfigClientSecret);
            if (string.IsNullOrWhiteSpace(clientSecret))throw new ArgumentNullException(nameof(clientSecret),$"You must provide a '{nameof(clientSecret)}' or specify '{ConfigClientSecret}' as an environment variable");

            if (string.IsNullOrWhiteSpace(tenantId))tenantId = Environment.GetEnvironmentVariable(ConfigTenantId);
            if (string.IsNullOrWhiteSpace(tenantId))throw new ArgumentNullException(nameof(tenantId),$"You must provide a '{nameof(tenantId)}' or specify '{ConfigTenantId}' as an environment variable");

            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId,clientSecret,tenantId,AzureEnvironment.AzureGlobalCloud);
        }

        public IAzure Authenticate(IAzureOptions azureOptions)
        {
            //=================================================================
            // Authenticate
            var credentials = GetCredentials(azureOptions.ClientId, azureOptions.ClientSecret, azureOptions.TenantId);

            return Authenticate(credentials, azureOptions.SubscriptionId);
        }

        public IAzure Authenticate(string clientId, string clientSecret, string tenantId = null, string subscriptionId = null)
        {
            //=================================================================
            // Authenticate
            var credentials = GetCredentials(clientId, clientSecret, tenantId);

            return Authenticate(credentials, subscriptionId);
        }

        public IAzure Authenticate(AzureCredentials credentials, string subscriptionId = null)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))subscriptionId = Environment.GetEnvironmentVariable(ConfigSubscriptionId);

            if (!string.IsNullOrWhiteSpace(subscriptionId))
                return Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithSubscription(subscriptionId);

            return Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();
        }

    }
}
