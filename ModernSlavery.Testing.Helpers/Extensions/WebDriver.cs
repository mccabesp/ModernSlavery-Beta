using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class WebDriver
    {
        public static IWebDriver CreateWebDriver(bool headless = false, bool logPerformance=false)
        {
            var chromeOptions = new ChromeOptions
            {
                AcceptInsecureCertificates = true,
                PageLoadStrategy = PageLoadStrategy.Normal,
            };
            chromeOptions.AddArgument("no-sandbox");

            if (logPerformance)
            {
                //This wont work until Selenium 4 or with 4-beta so dont use for now
                ChromePerformanceLoggingPreferences perfLogPrefs = new ChromePerformanceLoggingPreferences();
                perfLogPrefs.AddTracingCategories(new string[] { "devtools.network" });
                chromeOptions.PerformanceLoggingPreferences = perfLogPrefs;
                chromeOptions.SetLoggingPreference("performance", LogLevel.All);
                chromeOptions.AddAdditionalCapability(CapabilityType.EnableProfiling, true, true);
            }
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            var driverService = ChromeDriverService.CreateDefaultService(Directory.GetCurrentDirectory());
            driverService.HideCommandPromptWindow = headless;
            driverService.EnableVerboseLogging = true;
            var webDriver = new ChromeDriver(driverService, chromeOptions);
            return webDriver;
        }

        public static async Task<IHtmlDocument> GetHtmlDocumentAsync(this IWebDriver webDriver, bool includeHeaders=false)
        {
            var content = webDriver.PageSource;

            IDocument document = await BrowsingContext.New()
                .OpenAsync(ResponseFactory, CancellationToken.None).ConfigureAwait(false);
            return (IHtmlDocument)document;

            void ResponseFactory(VirtualResponse htmlResponse)
            {
                htmlResponse.Address(webDriver.Url);
                htmlResponse.Content(content);

                if (!includeHeaders) return;

                var headers = webDriver.GetResponseHeaders();
                if (headers.ContainsKey("status"))
                {
                    htmlResponse.Status(headers["status"].ToInt32());
                    headers.Remove("status");
                }

                foreach (var key in headers.Keys)
                    htmlResponse.Header(key, headers[key]);
            }
        }

        public static Dictionary <string, string> GetResponseHeaders(this IWebDriver webDriver)
        {
            // and capture the last recorded url (it may be a redirect, or the
            // original url)
            var currentURL = webDriver.Url;

            // then ask for all the performance logs from this request
            // one of them will contain the Network.responseReceived method
            // and we shall find the "last recorded url" response
            var logs = webDriver.Manage().Logs.GetLog("performance");
            var headers = new Dictionary<string, string>();
            foreach (var entry in logs)
            {
                dynamic json = JsonConvert.DeserializeObject(entry.Message);
                string method = json?.message?.method;
                if (method==null || !method.EqualsI("Network.responseReceived")) continue;
                var response = json?.message?.@params?.response;
                if (response == null) continue;

                string url = response?.url;
                if (!currentURL.EqualsI(url)) continue;
                headers["status"]=response.status;
                IDictionary<string, string> dictionary =response.headers.ToObject<IDictionary<string,string>>();
                headers.AddRange(dictionary);
                break;
            }
            return headers;
        }
    }
}
