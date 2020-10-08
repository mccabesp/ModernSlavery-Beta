﻿using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace ModernSlavery.Core.Extensions
{
    public static class FileSystem
    {
        /// Expands a condensed path relative to the application path (or basePath) up to a full path
        public static string ExpandLocalPath(string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            if (string.IsNullOrWhiteSpace(basePath)) basePath = AppDomain.CurrentDomain.BaseDirectory;

            path = path.Replace(@"/", @"\");
            path = path.Replace(@"~\", @".\");
            path = path.Replace(@"\\", @"\");

            if (path.StartsWith(@".\") || path.StartsWith(@"..\"))
            {
                var uri = new Uri(Path.Combine(basePath, path));
                return Path.GetFullPath(uri.LocalPath);
            }

            while (path.StartsWithAny('\\', '/')) path = path.Substring(1);

            if (!Path.IsPathRooted(path)) path = Path.Combine(basePath, path);

            return path;
        }

        public static bool IsValidFilePath(string filepath)
        {
            var file = new FileInfo(filepath);
            if (!string.IsNullOrWhiteSpace(file.DirectoryName) && !IsValidPathName(file.DirectoryName)) return false;
            return IsValidFileName(file.Name);
        }

        public static bool IsValidPathName(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }
        public static bool IsValidFileName(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename) && filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 ;
        }

        public static string GetMimeMapping(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(fileName, out contentType)) contentType = "application/octet-stream";

            return contentType;
        }
    }
}