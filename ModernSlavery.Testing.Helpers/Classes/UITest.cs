using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.EntityFrameworkCore.Internal;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Classes
{
    public class UITest
    {
        #region Private fields
        private IWebDriver _testWebBrowser;
        private Process _seleniumProcess;
        #endregion

        #region Protected properties
        private Lazy<Task<IHtmlDocument>> _HtmlDocument = null;
        protected IHtmlDocument HtmlDocument => _HtmlDocument.Value.Result;
        
        #endregion

        #region Setup and Teardown
        [OneTimeSetUp]
        public async Task SetupBrowser()
        {
            //Start the selenium browser process
            _seleniumProcess = WebDriver.StartSelenium();

            //Create the browser driver for selenium
            _testWebBrowser = WebDriver.CreateWebDriver();
        }

        [OneTimeTearDown]
        public async Task ShowdownBrowser()
        {
            //Stop the web browser and release all resources
            _testWebBrowser?.Close();
            _testWebBrowser?.Quit();
            _testWebBrowser = null;
            _seleniumProcess?.Close();
            _seleniumProcess?.Dispose();
            _seleniumProcess = null;
        }
        #endregion

        #region Private methods

        private void ResetHtmlDocument()
        {
            _HtmlDocument = new Lazy<Task<IHtmlDocument>>(async () => 
            {
                return await _testWebBrowser.GetHtmlDocumentAsync(); 
            });
        }
        #endregion

        #region Protected methods
        #region Browser methods
        public void Goto(string url)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));
            if (!url.IsUrl()) throw new ArgumentException($"'{url}'  is not a valid url",nameof(url));

            //Navigate to the new url
            _testWebBrowser.Url = url;
            _testWebBrowser.Navigate();

            //Reset the Html document ready for reloading
            ResetHtmlDocument();
        }

        #endregion

        #region HtmlElement methods
        public void Expect(string locator)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(locator)) throw new ArgumentNullException(nameof(locator));

            //Try and find by xpath than css
            var nodes = HtmlDocument.GetHtmlNodes(locator);
            if (nodes!=null && nodes.Any()) return;

            //Throw an assert failure if we cant find
            Assert.Fail($"Cannot find elements '{locator}' on page '{HtmlDocument.Url}'");
        }

        public void ExpectContains(string text)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            //try and find by text content
            var nodes = HtmlDocument.GetHtmlNodes($"//*[contains(text(),'{text}')]");
            if (nodes != null && nodes.Any()) return;

            //Throw an assert failure if we cant find
            Assert.Fail($"Cannot find elements containing '{text}' on page '{HtmlDocument.Url}'");
        }

        #endregion

        #region HtmlLabel method
        #endregion

        #region HtmlAnchor method
        #endregion

        #region HtmlHeader method
        #endregion

        #endregion

        public object Set(string locator, params string[] values)
        {
            throw new NotImplementedException();
        }

        
        public void ExpectContains(string locator, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ExpectHeader(string locator)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(locator)) throw new ArgumentNullException(nameof(locator));

            //Try and find by xpath than css
            var nodes = HtmlDocument.GetHtmlNodes(locator).OfType<IHtmlHeadingElement>();
            if (nodes != null && nodes.Any()) return;

            //Throw an assert failure if we cant find
            Assert.Fail($"Cannot find header '{locator}' on page '{HtmlDocument.Url}'");
        }
        public void ExpectHeaderContains(string text, StringComparison comparer = StringComparison.OrdinalIgnoreCase)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            var nodes = HtmlDocument.GetHeadings();
            if (nodes.Any(n => n.TextContent.Contains(text, comparer))) return;
            
            //Throw an assert failure if we cant find
            Assert.Fail($"Cannot find header containing '{text}' on page '{HtmlDocument.Url}'");
        }

        public object Above(string v)
        {
            throw new NotImplementedException();
        }
        public object Below(string v)
        {
            throw new NotImplementedException();
        }
        public object AboveHeader(string v)
        {
            throw new NotImplementedException();
        }
        public void BelowHeader(string v)
        {
            throw new NotImplementedException();
        }
        public void Click(string locator)
        {
            throw new NotImplementedException();
        }
        public void Click(object bottom, string v)
        {
            throw new NotImplementedException();
        }

        public void LoginAs<T>()
        {
            throw new NotImplementedException();
        }

        public void Run<T>()
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void AssumeDate(string v)
        {
            throw new NotImplementedException();
        }

        public void AssumeTime(string v)
        {
            throw new NotImplementedException();
        }



        public void GotoCopiedUrl(string v)
        {
            throw new NotImplementedException();
        }

        public void ExpectNo(string v)
        {
            throw new NotImplementedException();
        }
        public void ExpectButton(string v)
        {
            throw new NotImplementedException();
        }

        public void ExpectLabel(string v)
        {
            throw new NotImplementedException();
        }
        public void ExpectLabelContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ExpectField(string v)
        {
            throw new NotImplementedException();
        }

        public void ClickLabel(string v)
        {
            throw new NotImplementedException();
        }

        public void CopyUrl(string v)
        {
            throw new NotImplementedException();
        }

        public void ClearField(string v)
        {
            throw new NotImplementedException();
        }

        public object ExpectFieldContains(string locator,params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ExpectText(string v)
        {
            throw new NotImplementedException();
        }

        public void ExpectTextContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ClickButton(object contains, string v)
        {
            throw new NotImplementedException();
        }

        public void ExpectRow(string v)
        {
            throw new NotImplementedException();
        }
        public void ExpectRowContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void TakeScreenshot(string v)
        {
            throw new NotImplementedException();
        }

        public void ClickButton(string v)
        {
            throw new NotImplementedException();
        }

        public object AtRow(string v)
        {
            throw new NotImplementedException();
        }

        public object AtCell(string row, string column)
        {
            throw new NotImplementedException();
        }

        public object AtCellExpect(string row, string column, params string[] values)
        {
            throw new NotImplementedException();
        }

        public object AtCellExpectContains(string row, string column, params string[] values)
        {
            throw new NotImplementedException();
        }

        private object AtLabel(string v)
        {
            throw new NotImplementedException();
        }

        private object BelowLabel(string v)
        {
            throw new NotImplementedException();
        }
    }
}
