using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using StackExchange.Redis;
using DataProtectionOptions = ModernSlavery.Infrastructure.Hosts.DataProtectionOptions;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static (IServiceCollection, DistributedCacheOptions) AddDistributedCache(
            this IServiceCollection services, DistributedCacheOptions cacheOptions)
        {
            switch (cacheOptions.Type.ToLower())
            {
                case "redis":
                    if (string.IsNullOrWhiteSpace(cacheOptions.AzureConnectionString))
                        throw new Exception("Cannot find 'DistributedCache:AzureConnectionString'");

                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = cacheOptions.AzureConnectionString;
                    });
                    break;
                case "memory":
                    //Use a memory cache
                    services.AddDistributedMemoryCache();
                    break;
                default:
                    throw new Exception($"Unrecognised DistributedCache:Type='{cacheOptions.Type}'");
            }

            return (services, cacheOptions);
        }

        public static IServiceCollection AddDataProtection(
            this (IServiceCollection Services, DistributedCacheOptions CacheOptions) options,
            DataProtectionOptions dataProtectionOptions)
        {
            switch (dataProtectionOptions.Type.ToLower())
            {
                case "redis":
                    if (string.IsNullOrWhiteSpace(options.CacheOptions.AzureConnectionString))
                        throw new Exception("Cannot 'DistributedCache:AzureConnectionString'");

                    if (string.IsNullOrWhiteSpace(dataProtectionOptions.KeyName))
                        throw new Exception("Invalid or missing setting 'DataProtection:KeyName'");

                    var redis = ConnectionMultiplexer.Connect(options.CacheOptions.AzureConnectionString);
                    options.Services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    }).PersistKeysToStackExchangeRedis(redis, dataProtectionOptions.KeyName);
                    break;
                case "blob":
                    //Use blob storage to persist data protection keys equivalent to old MachineKeys
                    if (string.IsNullOrWhiteSpace(options.CacheOptions.AzureConnectionString))
                        throw new Exception("Cannot 'DistributedCache:AzureConnectionString'");

                    if (string.IsNullOrWhiteSpace(dataProtectionOptions.Container))
                        throw new Exception("Invalid or missing setting 'DataProtection:Container'");

                    if (string.IsNullOrWhiteSpace(dataProtectionOptions.KeyFilepath))
                        throw new Exception("Invalid or missing setting 'DataProtection:KeyFilePath'");

                    //Get or create the container automatically
                    var storageAccount = CloudStorageAccount.Parse(options.CacheOptions.AzureConnectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var keyContainer = blobClient.GetContainerReference(dataProtectionOptions.Container);
                    keyContainer.CreateIfNotExists();

                    options.Services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    }).PersistKeysToAzureBlobStorage(keyContainer, dataProtectionOptions.KeyFilepath);
                    break;
                case "memory":
                    options.Services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    });
                    break;
                case "none":
                    break;
                default:
                    throw new Exception($"Unrecognised DataProtection:Type='{dataProtectionOptions.Type}'");
            }

            return options.Services;
        }

        /// <summary>
        ///     Configure the Owin authentication for Identity Server
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddIdentityServerClient(this IServiceCollection services,
            string authority,
            string signedOutRedirectUri,
            string clientId,
            string clientSecret = null,
            HttpMessageHandler backchannelHttpHandler = null)
        {
            //Turn off the JWT claim type mapping to allow well-known claims (e.g. ‘sub’ and ‘idp’) to flow through unmolested
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultScheme = "Cookies";
                        options.DefaultChallengeScheme = "oidc";
                    })
                .AddOpenIdConnect(
                    "oidc",
                    options =>
                    {
                        options.SignInScheme = "Cookies";
                        options.Authority = authority;
                        options.RequireHttpsMetadata = true;
                        options.ClientId = clientId;
                        if (!string.IsNullOrWhiteSpace(clientSecret))
                            options.ClientSecret = clientSecret.GetSHA256Checksum();

                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("roles");
                        options.SaveTokens = true;
                        options.SignedOutRedirectUri = signedOutRedirectUri;
                        options.Events.OnRedirectToIdentityProvider = context =>
                        {
                            var referrer = context.HttpContext?.GetUri();
                            if (referrer != null)
                                context.ProtocolMessage.SetParameter("Referrer", referrer.PathAndQuery);

                            return Task.CompletedTask;
                        };
                        options.BackchannelHttpHandler = backchannelHttpHandler;
                    })
                .AddCookie(
                    "Cookies",
                    options =>
                    {
                        options.AccessDeniedPath = "/Error/403"; //Show forbidden error page
                    });
            return services;
        }
    }
}