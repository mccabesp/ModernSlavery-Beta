using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace ModernSlavery.Extensions.AspNetCore
{
    public static partial class Extensions
    {

        public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration config)
        {
            var cacheType= config.GetValue("DistributedCache:Type", "Memory");

            switch (cacheType.ToLower())
            {
                case "redis":
                    var redisConnectionString = config.GetConnectionString("RedisCache");
                    if (string.IsNullOrWhiteSpace(redisConnectionString)) redisConnectionString = config.GetValue("DistributedCache:ConnectionString", "");
                    if (string.IsNullOrWhiteSpace(redisConnectionString)) throw new Exception("Cannot find 'RedisCache' ConnectionString or 'DistributedCache:ConnectionString'");
                    services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
                    break;
                case "memory":
                    //Use a memory cache
                    services.AddDistributedMemoryCache();
                    break;
                default:
                    throw new Exception($"Unrecognised DistributedCache:Type='{cacheType}'");
            }

            return services;
        }

        public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration config)
        {
            var storageType = config.GetValue("DataProtection:Type", "Local");
            var applicationDiscriminator = config.GetValue("DataProtection:ApplicationDiscriminator", "");

            switch (storageType.ToLower())
            {
                case "redis":
                    var redisConnectionString = config.GetConnectionString("RedisCache");
                    if (string.IsNullOrWhiteSpace(redisConnectionString)) redisConnectionString = config.GetValue("DistributedCache:ConnectionString", "");
                    if (string.IsNullOrWhiteSpace(redisConnectionString)) redisConnectionString = config.GetValue("DataProtection:ConnectionString", "");
                    if (string.IsNullOrWhiteSpace(redisConnectionString)) throw new Exception("Cannot find 'RedisCache' ConnectionString or 'DistributedCache:ConnectionString' or 'DataProtection:ConnectionString'");
                    
                    var keyName = config.GetValue("DataProtection:KeyName", "DataProtection-Keys");
                    if (string.IsNullOrWhiteSpace(keyName)) throw new Exception("Invalid or missing setting 'DataProtection:KeyName'");

                    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
                    services.AddDataProtection(options =>{
                        options.ApplicationDiscriminator = applicationDiscriminator;
                    }).PersistKeysToStackExchangeRedis(redis, keyName);
                    break;
                case "blob":
                    //Use blob storage to persist data protection keys equivalent to old MachineKeys
                    string storageConnectionString = config.GetConnectionString("AzureStorage");
                    if (string.IsNullOrWhiteSpace(storageConnectionString)) storageConnectionString = config.GetValue("DataProtection:ConnectionString", "");
                    if (string.IsNullOrWhiteSpace(storageConnectionString)) throw new Exception("Cannot find 'AzureStorage' ConnectionString or 'DataProtection:ConnectionString'");

                    var containerName = config.GetValue("DataProtection:Container", "shared-configuration");
                    if (string.IsNullOrWhiteSpace(containerName)) throw new Exception("Invalid or missing setting 'DataProtection:Container'");

                    var keyFilePath = config.GetValue("DataProtection:FilePath", "data-protection/keys.xml");
                    if (string.IsNullOrWhiteSpace(keyFilePath)) throw new Exception("Invalid or missing setting 'DataProtection:FilePath'");

                    //Get or create the container automatically
                    var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var keyContainer = blobClient.GetContainerReference(containerName);
                    keyContainer.CreateIfNotExists();

                    services.AddDataProtection(options => {
                        options.ApplicationDiscriminator = applicationDiscriminator;
                    }).PersistKeysToAzureBlobStorage(keyContainer, keyFilePath);
                    break;
                case "memory":
                    services.AddDataProtection(options => {
                        options.ApplicationDiscriminator = applicationDiscriminator;
                    });
                    break;
                case "none":
                    break;
                default:
                    throw new Exception($"Unrecognised DataProtection:Type='{storageType}'");
            }

            return services;
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
                    options => {
                        options.DefaultScheme = "Cookies";
                        options.DefaultChallengeScheme = "oidc";
                    })
                .AddOpenIdConnect(
                    "oidc",
                    options => {
                        options.SignInScheme = "Cookies";
                        options.Authority = authority;
                        options.RequireHttpsMetadata = true;
                        options.ClientId = clientId;
                        if (!string.IsNullOrWhiteSpace(clientSecret))
                        {
                            options.ClientSecret = clientSecret.GetSHA256Checksum();
                        }

                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("roles");
                        options.SaveTokens = true;
                        options.SignedOutRedirectUri = signedOutRedirectUri;
                        options.Events.OnRedirectToIdentityProvider = context => {
                            Uri referrer = context.HttpContext?.GetUri();
                            if (referrer != null)
                            {
                                context.ProtocolMessage.SetParameter("Referrer", referrer.PathAndQuery);
                            }

                            return Task.CompletedTask;
                        };
                        options.BackchannelHttpHandler = backchannelHttpHandler;
                    })
                .AddCookie(
                    "Cookies",
                    options => {
                        options.AccessDeniedPath = "/Error/403"; //Show forbidden error page
                    });
            return services;
        }

    }


}
