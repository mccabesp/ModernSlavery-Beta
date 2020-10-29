using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ModernSlavery.Infrastructure.Azure
{
    public class DevOpsManager
    {
        public const string ConfigDevOpsOrganisation = "Organisation";
        public const string ConfigPersonalAccessToken = "PAT";

        private string _organisationName;
        private string _personalAccessToken;
        public DevOpsManager(string organisationName, string personalAccessToken)
        {
            if (string.IsNullOrWhiteSpace(organisationName)) organisationName = Environment.GetEnvironmentVariable(ConfigDevOpsOrganisation);
            if (string.IsNullOrWhiteSpace(organisationName)) throw new ArgumentNullException(nameof(organisationName), $"You must provide a '{nameof(organisationName)}' or specify '{ConfigDevOpsOrganisation}' as an environment variable");

            if (string.IsNullOrWhiteSpace(personalAccessToken)) personalAccessToken = Environment.GetEnvironmentVariable(ConfigPersonalAccessToken);
            if (string.IsNullOrWhiteSpace(personalAccessToken)) throw new ArgumentNullException(nameof(personalAccessToken), $"You must provide a '{nameof(personalAccessToken)}' or specify '{ConfigPersonalAccessToken}' as an environment variable");

            _organisationName = organisationName;
            _personalAccessToken = personalAccessToken;
            AuthHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken))));
        }

        private VssConnection _connection=null;
        public VssConnection Connection=> _connection??=GetConnection();

        public readonly AuthenticationHeaderValue AuthHeader;

        // MSA backed accounts will return Guid.Empty
        private Guid GetAccountTenant(string vstsAccountName)
        {
            Guid tenantGuid = Guid.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(String.Format("https://{0}.visualstudio.com", vstsAccountName));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "VSTSAuthSample-AuthenticateADALNonInteractive");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                HttpResponseMessage response = client.GetAsync("_apis/connectiondata").Result;

                // Get the tenant from the Login URL
                var wwwAuthenticateHeaderResults = response.Headers.WwwAuthenticate.ToList();
                var bearerResult = wwwAuthenticateHeaderResults.Where(p => p.Scheme == "Bearer");
                foreach (var item in wwwAuthenticateHeaderResults)
                {
                    if (item.Scheme.StartsWith("Bearer"))
                    {
                        tenantGuid = Guid.Parse(item.Parameter.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                        break;
                    }
                }
            }

            return tenantGuid;
        }

        private VssBasicCredential GetPersonalAccessTokenCredentials(string personalAccessToken)
        {
            var credentials = new VssBasicCredential("", personalAccessToken);
            return credentials;
        }

        private VssConnection GetConnection()
        {
            var credentials = GetPersonalAccessTokenCredentials(_personalAccessToken);

            // Create instance of VssConnection using passed credentials
            var connection = new VssConnection(new Uri($"https://{_organisationName}.visualstudio.com"), credentials);
            return connection;
        }

        internal async IAsyncEnumerable<TeamProjectReference> ListProjectsAsync()
        {
            // Create instance of VssConnection using passed credentials
            var projectHttpClient = Connection.GetClient<ProjectHttpClient>();
            foreach (var project in await projectHttpClient.GetProjects())
                yield return project;
        }
    }
}
