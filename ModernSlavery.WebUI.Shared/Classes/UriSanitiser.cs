using System;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public static class UriSanitiser
    {

        public static bool IsValidHttpOrHttpsLink(string uri)
        {
            bool uriIsValid = Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return uriIsValid;
        }

    }
}
