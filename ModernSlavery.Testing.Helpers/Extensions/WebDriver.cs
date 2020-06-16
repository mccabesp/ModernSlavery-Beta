using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class WebDriver
    {
        public static Process StartSelenium()
        {
            return null;
            var _selenium = new Process
            {
                StartInfo = new ProcessStartInfo { FileName = "selenium-standalone", Arguments = "start", UseShellExecute = true }
            };
            _selenium.Start();

            return _selenium;
        }

        public static IWebDriver CreateWebDriver(bool hideCommandPromptWindow=false)
        {
            string driverPath = Directory.GetCurrentDirectory();

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = hideCommandPromptWindow;
            
            var options = new ChromeOptions();
            if (!Debugger.IsAttached)
            {
                options.AddArgument("--headless"); //Optional, comment this out if you want to SEE the browser window
            }

            options.AcceptInsecureCertificates = true;
            options.SetLoggingPreference(LogType.Browser, LogLevel.All);
            var webDriver = new ChromeDriver(driverService, options);
            return webDriver;
        }

        public static async Task<IHtmlDocument> GetHtmlDocumentAsync(this IWebDriver webDriver, bool includeHeaders=false)
        {
            var content = webDriver.PageSource;

            IDocument document = await BrowsingContext.New()
                .OpenAsync(ResponseFactory, CancellationToken.None);
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

                if (!json.method.EqualsI("Network.responseReceived") || !json.method.@params.response.url.EqualsI(currentURL)) continue;
                    
                headers["status"]=json.method.@params.response.status;

                foreach (var header in json.method.@params.response.headers)
                    headers[header.key]=header.value;
            }
            return headers;
        }
    }
}
