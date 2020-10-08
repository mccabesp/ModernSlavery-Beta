using ModernSlavery.Testing.Helpers.Classes;
using System;
using System.Collections.Generic;
using Selenium.Axe;
using System.IO;
using ModernSlavery.Core.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AxeHelper
    {
        private static readonly List<string> Tags = new List<string>() { "wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag2a", "best-practice" };
        private static readonly HashSet<ResultType> ResultTypes = new HashSet<ResultType> { ResultType.Violations, ResultType.Incomplete };
        private static readonly string[] BadImpacts = new[]  { "Moderate", "Serious", "Critical" };

        /// <summary>
        /// Runs an accessubility check on the last loaded web page and writes a report to a HTML file
        /// Throws Assert.Fail if any severe violations or Asser.Warning for non-severe violations
        /// </summary>
        /// <param name="uiTest">The calling UITest class</param>
        /// <param name="filePath">The filename to store the report as.
        /// This can be an absolute path or path relative to the AxeResults output folder.
        /// Any subfolders are automatically created if they dont already exist.
        /// File extensions are ignored and replaced with '.html'
        /// If empty a filename is generated from the test name and UrlPath (eg: the URl '\viewing\search' will be saved to file ".\AxeResults\WebTestHost_SeleniumHelper_TestMethods_OK\viewing\search_GET.html",
        /// </param>
        /// <param name="httpMethod">The Http method so save against the filename. Defaults to 'GET' when empty.
        /// This is ignored if the 'filePath' parameter is provided. Set to "POST" after a postback
        /// </param>
        /// <returns></returns>
        public static async Task CheckAccessibilityAsync(this BaseUITest uiTest, string filePath=null,string httpMethod="GET")
        {
            if (uiTest==null) throw new ArgumentNullException(nameof(uiTest));

            var uri = new Uri(uiTest.WebDriver.Url);
            if (string.IsNullOrWhiteSpace(filePath)) {
                if (string.IsNullOrWhiteSpace(uiTest.WebDriver.Url)) throw new ArgumentNullException(nameof(filePath));
                filePath = $"/{TestContext.CurrentContext.Test.Name}{uri.LocalPath}_{httpMethod.ToUpper()}";
            }

            filePath = ExpandResultsFilePath(filePath);

            LoggingHelper.EnsureFilePathExists(filePath);

            if (File.Exists(filePath)) throw new Exception($"File: {filePath} already exists");

            var options = new AxeRunOptions() 
                { 
                     ResultTypes = ResultTypes,
                     RunOnly= new RunOnlyOptions{Type="tag",Values= Tags}
                };
            
            var axeResult = new AxeBuilder(uiTest.WebDriver)
                .WithOptions(options)
                .Analyze();

            uiTest.WebDriver.CreateAxeHtmlReport(axeResult, filePath);

            filePath = filePath.Substring(LoggingHelper.AxeResultsFilepath.Length);

            //Throw errors when violations are too severe
            var violations = axeResult.Violations.Where(v => v.Impact.EqualsI(BadImpacts) && Tags.ContainsI(v.Tags)).Sum(v=>v.Nodes.Count())+axeResult.Incomplete.Where(v => v.Impact.EqualsI(BadImpacts)).Sum(v=>v.Nodes.Count());

            if (violations > 0)
                Assert.Fail($"Accessibility: {violations} serious violation{(violations == 1 ? "" : "s")} on {uri.PathAndQuery} (see {filePath})");            
            else 
            {
                violations = axeResult.Violations.Where(v => Tags.ContainsI(v.Tags)).Sum(v => v.Nodes.Count()) + axeResult.Incomplete.Sum(v => v.Nodes.Count());
                if (violations > 0)Assert.Warn($"Accessibility: {violations} violation{(violations==1 ? "" : "s")} on {uri.PathAndQuery}  (see {filePath})");
            }
        }

        private static string ExpandResultsFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!FileSystem.IsValidFilePath(filePath)) throw new ArgumentException($"Invalid characters in filepath", nameof(filePath));
            
            //Ensure the extension is html
            var extension = Path.GetExtension(filePath);
            if (!extension.EqualsI(".html"))filePath = filePath.Substring(0, filePath.Length - extension.Length) + ".html";

            //Ensure the path is absolute and relative to Axe log resutls path
            return FileSystem.ExpandLocalPath(filePath, LoggingHelper.AxeResultsFilepath);
        }
    }
}
