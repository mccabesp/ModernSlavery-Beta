﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using ModernSlavery.Core.Models;

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
        private const string CompiledDirectory = "compiled";

        private const string AppCssRegex = "app-[^-]*.css";
        private const string AppIe8CssRegex = "app-ie8-[^-]*.css";
        private const string AppJsRegex = "app-.*.js";

        private readonly SharedOptions _sharedOptions;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Dictionary<string, string> cachedFilenames = new Dictionary<string, string>();

        public StaticAssetsVersioningHelper(SharedOptions sharedOptions, IWebHostEnvironment webHostEnvironment)
        {
            _sharedOptions = sharedOptions;
            _webHostEnvironment= webHostEnvironment;
        }

        public string GetAppCssFilename()
        {
            return GetStaticFile(CompiledDirectory, AppCssRegex);
        }

        public string GetAppIe8CssFilename()
        {
            return GetStaticFile(CompiledDirectory, AppIe8CssRegex);
        }

        public string GetAppJsFilename()
        {
            return GetStaticFile(CompiledDirectory, AppJsRegex);
        }

        private string GetStaticFile(string directory, string fileRegex)
        {
            if (_sharedOptions.IsDevelopment())
                // When developing locally, skip the cache
                return FindMatchingFile(directory, fileRegex);

            // In all other environments (Dev, Test, Pre-Prod, Prod)
            // cache the filename so we don't need to search a directory for each request
            var cacheKey = directory + "/" + fileRegex;

            if (!cachedFilenames.ContainsKey(cacheKey))
                cachedFilenames[cacheKey] = FindMatchingFile(directory, fileRegex);

            return cachedFilenames[cacheKey];
        }

        private string FindMatchingFile(string directory, string fileRegex)
        {
            var pathToFiles = Path.Combine(_webHostEnvironment.WebRootPath, directory);

            var allFilePaths = Directory.GetFiles(pathToFiles);
            var allFileNames = allFilePaths.Select(filePath => Path.GetFileName(filePath)).ToList();
            var matchingFiles = allFileNames.Where(file => Regex.Match(file, fileRegex).Success).ToList();

            if (matchingFiles.Count == 0)
                throw new Exception(
                    "Cannot find the static asset you requested: "
                    + $"directory[{directory}] fileRegex[{fileRegex}]");
            if (matchingFiles.Count > 1)
                throw new Exception(
                    "We found more than 1 matching static assets: "
                    + $"directory[{directory}] fileRegex[{fileRegex}] found[{matchingFiles.Count}]");
            return "/" + directory + "/" + matchingFiles[0];
        }
    }
}