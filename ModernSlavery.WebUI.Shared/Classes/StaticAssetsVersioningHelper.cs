using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public interface IStaticAssetsVersioningHelper
    {
        string GetAppCssFilename();
        string GetAppIe8CssFilename();
        string GetAppJsFilename();
    }

    public class StaticAssetsVersioningHelper : IStaticAssetsVersioningHelper
    {
        public StaticAssetsVersioningHelper(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }

        private readonly SharedOptions _sharedOptions;
        private const string PathFromExecutableToWwwRoot = "wwwroot";
        private const string CompiledDirectory = "compiled";

        private const string AppCssRegex = "app-[^-]*.css";
        private const string AppIe8CssRegex = "app-ie8-[^-]*.css";
        private const string AppJsRegex = "app-.*.js";

        private Dictionary<string, string> cachedFilenames = new Dictionary<string, string>();

        public string GetAppCssFilename() => GetStaticFile(CompiledDirectory, AppCssRegex);
        public string GetAppIe8CssFilename() => GetStaticFile(CompiledDirectory, AppIe8CssRegex);
        public string GetAppJsFilename() => GetStaticFile(CompiledDirectory, AppJsRegex);

        private string GetStaticFile(string directory, string fileRegex)
        {
            if (_sharedOptions.IsLocal())
            {
                // When developing locally, skip the cache
                return FindMatchingFile(directory, fileRegex);
            }
            else
            {
                // In all other environments (Dev, Test, Pre-Prod, Prod)
                // cache the filename so we don't need to search a directory for each request
                string cacheKey = directory + "/" + fileRegex;

                if (!cachedFilenames.ContainsKey(cacheKey))
                {
                    cachedFilenames[cacheKey] = FindMatchingFile(directory, fileRegex);
                }

                return cachedFilenames[cacheKey];
            }
        }

        private string FindMatchingFile(string directory, string fileRegex)
        {
            string executablePath = Assembly.GetEntryAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executablePath);
            string pathToFiles = Path.Combine(executableDirectory, PathFromExecutableToWwwRoot, directory);

            string[] allFilePaths = Directory.GetFiles(pathToFiles);
            List<string> allFileNames = allFilePaths.Select(filePath => Path.GetFileName(filePath)).ToList();
            List<string> matchingFiles = allFileNames.Where(file => Regex.Match(file, fileRegex).Success).ToList();

            if (matchingFiles.Count == 0)
            {
                throw new Exception(
                    $"Cannot find the static asset you requested: "
                    + $"directory[{directory}] fileRegex[{fileRegex}]");
            }
            else if (matchingFiles.Count > 1)
            {
                throw new Exception(
                    $"We found more than 1 matching static assets: "
                    + $"directory[{directory}] fileRegex[{fileRegex}] found[{matchingFiles.Count}]");
            }
            else
            {
                return "/" + directory + "/" + matchingFiles[0];
            }
        }
    }
}
