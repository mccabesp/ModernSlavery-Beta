using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
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
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace ModernSlavery.Testing.Helpers.Classes
{
    public class UITest
    {
        #region Setup and Teardown
        [OneTimeSetUp]
        public async Task SetupBrowser()
        {
            //Create the browser driver for selenium and show console if debugging
            _testWebBrowser = WebDriver.CreateWebDriver(!Debugger.IsAttached, RetrieveHeaders);
        }

        [OneTimeTearDown]
        public async Task ShowdownBrowser()
        {
            //Stop the web browser and release all resources
            _testWebBrowser?.Close();
            _testWebBrowser?.Quit();
            _testWebBrowser = null;
        }

        #endregion

        #region Private fields
        private Lazy<Task<IHtmlDocument>> _HtmlDocument = null;
        private IWebDriver _testWebBrowser;
        #endregion

        #region Protected properties
        protected IHtmlDocument HtmlDocument => _HtmlDocument.Value.Result;
        protected IConfiguration Config { get; private set; } = Extensions.Configuration.GetJsonSettings();
        private bool RetrieveHeaders => Config.GetValue("Selenium:RetrieveHeaders", false);
        #endregion

        #region Private methods
       
        private void ResetHtmlDocument()
        {
            _HtmlDocument = new Lazy<Task<IHtmlDocument>>(async () => 
            {
                return await _testWebBrowser.GetHtmlDocumentAsync(RetrieveHeaders); 
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

            //Reset the Html document ready for reloading
            ResetHtmlDocument();
        }

        #endregion

        #region HtmlElement methods
        public void Expect(string selector)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(selector)) throw new ArgumentNullException(nameof(selector));

            //Try and find by xpath than css
            var nodes = HtmlDocument.GetHtmlNodes(selector);
            if (nodes!=null && nodes.Any()) return;

            //Throw an assert failure if we cant find
            Assert.Fail($"Cannot find elements '{selector}' on page '{HtmlDocument.Url}'");
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

        public object Set(string selector, params string[] values)
        {
            throw new NotImplementedException();
        }

        
        public void ExpectContains(string selector, params string[] values)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IHtmlHeadingElement> ExpectHeader(string selector)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(selector)) throw new ArgumentNullException(nameof(selector));

            //Try and find by xpath than css
            var headers = HtmlDocument.GetHtmlNodes(selector).OfType<IHtmlHeadingElement>();

            //Throw an assert failure if we cant find
            Assert.That(headers != null && headers.Any(),$"Cannot find header '{selector}' on page '{HtmlDocument.Url}'");

            return headers;
        }

        public IEnumerable<IHtmlHeadingElement> ExpectHeaderContains(string text, StringComparison comparer = StringComparison.OrdinalIgnoreCase)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            var headings = HtmlDocument.GetHeadings().Where(n => n.TextContent.Contains(text, comparer));

            //Throw an assert failure if we cant find
            Assert.That(headings.Any(),$"Cannot find header containing '{text}' on page '{HtmlDocument.Url}'");
            
            return headings;
        }

        public object Above(string selector)
        {
            throw new NotImplementedException();
        }
        public object Below(string selector)
        {
            throw new NotImplementedException();
        }
        public object AboveHeader(string selector)
        {
            throw new NotImplementedException();
        }
        public void BelowHeader(string selector)
        {
            throw new NotImplementedException();
        }
        
        public IHtmlElement Click(string selector)
        {
            //Validate the parameters
            if (string.IsNullOrWhiteSpace(selector)) throw new ArgumentNullException(nameof(selector));

            var nodes = HtmlDocument.GetHtmlNodes(selector);

            //Throw an assert failure if too many
            var count = nodes.Count();
            Assert.That(count<2, $"Cannot click {count} elements matching '{selector}' on page '{HtmlDocument.Url}'");

            var element = nodes.SingleOrDefault() as IHtmlElement;
            Assert.IsNotNull(element, $"Cannot find element matching '{selector}' on page '{HtmlDocument.Url}'");
            
            selector = element.GetSelector();
            var webElement = _testWebBrowser.FindElement(By.CssSelector(selector));
            Assert.IsNotNull(webElement, $"Cannot find element matching '{selector}' on page '{HtmlDocument.Url}'");

            webElement.Click();

            return element;
        }

        public void Click(object bottom, string selector)
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

        public void AssumeDate(string selector)
        {
            throw new NotImplementedException();
        }

        public void AssumeTime(string selector)
        {
            throw new NotImplementedException();
        }



        public void GotoCopiedUrl(string selector)
        {
            throw new NotImplementedException();
        }

        public void ExpectNo(string selector)
        {
            throw new NotImplementedException();
        }
        public void ExpectButton(string selector)
        {
            throw new NotImplementedException();
        }

        public void ExpectLabel(string selector)
        {
            throw new NotImplementedException();
        }
        public void ExpectLabelContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ExpectField(string selector)
        {
            throw new NotImplementedException();
        }

        public void ClickLabel(string selector)
        {
            throw new NotImplementedException();
        }

        public void CopyUrl(string selector)
        {
            throw new NotImplementedException();
        }

        public void ClearField(string selector)
        {
            throw new NotImplementedException();
        }

        public object ExpectFieldContains(string selector,params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ExpectText(string selector)
        {
            throw new NotImplementedException();
        }

        public void ExpectTextContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void ClickButton(object contains, string selector)
        {
            throw new NotImplementedException();
        }

        public void ExpectRow(string selector)
        {
            throw new NotImplementedException();
        }
        public void ExpectRowContains(string v, params string[] values)
        {
            throw new NotImplementedException();
        }

        public void TakeScreenshot(string selector)
        {
            throw new NotImplementedException();
        }

        public void ClickButton(string selector)
        {
            throw new NotImplementedException();
        }

        public object AtRow(string selector)
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

        private object AtLabel(string selector)
        {
            throw new NotImplementedException();
        }

        private object BelowLabel(string selector)
        {
            throw new NotImplementedException();
        }
    }
}
