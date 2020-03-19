using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    public class BasicAuthenticationMiddleware
    {
        private const string SessionAuthenticatedKey= "BasicAuthentication:Authenticated";
        private const string HttpAuthorizationHeader = "Authorization";  // HTTP1.1 Authorization header 
        private const string HttpBasicSchemeName = "Basic"; // HTTP1.1 Basic Challenge Scheme Name 
        private const char HttpCredentialSeparator = ':'; // HTTP1.1 Credential username and password separator 
        private const string HttpWWWAuthenticateHeader = "WWW-Authenticate"; // HTTP1.1 Basic Challenge Scheme Name 
        private static string _Realm = null; // HTTP.1.1 Basic Challenge Realm 
        private string _Username; 
        private string _Password;

        private readonly RequestDelegate _next;

        public BasicAuthenticationMiddleware(RequestDelegate next, string username = null, string password = null, string realm=null)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            _Username = username;
            _Password = password;
            if (!string.IsNullOrWhiteSpace(realm)) _Realm = realm;
            _next = next;
        }

        public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var authenticated = GetIsAuthenticated(httpContext);
            if (!authenticated) 
            {
                if (httpContext.Request.Headers.ContainsKey(HttpAuthorizationHeader))
                {

                    var authorizationHeader = httpContext.Request.Headers[HttpAuthorizationHeader];
                    httpContext.Request.Headers.Remove(HttpAuthorizationHeader);
                    try
                    {
                        // 
                        //  Extract the basic authentication credentials from the request 
                        // 
                        var credentials = ExtractBasicCredentials(authorizationHeader);

                        // 
                        // Validate the user credentials 
                        // 
                        if (ValidateCredentials(credentials.Username, credentials.Password))
                        {
                            SetIsAuthenticated(httpContext, true);
                            authenticated = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                } 
            }

            if (authenticated)
                await _next.Invoke(httpContext);
            else
                IssueAuthenticationChallenge(httpContext);
        }

        protected virtual bool GetIsAuthenticated(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var authenticated=httpContext.Session.GetString(SessionAuthenticatedKey).ToBoolean();
            return authenticated;
        }

        protected virtual void SetIsAuthenticated(Microsoft.AspNetCore.Http.HttpContext httpContext, bool authenticated)
        {
            if (authenticated)
                httpContext.Session.SetString(SessionAuthenticatedKey,"true");
            else
                httpContext.Session.Remove(SessionAuthenticatedKey);
        }

        protected virtual bool ValidateCredentials(string userName, string password)
        {
            return userName.Equals(_Username, StringComparison.InvariantCultureIgnoreCase) && password.Equals(_Password);
        }

        protected virtual (string Username, string Password) ExtractBasicCredentials(string authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))throw new ArgumentNullException(nameof(authorizationHeader));

            var verifiedAuthorizationHeader = authorizationHeader.Trim();
            if (verifiedAuthorizationHeader.IndexOf(HttpBasicSchemeName) != 0) throw new ArgumentException($"Authentication scheme is not '{HttpBasicSchemeName}'", nameof(authorizationHeader));

            // get the credential payload 
            verifiedAuthorizationHeader = verifiedAuthorizationHeader.Substring(HttpBasicSchemeName.Length, verifiedAuthorizationHeader.Length - HttpBasicSchemeName.Length).Trim();

            // decode the base 64 encoded credential payload 
            var credentialBase64DecodedArray = Convert.FromBase64String(verifiedAuthorizationHeader);
            var decodedAuthorizationHeader = Encoding.UTF8.GetString(credentialBase64DecodedArray, 0, credentialBase64DecodedArray.Length);

            // get the username, password, and realm 
            var separatorPosition = decodedAuthorizationHeader.IndexOf(HttpCredentialSeparator);

            if (separatorPosition < 0)throw new ArgumentException($"Missing credential separator '{HttpCredentialSeparator}'", nameof(authorizationHeader));

            var username = decodedAuthorizationHeader.Substring(0, separatorPosition).Trim();
            var password = decodedAuthorizationHeader.Substring(separatorPosition + 1, (decodedAuthorizationHeader.Length - separatorPosition - 1)).Trim();

            return (username,password);
        }

        protected virtual void IssueAuthenticationChallenge(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            // 
            // Issue a basic challenge
            // 
            httpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            httpContext.SetResponseHeader(HttpWWWAuthenticateHeader, $"Basic {(string.IsNullOrWhiteSpace(_Realm) ? "" : $"\"{_Realm}\"")}");
        }

    }
}
