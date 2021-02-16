using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using StackExchange.Redis;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static partial class Extensions
    {
        public static void AddDataProtection(this IServiceCollection services, DataProtectionOptions dataProtectionOptions)
        {
            switch (dataProtectionOptions.Type.ToLower())
            {
                case "redis":
                    var redis = ConnectionMultiplexer.Connect(dataProtectionOptions.AzureConnectionString);
                    services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    }).PersistKeysToStackExchangeRedis(redis, dataProtectionOptions.KeyName);
                    break;
                case "blob":
                    //Use blob storage to persist data protection keys equivalent to old MachineKeys

                    //Get or create the container automatically
                    var storageAccount = CloudStorageAccount.Parse(dataProtectionOptions.AzureConnectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var keyContainer = blobClient.GetContainerReference(dataProtectionOptions.Container);
                    keyContainer.CreateIfNotExists();

                    services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    }).PersistKeysToAzureBlobStorage(keyContainer, dataProtectionOptions.KeyFilepath);
                    break;
                case "memory":
                    services.AddDataProtection(options =>
                    {
                        options.ApplicationDiscriminator = dataProtectionOptions.ApplicationDiscriminator;
                    });
                    break;
                case "none":
                    break;
                default:
                    throw new Exception($"Unrecognised DataProtection:Type='{dataProtectionOptions.Type}'");
            }
        }

        /// <summary>
        ///     Configure the Owin authentication for Identity Server
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddIdentityServerClient(this IServiceCollection services,
            SharedOptions sharedOptions,
            string authority,
            string clientId,
            string clientSecret,
            string signedOutRedirectUri,
            bool allowInvalidServerCertificates=false)
        {
            //Turn off the JWT claim type mapping to allow well-known claims (e.g. ‘sub’ and ‘idp’) to flow through unmolested
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(
                    options => 
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;//IdentityServerConstants.DefaultCheckSessionCookieName;
                        options.DefaultChallengeScheme = "oidc";
                    })
                .AddOpenIdConnect(
                    "oidc",
                    options => 
                    {
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.Authority = authority;
                        options.RequireHttpsMetadata = true;
                        options.ClientId = clientId;
                        options.ClientSecret = clientSecret;
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

                        //Ignore self-signed or invalid certificates from identity server
                        var clientHandler = new HttpClientHandler();
                        if (allowInvalidServerCertificates)
                            clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                        options.BackchannelHttpHandler = clientHandler;
                    })
                .AddCookie(
                    CookieAuthenticationDefaults.AuthenticationScheme,//IdentityServerConstants.DefaultCheckSessionCookieName;
                    options =>
                    {
                        options.Cookie.IsEssential = true;//Authentication cookies are allowed when a site visitor hasn't consented to data collection. For more information, see General Data Protection Regulation (GDPR) support in ASP.NET Core. https://docs.microsoft.com/en-us/aspnet/core/security/gdpr?view=aspnetcore-5.0#essential-cookies
                        options.Cookie.SecurePolicy =  Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(sharedOptions.SessionTimeOutMinutes);
                        options.SlidingExpiration = true;
                        options.AccessDeniedPath = "/Error/403"; //Show forbidden error page
                        options.Events.OnRedirectToAccessDenied = new Func<Microsoft.AspNetCore.Authentication.RedirectContext<CookieAuthenticationOptions>, Task>(context => {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthorizeAttribute>>();

                            var user = !context.HttpContext.User.Identity.IsAuthenticated ? "Anonymous" : context.HttpContext.User.GetEmail() ?? context.HttpContext.User.GetName() ?? context.HttpContext.User.GetSubject().ToString();

                            logger.LogWarning($"Access to {context.Request.Path} by USER:{user} on IP:{context.HttpContext.GetUserHostAddress()} was forbidden");
                            context.Response.Redirect(options.AccessDeniedPath);
                            return context.Response.CompleteAsync();
                        });
                    });
            return services;
        }
    }
}