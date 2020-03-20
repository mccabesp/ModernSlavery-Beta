using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace ModernSlavery.Extensions
{
    public static partial class Extensions
    {
        public static void DisableResponseCache(this HttpContext context)
        {
            SetResponseCache(context, 0);
        }

        public static void SetResponseCache(this HttpContext context, int maxSeconds)
        {
            if (maxSeconds > 0)
            {
                context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={maxSeconds}";
            }
            else
            {
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue {
                    NoCache = true, NoStore = true, MaxAge = new TimeSpan(0), MustRevalidate = true
                };
            }
        }

        /// <summary>
        ///     Removes null header or ensures header is set to correct value
        ///     ///
        /// </summary>
        /// <param name="context">The HttpContext to remove the header from</param>
        /// <param name="key">The key of the header name</param>
        /// <param name="value">The value which the header should be - if empty removed the header</param>
        public static void SetResponseHeader(this HttpContext context, string key, string value = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (context.Response.Headers.ContainsKey(key))
                    {
                        context.Response.Headers.Remove(key);
                    }
                }
                else if (!context.Response.Headers.ContainsKey(key))
                {
                    context.Response.Headers.Add(key, value);
                }
                else if (context.Response.Headers[key] != value)
                {
                    context.Response.Headers.Remove(key); //This is required as cannot change a key once added
                    context.Response.Headers[key] = value;
                }
            }
            catch (Exception ex)
            {
                if (context.Response.Headers.ContainsKey(key))
                {
                    throw new Exception($"Could not set header '{key}' from value '{context.Response.Headers[key]}' to '{value}' ", ex);
                }

                throw new Exception($"Could not add header '{key}' to value '{value}' ", ex);
            }
        }

        public static bool IsValidField(this ModelStateDictionary modelState, string key)
        {
            return modelState[key] == null || modelState[key].ValidationState == ModelValidationState.Valid;
        }

        public static string GetParams(this HttpContext context, string key)
        {
            StringValues? param = context.Request?.Query[key];
            if (string.IsNullOrWhiteSpace(param))
            {
                param = context.Request?.Form[key];
            }

            return param;
        }

        public static string GetBrowser(this HttpContext context)
        {
            return context.Request?.Headers["User-Agent"].ToStringOrNull();
        }

        public static Uri GetUri(this HttpContext context)
        {
            string host = context.Request.Scheme.EqualsI("https") && context.Request.Host.Port == 443
                          || context.Request.Scheme.EqualsI("http") && context.Request.Host.Port == 80
                ? context.Request.Host.Host
                : context.Request.Host.ToString();
            string uri = $"{context.Request.Scheme}://{host}";
            string path = context.Request.Path.ToString().TrimI("/\\ ");
            if (!string.IsNullOrWhiteSpace(path))
            {
                uri += $"/{path}";
            }

            string querystring = context.Request.QueryString.ToString().TrimI("? ");
            if (!string.IsNullOrWhiteSpace(querystring))
            {
                uri += $"?{querystring}";
            }

            return new Uri(uri);
        }

        public static Uri GetUrlReferrer(this HttpContext context)
        {
            string url = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                return new Uri(url);
            }
            catch (UriFormatException ufe)
            {
                throw new UriFormatException($"Cannot create uri from '{url}'", ufe);
            }
        }

        public static bool GetIsExternalUrl(this HttpContext context, string href)
        {
            if (string.IsNullOrWhiteSpace(href) || href.IsRelativeUri())
            {
                return false;
            }

            if (!Uri.IsWellFormedUriString(href, UriKind.Absolute))
            {
                throw new ArgumentException($"Url '{href}' is not well formed", nameof(href));
            }

            var uri = new Uri(href);
            return !uri.Host.EqualsI(context.Request.Host.Host, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetUserHostAddress(this HttpContext context)
        {
            return context.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        }

        #region Cookies

        public static string GetRequestCookieValue(this HttpContext context, string key)
        {
            return context.Request.Cookies.ContainsKey(key) ? context.Request.Cookies[key] : null;
        }

        public static void SetResponseCookie(this HttpContext context,
            string key,
            string value,
            DateTime expires,
            string subdomain = null,
            string path = "/",
            bool httpOnly = false,
            bool secure = false)
        {
            var cookieOptions = new CookieOptions {
                Expires = expires,
                SameSite= Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                Domain = subdomain,
                Path = path,
                Secure = secure,
                HttpOnly = httpOnly
            };
            if (string.IsNullOrWhiteSpace(value))
            {
                context.Response.Cookies.Delete(key);
            }
            else
            {
                context.Response.Cookies.Append(key, value, cookieOptions);
            }
        }

        #endregion

    }
}
