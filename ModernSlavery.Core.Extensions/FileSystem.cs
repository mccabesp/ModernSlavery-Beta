using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;

namespace ModernSlavery.Core.Extensions
{
    public static class FileSystem
    {
        public const char ReplacementChar = '\uFFFD';//'�' - replacement character used when cant encode/decode

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

        /// <summary>
        /// Reads text from a stream converting source encoding specified by Byte-Order-Markers (BOM) to target and optionally replaces any extended separator spacing characters (eg., non-breaking space) with standard space character.
        /// Throws an exception if cannot convert any characters
        /// </summary>
        /// <param name="stream">The source stream of characters</param>
        /// <param name="encoding">The target encoding (Default=UTF8) when encoding not specified in Byte-Order-Markers (BOM)</param>
        /// <param name="replaceSpaceSeparators">Whether to replace extended seperator-spaces (eg., non-breaking space) with standard space character</param>
        /// <returns>A text string using target encoding with any required separator-space replacements</returns>
        public static string ReadTextWithEncoding(this Stream stream, Encoding? targetEncoding = null, bool replaceSpaceSeparators=true)
        {
            if (stream==null) throw new ArgumentNullException(nameof(stream));

            if (targetEncoding == null) targetEncoding = Encoding.Default;

            targetEncoding = Encoding.GetEncoding(targetEncoding.CodePage,new EncoderExceptionFallback(),new DecoderExceptionFallback());

            string text;
            using var sr = new StreamReader(stream, targetEncoding, true);
            text = sr.ReadToEnd();
            return replaceSpaceSeparators ? text?.ReplaceSpaceSeparators() : text;
        }

        /// <summary>
        /// Reads text from a stream converting source encoding specified by Byte-Order-Markers (BOM) to target and optionally replaces any extended separator spacing characters (eg., non-breaking space) with standard space character.
        /// Throws an exception if cannot convert any characters
        /// </summary>
        /// <param name="filePath">The full path and filename of the source file</param>
        /// <param name="targetEncoding">The target encoding (Default=UTF8) when encoding not specified in Byte-Order-Markers (BOM)</param>
        /// <param name="replaceSpaceSeparators">Whether to replace extended seperator-spaces (eg., non-breaking space) with standard space character</param>
        /// <returns>A text string using target encoding with any required separator-space replacements</returns>
        public static string ReadTextWithEncoding(string filePath, Encoding? targetEncoding = null, bool replaceSpaceSeparators = true)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException("Cannot find file",filePath);

            using var stream=new FileStream(filePath, FileMode.Open);
            return stream.ReadTextWithEncoding(targetEncoding, replaceSpaceSeparators);
        }

        /// <summary>
        /// Checks a string doesnt contain any default replacement characters due to bad encoding/decoding
        /// </summary>
        /// <param name="text">The text to check</param>
        /// <returns>The unchanged text to check</returns>
        public static string CheckEncoding(this string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && text.Contains(ReplacementChar)) throw new ArgumentException("Replacement character '�' found - indicates incorrect encoding/decoding possibly due to missing or incorrect Byte-Order-Mark (BOM) in original source file", nameof(text));
            return text;

        }

    }
}