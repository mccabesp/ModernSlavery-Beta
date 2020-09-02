using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class DevOpsAgent
    {
        public static void LogWarning(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            Console.WriteLine($"Write-Host \"##vso[task.logissue type=warning;]{message}\"");
            Console.WriteLine($"Write-Host \"##[warning]{message}\"");
        }
        public static void LogError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            Console.WriteLine($"Write-Host \"##vso[task.logissue type=error;]{message}\"");
            Console.WriteLine($"Write-Host \"##[error]{message}\"");
        }

        public static void LogInformation(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            Console.WriteLine($"Write-Host \"{message}\"");
        }

        public static void LogDebug(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            Console.WriteLine($"Write-Host \"##vso[task.logissue type=error;]{message}\"");
            Console.WriteLine($"Write-Host \"##[debug]{message}\"");
        }

        public static void LogGroupStart(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(message);
            Console.WriteLine($"Write-Host \"##[group]{message}\"");
        }

        public static void LogGroupEnd()
        {
            Console.WriteLine($"Write-Host \"##[endgroup]\"");
        }

    }
}
