using ModernSlavery.Testing.Helpers.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using Selenium.Axe;
using System.IO;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AxeHelper
    {
        public static void CheckAccessibility(this BaseUITest uiTest, string filePath=null)
        {
            if (uiTest==null) throw new ArgumentNullException(nameof(uiTest));
            filePath = ExpandResultsFilePath(filePath);
            if (File.Exists(filePath)) throw new Exception($"File: {filePath} already exists");

            var axeResults = new AxeBuilder(uiTest.WebDriver).Analyze();

            uiTest.WebDriver.CreateAxeHtmlReport(axeResults, filePath);
        }

        private static string ExpandResultsFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            //Ensure the extension is html
            var extension = Path.GetExtension(filePath);
            if (!extension.EqualsI(".html"))filePath = filePath.Substring(0, filePath.Length - extension.Length) + ".html";

            //Exsure path if full
            return Path.IsPathRooted(filePath) ? filePath:  Path.Combine(filePath, LoggingHelper.AxeResultsFilepath);
        }
    }
}
