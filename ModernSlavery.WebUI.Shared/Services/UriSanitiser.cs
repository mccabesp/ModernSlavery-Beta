using System;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public static class UriSanitiser
    {
        public static bool IsValidHttpOrHttpsLink(string uri)
        {
            var uriIsValid = Uri.TryCreate(uri, UriKind.Absolute, out var uriResult)
                             && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return uriIsValid;
        }
    }
}