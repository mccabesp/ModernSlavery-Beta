﻿using System;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Shared.Classes.Cookies
{
    public class CookieHelper
    {
        public const string CookieSettingsCookieName = "cookie_settings";
        public const string SeenCookieMessageCookieName = "seen_cookie_message";
        public const int CurrentCookieMessageVersion = 1;

        #region Cookie Settings

        public static CookieSettings GetCookieSettingsCookie(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(CookieSettingsCookieName, out var cookieSettingsString))
                try
                {
                    return JsonConvert.DeserializeObject<CookieSettings>(cookieSettingsString);
                }
                catch (JsonException)
                {
                    /* If we can't deserialize the JSON, just return false for everything */
                }

            return new CookieSettings
            {
                GoogleAnalyticsMSU = false, GoogleAnalyticsGovUk = false, ApplicationInsights = false,
                RememberSettings = false
            };
        }

        public static void SetCookieSettingsCookie(HttpResponse response, CookieSettings cookieSettings)
        {
            var cookieSettingsString = Json.SerializeObject(cookieSettings);

            response.Cookies.Append(
                CookieSettingsCookieName,
                cookieSettingsString,
                new CookieOptions {Secure = true, SameSite = SameSiteMode.Strict, MaxAge = TimeSpan.FromDays(365)});
        }

        #endregion

        #region Seen Cookie Message

        public static bool HasSeenLatestCookieMessage(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(SeenCookieMessageCookieName, out var seenCookieMessageString))
                try
                {
                    var seenCookieMessage = JsonConvert.DeserializeObject<SeenCookieMessage>(seenCookieMessageString);
                    var latestVersionOfCookieMessageTheUserHasSeen = seenCookieMessage.Version;
                    var hasSeenLatestCookieMessage =
                        latestVersionOfCookieMessageTheUserHasSeen >= CurrentCookieMessageVersion;
                    return hasSeenLatestCookieMessage;
                }
                catch (JsonException)
                {
                    /* If we can't deserialize the JSON, assume they haven't seen the message (i.e. return false) */
                }

            return false;
        }

        public static void SetSeenCookieMessageCookie(HttpResponse response)
        {
            var seenCookieMessage = new SeenCookieMessage {Version = 1};

            string seenCookieMessageString = Json.SerializeObject(seenCookieMessage);

            response.Cookies.Append(
                SeenCookieMessageCookieName,
                seenCookieMessageString,
                new CookieOptions {Secure = true, SameSite = SameSiteMode.Strict, MaxAge = TimeSpan.FromDays(365)});
        }

        #endregion
    }
}