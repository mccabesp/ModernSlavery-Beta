using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes
{
    /// <summary>
    /// Checks for specific Xss markers
    /// </summary>
    public class XssValidator
    {
        private readonly ILogger _logger;
        public XssValidator(ILogger<XssValidator> logger)
        {
            _logger = logger;
        }

        private static readonly char[] _startingChars = { '<', '&' };

        public IEnumerable<(int position, string badChars)> Validate(IEnumerable<string> strings)
        {
            foreach (var s in strings)
                yield return Validate(s);
        }

        public (int position, string badChars) Validate(string s)
        {
            for (var i = 0; ;)
            {

                // Look for the start of one of our patterns 
                var n = s.IndexOfAny(_startingChars, i);

                // If not found, the string is safe
                if (n < 0) return default;

                // If it's the last char, it's safe 
                if (n == s.Length - 1) return default;

                var position = n;
                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1])) return (position,"<");
                        if (s[n + 1] == '!') return (position, "<!");
                        if (s[n + 1] == '/') return (position, "</");
                        if (s[n + 1] == '?') return (position, "<?");
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. S) 
                        if (s[n + 1] == '#') return (position, "&#");
                        break;

                }

                // Continue searching
                i = n + 1;
            }
        }

        private bool IsAtoZ(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public void LogViolation(HttpContext httpContext, string propertyName, string badChars, int position, string text)
        {
            dynamic error = new
            {
                Message = $"({WebUtility.HtmlEncode(badChars)}) detected: Possible Xss attack",
                SourceIP = httpContext?.GetUserHostAddress(),
                Identity = GetIdentity(httpContext?.User),
                Target=httpContext.Request.Path,
                Source = $"{propertyName} [{position}]: {WebUtility.HtmlEncode(text)}"
            };

            //Log the error
            string json = Json.SerializeObject(error);
            _logger.LogWarning(json);
        }

        private string GetIdentity(ClaimsPrincipal user)
        {
            if (user == null) return "[Anonymous]";
            var sub = user.GetClaim("sub");
            var name = user.GetClaim("name");
            if (string.IsNullOrWhiteSpace(name))
                return sub;
            if (string.IsNullOrWhiteSpace(sub))
                return name;
            return $"{sub}: {name}";
        }

    }
}
