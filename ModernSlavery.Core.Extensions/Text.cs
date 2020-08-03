﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModernSlavery.Core.Extensions
{
    public static class Text
    {
        public const string NumberChars = "1234567890";
        public const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string LowerCaseChars = UpperCaseChars.ToLower();

        public static bool IsNumber(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            return Regex.IsMatch(input, "^\\d+$");
        }

        public static bool IsCompanyNumber(this string input, int minLength = 8)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            if (!input.ContainsNumber()) return false;

            return Regex.IsMatch(input, "^[0-9A-Za-z]{" + minLength + ",8}$");
        }

        public static bool IsDUNSNumber(this string input, int minLength = 9)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            if (!input.ContainsNumber()) return false;

            return Regex.IsMatch(input, "^[0-9]{" + minLength + ",9}$");
        }

        public static string FixCompanyNumber(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.IsCompanyNumber(6)) input = input.PadLeft(8, '0');

            return input;
        }

        public static bool ContainsNumber(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            return Regex.IsMatch(input, "[0-9]");
        }

        public static bool IsAnyNullOrWhiteSpace(params string[] inputs)
        {
            return inputs.Any(i => string.IsNullOrWhiteSpace(i));
        }

        public static bool IsAnyNull<T>(params T[] inputs)
        {
            return inputs.Any(i => i==null);
        }

        public static bool IsAllNullOrWhiteSpace(params string[] inputs)
        {
            return inputs.All(i => string.IsNullOrWhiteSpace(i));
        }

        public static bool IsAllNull<T>(params T[] inputs)
        {
            return inputs.All(i => i == null);
        }


        private const string variablePattern = @"\$\((.*?)\)";
        public static readonly Regex VariableRegex = new Regex(variablePattern, RegexOptions.IgnoreCase);

        /// <summary>
        ///     Returns the name of the variable between $( and )
        /// </summary>
        /// <param name="text">The text to search</param>
        /// <param name="matchPattern">The pattern to use</param>
        /// <returns></returns>
        public static string GetVariableName(this string text, string matchPattern = @"^\$\((.*)\)$")
        {
            return new Regex(matchPattern).Matches(text)?.FirstOrDefault()?.Groups[1]?.Value;
        }

        /// <summary>
        /// Resolves all variables using pattern $(key) from a dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary containing the keys and replacement values</param>
        /// <param name="text">The source text containing the variable declarations $(key)</param>
        /// <returns>The source text with all variable declarations replaced</returns>
        public static string ResolveVariableNames(this IDictionary<string, string> dictionary, string text)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (string.IsNullOrWhiteSpace(text)) return text;
            var badKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in VariableRegex.Matches(text))
            {
                var key = m.Groups[1].Value;
                if (dictionary.ContainsKey(key))
                    text = text.Replace(m.Groups[0].Value, dictionary[key],StringComparison.OrdinalIgnoreCase);
                else
                    badKeys.Add(key);
            }

            if (badKeys.Any()) throw new KeyNotFoundException($"Cannot find variables '{badKeys.ToDelimitedString(", ")}'");
            return text;
        }

        public static string TrimI(this string source, string trimChars)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimChars)) return source;

            return source.Trim(trimChars.ToCharArray());
        }

        public static string TrimI(this string source, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(source)) return source;

            return trimChars == null || trimChars.Length == 0 ? source.Trim() : source.Trim(trimChars);
        }

        public static string TrimSuffix(this string source, string suffix)
        {
            if (source.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
                source = source.Remove(source.Length - suffix.Length);

            return source;
        }

        public static string TrimStartI(this string source, string trimChars)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimChars)) return source;

            return source.TrimStartI(trimChars.ToCharArray());
        }

        public static string TrimStartI(this string source, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(source)) return source;

            return trimChars == null || trimChars.Length == 0 ? source.TrimStart() : source.TrimStart(trimChars);
        }

        public static string RemoveStartI(this string text, string prefix)
        {
            if (text.StartsWithI(prefix)) text = text.Substring(prefix.Length);

            return text;
        }

        public static string RemoveEndI(this string text, string suffix)
        {
            var i = text.LastIndexOf(suffix);
            if (i > -1) text = text.Substring(0, i);

            return text;
        }

        public static string Strip(this string text, string excludeChars)
        {
            if (text == null) return null;

            string newText = null;
            for (var i = 0; i < text.Length; i++)
                if (excludeChars.IndexOf(text[i]) < 0)
                    newText += text[i];

            return newText;
        }

        public static string Coalesce(this string text, params string[] options)
        {
            if (!string.IsNullOrWhiteSpace(text)) return text;

            foreach (var option in options)
                if (!string.IsNullOrWhiteSpace(option))
                    return option;

            return null;
        }

        //Checks if a byte sequence starts with another byte sequence
        public static bool StartsWith(this string text, byte[] subStr)
        {
            if (text.Length < subStr.Length || subStr.Length < 1) return false;

            for (var i = 0; i < subStr.Length; i++)
                if (text[i] != subStr[i])
                    return false;

            return true;
        }

        public static int LineCount(this string text, string newLineChars = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            if (string.IsNullOrWhiteSpace(newLineChars)) newLineChars = Environment.NewLine;

            text = text.Replace(newLineChars, "\n");
            var args = text.SplitI("\n");
            return args.Length;
        }

        /// <summary>
        ///     Returns all characters before the first occurrence of a string
        /// </summary>
        public static string BeforeFirst(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool inclusive = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var i = text.IndexOf(separator, 0, comparisionType);
            if (i > -1) return text.Substring(0, inclusive ? i + 1 : i);

            return includeWhenNoSeparator ? text : null;
        }

        /// <summary>
        ///     Returns all characters before the last occurrence of a string
        /// </summary>
        public static string BeforeLast(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool inclusive = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            int i = text.LastIndexOf(separator, text.Length - 1, comparisionType);
            if (i > -1)
            {
                return text.Substring(0, inclusive ? i + 1 : i);
            }

            return includeWhenNoSeparator ? text : null;
        }

        /// <summary>
        ///     Returns all characters after the first occurrence of a string
        /// </summary>
        public static string AfterFirst(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool includeSeparator = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var i = text.IndexOf(separator, 0, comparisionType);
            if (i > -1) return text.Substring(includeSeparator ? i : i + separator.Length);

            return includeWhenNoSeparator ? text : null;
        }

        /// <summary>
        ///     Returns all characters after the last occurrence of a string
        /// </summary>
        public static string AfterLast(this string text,
            string separator,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool includeSeparator = false,
            bool includeWhenNoSeparator = true)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            var i = text.LastIndexOf(separator, text.Length - 1, comparisionType);
            if (i > -1) return text.Substring(includeSeparator ? i : i + 1);

            return includeWhenNoSeparator ? text : null;
        }

        /// <summary>
        ///     Returns all characters after the last occurrence of any specified character
        /// </summary>
        public static string AfterLastAny(this string text,
            string separators,
            StringComparison comparisionType = StringComparison.OrdinalIgnoreCase,
            bool inclusive = false)
        {
            if (comparisionType.IsAny(
                StringComparison.OrdinalIgnoreCase,
                StringComparison.CurrentCultureIgnoreCase,
                StringComparison.InvariantCultureIgnoreCase))
            {
                text = text.ToLower();
                separators = separators.ToLower();
            }

            var i = text.LastIndexOfAny(separators.ToCharArray(), text.Length - 1);
            if (i > -1) return text.Substring(inclusive ? i : i + 1);

            return null;
        }

        public static bool EqualsI(this string original, params string[] target)
        {
            if (string.IsNullOrWhiteSpace(original)) original = "";

            for (var i = 0; i < target.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(target[i])) target[i] = "";

                if (original.Equals(target[i], StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            return false;
        }

        public static bool ContainsI(this string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrEmpty(pattern)) return false;

            return source.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool ContainsAll(this string text, string validCharacters)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            if (validCharacters == null || validCharacters.Length == 0)
                throw new ArgumentNullException(nameof(validCharacters));

            foreach (var c in text)
                if (validCharacters.IndexOf(c) == -1)
                    return false;

            return true;
        }

        public static bool ContainsAny(this string text, params char[] characters)
        {
            if (string.IsNullOrWhiteSpace(text) || characters == null || characters.Length < 1) return false;

            return text.IndexOfAny(characters) > -1;
        }

        public static bool EndsWithI(this string source, params string[] strings)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;

            foreach (var str in strings)
            {
                if (string.IsNullOrWhiteSpace(str)) continue;

                if (source.ToLower().EndsWith(str.ToLower())) return true;
            }

            return false;
        }

        public static bool StartsWithAny(this string source, params char[] chars)
        {
            return source.Length > 0 && source[0].IsAny(chars);
        }

        /// <summary>
        ///     Super-fast Case-Insensitive text replace
        /// </summary>
        /// <param name="text">The original text string</param>
        /// <param name="fromStr">The string to search for</param>
        /// <param name="toStr">The string to replace with</param>
        /// <returns></returns>
        public static string ReplaceI(this string original, string pattern, string replacement = null)
        {
            if (string.IsNullOrWhiteSpace(original)) return null;

            if (string.IsNullOrWhiteSpace(replacement)) replacement = "";

            int count, position0, position1;
            count = position0 = position1 = 0;
            var upperString = original.ToUpper();
            var upperPattern = pattern.ToUpper();
            var inc = original.Length / pattern.Length * (replacement.Length - pattern.Length);
            var chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(
                       upperPattern,
                       position0))
                   != -1)
            {
                for (var i = position0; i < position1; ++i) chars[count++] = original[i];

                for (var i = 0; i < replacement.Length; ++i) chars[count++] = replacement[i];

                position0 = position1 + pattern.Length;
            }

            if (position0 == 0) return original;

            for (var i = position0; i < original.Length; ++i) chars[count++] = original[i];

            return new string(chars, 0, count);
        }

        public static bool StartsWithI(this string original, params string[] texts)
        {
            if (string.IsNullOrWhiteSpace(original)) return false;

            if (texts != null)
                foreach (var text in texts)
                    if (text != null && original.ToLower().StartsWith(text.ToLower()))
                        return true;

            return false;
        }

        public static string Left(this string original, int length)
        {
            if (string.IsNullOrWhiteSpace(original) || length >= original.Length) return original;

            return original.Substring(0, length);
        }

        public static bool LikeAny(this string input, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
                if (input.Like(pattern))
                    return true;

            return false;
        }

        public static bool Like(this string input, string pattern)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(pattern)) return false;

            input = input.ToLower();
            pattern = pattern.ToLower().Trim();
            if (input == pattern) return true;

            var expression = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".").Replace("+", "\\+") +
                             "$";
            return Regex.IsMatch(input, expression);
        }

        public static string ToShortString(this Guid guid)
        {
            return guid.ToString().Strip("- {}");
        }

        public static string EncodeUrlBase64(this string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                base64String = base64String.Replace('+', '-');
                base64String = base64String.Replace('/', '_');
                base64String = base64String.Replace('=', '!');
            }

            return base64String;
        }

        public static string DecodeUrlBase64(this string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                base64String = base64String.Replace('-', '+');
                base64String = base64String.Replace('_', '/');
                base64String = base64String.Replace('!', '=');
            }

            return base64String;
        }

        public static int LevenshteinCompute(this string s, string t, bool caseIndensitive = true)
        {
            if (caseIndensitive)
            {
                s = s.ToLower();
                t = t.ToLower();
            }

            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) return m;

            if (m == 0) return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
                //Step 4
            for (var j = 1; j <= m; j++)
            {
                // Step 5
                var cost = t[j - 1] == s[i - 1] ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

            // Step 7
            return d[n, m];
        }

        public static bool IsUK(this string country)
        {
            return string.IsNullOrWhiteSpace(country)
                   || country.EqualsI(
                       "England",
                       "United Kingdom",
                       "GB",
                       "UK",
                       "Great Britain",
                       "Britain",
                       "Scotland",
                       "Wales",
                       "Northern Ireland");
        }

        public static void WriteLine(this StringWriter writer, Color color, string text, string tagName = "span")
        {
            writer.WriteLine($"<{tagName} style=\"color:{ColorTranslator.ToHtml(color)}\">{text}</{tagName}>");
        }

        public static string ToAbbr(this string s, string separator = "", int minLength = 3,
            params string[] excludeWords)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            var wordList = s.ToLower().SplitI(" .-;:_,&+[]{}<>()").ToList();

            if (excludeWords != null && excludeWords.Length > 0)
                wordList = wordList.Except(excludeWords, StringComparer.OrdinalIgnoreCase).ToList();

            if (wordList.Count < minLength) return null;

            return string.Join(separator, wordList.Select(x => x[0]));
        }
    }
}