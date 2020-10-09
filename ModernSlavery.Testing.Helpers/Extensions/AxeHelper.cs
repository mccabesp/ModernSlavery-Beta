using ModernSlavery.Testing.Helpers.Classes;
using System;
using System.Collections.Generic;
using Selenium.Axe;
using System.IO;
using ModernSlavery.Core.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using System.Data;
using Microsoft.WindowsAzure.Storage.File;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AxeHelper
    {
        private static readonly List<string> Tags = new List<string>() { "wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag2a", "best-practice" };
        private static readonly HashSet<ResultType> ResultTypes = new HashSet<ResultType> { ResultType.Violations, ResultType.Incomplete };
        private static readonly string[] BadImpacts = new[]  { "Moderate", "Serious", "Critical" };
        private static bool ShowFailuresAsWarnings = true;

        private static Dictionary<string, AxeResult> PagesAnalysed = new Dictionary<string, AxeResult>(StringComparer.OrdinalIgnoreCase);

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

            //Check if we have already analysed this page
            var pageDescriptor = $"{uri.LocalPath}_{httpMethod.ToUpper()}";
            if (PagesAnalysed.ContainsKey(pageDescriptor)) return;

            if (string.IsNullOrWhiteSpace(filePath)) {
                if (string.IsNullOrWhiteSpace(uiTest.WebDriver.Url)) throw new ArgumentNullException(nameof(filePath));
                filePath = $"{pageDescriptor}";
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

            //Save the results
            axeResult.TestEnvironment.UserAgent = filePath; //Use UserAgent to store ReportPath
            PagesAnalysed[pageDescriptor] = axeResult;

            //Throw errors when violations are too severe
            var violations = axeResult.Violations.Where(v => v.Impact.EqualsI(BadImpacts) && Tags.ContainsI(v.Tags)).Sum(v=>v.Nodes.Count())+axeResult.Incomplete.Where(v => v.Impact.EqualsI(BadImpacts)).Sum(v=>v.Nodes.Count());

            if (violations > 0)
            {
                if (ShowFailuresAsWarnings)
                    Assert.Warn($"Accessibility: {violations} serious violation{(violations == 1 ? "" : "s")} on {uri.PathAndQuery} (see {filePath})");
                else
                    Assert.Fail($"Accessibility: {violations} serious violation{(violations == 1 ? "" : "s")} on {uri.PathAndQuery} (see {filePath})");
            }
            else
            {
                violations = axeResult.Violations.Where(v => Tags.ContainsI(v.Tags)).Sum(v => v.Nodes.Count()) + axeResult.Incomplete.Sum(v => v.Nodes.Count());
                if (violations > 0) Assert.Warn($"Accessibility: {violations} violation{(violations == 1 ? "" : "s")} on {uri.PathAndQuery}  (see {filePath})");
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

        public static void SaveResultSummary()
        {
            if (!PagesAnalysed.Any()) return;

            var records = new List<dynamic>();
            foreach (var pageDescriptor in PagesAnalysed.Keys)
            {
                var results = PagesAnalysed[pageDescriptor];
                foreach (var violation in results.Violations.Where(v => v.Impact.EqualsI(BadImpacts) && Tags.ContainsI(v.Tags)))
                {
                    var record = new
                    {
                        Page = pageDescriptor.BeforeLast("_"),
                        Method = pageDescriptor.AfterLast("_", includeWhenNoSeparator: false),
                        results.Url,
                        Type = "Violation",
                        violation.Impact,
                        violation.Description,
                        Incidents= violation.Nodes.Count(),
                        Tags= violation.Tags.ToDelimitedString(),
                        ReportPath = results.TestEnvironment.UserAgent,
                        Timestamp = results.Timestamp==null ? null : results.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss")
                    };
                    records.Add(record);
                }
                foreach (var violation in results.Incomplete.Where(v => v.Impact.EqualsI(BadImpacts) && Tags.ContainsI(v.Tags)))
                {
                    var record = new
                    {
                        Page = pageDescriptor.BeforeLast("_"),
                        Method = pageDescriptor.AfterLast("_", includeWhenNoSeparator: false),
                        results.Url,
                        Type = "Incomplete",
                        violation.Impact,
                        violation.Description,
                        Incidents = violation.Nodes.Count(),
                        Tags = violation.Tags.ToDelimitedString(),
                        ReportPath = results.TestEnvironment.UserAgent,
                        Timestamp = results.Timestamp == null ? null : results.Timestamp.Value.ToString("dd/MM/yyyy HH:mm:ss")
                    };
                    records.Add(record);
                }
            }

            if (records == null || !records.Any()) return;

            var filePath = Path.Combine(LoggingHelper.AxeResultsFilepath, $"AxeResultSummary({DateTime.Now:yy-MM-dd}).csv");
            if (File.Exists(filePath)) File.Delete(filePath);

            var table = records.ToDataTable();

            using (var textWriter = new StringWriter())
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                config.ShouldQuote = (field, context) => true;
                config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;

                using (var writer = new CsvWriter(textWriter, config))
                {
                    for (var c = 0; c < table.Columns.Count; c++) writer.WriteField(table.Columns[c].ColumnName);
                    writer.NextRecord();

                    foreach (DataRow row in table.Rows)
                    {
                        for (var c = 0; c < table.Columns.Count; c++) writer.WriteField(row[c].ToString());
                        writer.NextRecord();
                    }
                }

                var appendString = textWriter.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(appendString))
                    File.WriteAllText(filePath, appendString);
            }
        }
    }
}
