using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class LoggingHelper
    {

        public static void DeleteFiles(string filepath, string filePattern="*.*")
        {
            //Delete all previous screenshots
            if (!Directory.Exists(filepath)) return;
            
            var files = Directory.GetFiles(filepath, filePattern, SearchOption.AllDirectories);
            foreach (var file in files)
                File.Delete(file);
        }

        public static void AttachFiles(string filepath, string name="File", string filePattern = "*.*")
        {
            //Delete all previous screenshots
            if (!Directory.Exists(filepath)) return;

            var files = Directory.GetFiles(filepath, filePattern, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                TestContext.AddTestAttachment(file, $"[{name.ToUpper()}]: {filename}");
                TestContext.Progress.WriteLine($"Added test attachment: {filename}");
            }            
        }

        public static void LogWarning(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            throw new NotImplementedException();
            Console.WriteLine($"Write-Host \"##vso[task.logissue type=warning;]{message}\"");
            Console.WriteLine($"Write-Host \"##[warning]{message}\"");
        }
        public static void LogError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            TestContext.Error.WriteLine(message);
        }

        public static void LogInformation(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            throw new NotImplementedException();
            Console.WriteLine($"Write-Host \"{message}\"");
        }

        public static void LogDebug(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            throw new NotImplementedException();
            Console.WriteLine($"Write-Host \"##vso[task.logissue type=error;]{message}\"");
            Console.WriteLine($"Write-Host \"##[debug]{message}\"");
        }

        public static void LogGroupStart(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            throw new NotImplementedException();
            Console.WriteLine($"Write-Host \"##[group]{message}\"");
        }

        public static void LogGroupEnd()
        {
            throw new NotImplementedException();
            Console.WriteLine($"Write-Host \"##[endgroup]\"");
        }
    }
}
