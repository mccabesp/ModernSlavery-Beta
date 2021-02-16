using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Core.Classes
{
    public static class Extensions
    {
        #region FileSystem

        public class CsvBadDataException : CsvHelperException
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="CsvBadDataException" /> class.
            /// </summary>
            public CsvBadDataException()
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="CsvBadDataException" /> class
            ///     with a specified error message.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public CsvBadDataException(string message) : base(message)
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="CsvBadDataException" /> class
            ///     with a specified error message and a reference to the inner exception that
            ///     is the cause of this exception.
            /// </summary>
            /// <param name="message">The error message that explains the reason for the exception.</param>
            /// <param name="innerException">
            ///     The exception that is the cause of the current exception, or a null reference (Nothing in
            ///     Visual Basic) if no inner exception is specified.
            /// </param>
            public CsvBadDataException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public static async Task<List<T>> ReadCSVAsync<T>(this IFileRepository fileRepository, string filePath,bool validateHeaders = true)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            var content = await fileRepository.ReadAsync(filePath).ConfigureAwait(false);
            return ReadCSV<T>(content,validateHeaders);
        }

        public static List<T> ReadCSV<T>(string content, bool validateHeaders=true)
        {
            using (TextReader textReader = new StringReader(content))
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                config.ShouldQuote = (field, context) => true;
                config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;
                config.MissingFieldFound = null;
                config.IgnoreReferences=true; //Otherwise virtual properties are set with weird values
                if (!validateHeaders)config.HeaderValidated = null;

                using (var csvReader = new CsvReader(textReader, config))
                {
                    try
                    {
                        return csvReader.GetRecords<T>().ToList();
                    }
                    catch (CsvHelperException ex)
                    {
                        if (ex.Data.Count > 0) throw new CsvBadDataException(ex.Data.Values.ToList<string>()[0]);

                        //ex.Data.Values has more info...
                        throw;
                    }
                }
            }
        }

        public static DataTable ToDataTable(this string csvContent)
        {
            var table = new DataTable();

            using (TextReader sr = new StringReader(csvContent))
            {
                using (var csvReader = new CsvReader(sr, CultureInfo.CurrentCulture))
                {
                    csvReader.Read();
                    csvReader.ReadHeader();
                    foreach (var header in csvReader.Context.HeaderRecord) table.Columns.Add(header);

                    while (csvReader.Read())
                    {
                        var row = table.NewRow();
                        foreach (DataColumn col in table.Columns)
                            row[col.ColumnName] = csvReader.GetField(col.DataType, col.ColumnName);

                        table.Rows.Add(row);
                    }
                }
            }

            return table;
        }

        /// <summary>
        ///     Save records to remote CSV via temporary local storage
        /// </summary>
        /// <param name="records">collection of records to write</param>
        /// <param name="filePath">the remote location of the file to save overwrite</param>
        /// <param name="oldfilePath">the previous file (if any) to be deleted on successful copy</param>
        public static async Task<long> SaveCSVAsync(this IFileRepository fileRepository,
            IEnumerable records,
            string filePath,
            string oldfilePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            long size = 0;
            var tempfile = new FileInfo(Path.GetTempFileName());
            try
            {
                using (var textWriter = tempfile.CreateText())
                {
                    var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                    config.ShouldQuote = (field, context) => true;
                    config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;

                    using (var writer = new CsvWriter(textWriter, config))
                    {
                        writer.WriteRecords(records);
                    }
                }

                //Save CSV to storage
                await fileRepository.WriteAsync(filePath, tempfile).ConfigureAwait(false);

                size = await fileRepository.GetFileSizeAsync(filePath).ConfigureAwait(false);


                //Set the count in the metadata file
                var count = 0;
                foreach (var item in records) count++;

                await fileRepository.SetMetaDataAsync(filePath, "RecordCount", count.ToString()).ConfigureAwait(false);

                //Delete the old file if it exists
                if (!string.IsNullOrWhiteSpace(oldfilePath)
                    && await fileRepository.GetFileExistsAsync(oldfilePath).ConfigureAwait(false)
                    && !filePath.EqualsI(oldfilePath))
                    await fileRepository.DeleteFileAsync(oldfilePath).ConfigureAwait(false);
            }
            finally
            {
                File.Delete(tempfile.FullName);
            }

            return size;
        }

        public static string ToCSV(this DataTable datatable)
        {
            using (var stream = new MemoryStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var textWriter = new StreamWriter(stream))
                    {
                        var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                        config.ShouldQuote = (field, context) => true;
                        config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;

                        using (var writer = new CsvWriter(textWriter, config))
                        {
                            writer.Configuration.ShouldQuote = (field, context) => true;

                            // Write columns
                            foreach (DataColumn column in datatable.Columns
                            ) //copy datatable CHAIN to DT, or just use CHAIN
                                writer.WriteField(column.ColumnName);

                            writer.NextRecord();

                            // Write row values
                            foreach (DataRow row1 in datatable.Rows)
                            {
                                for (var i = 0; i < datatable.Columns.Count; i++) writer.WriteField(row1[i]);

                                writer.NextRecord();
                            }

                            textWriter.Flush();
                            stream.Position = 0;
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static async Task AppendCsvRecordsAsync<T>(this IFileRepository fileRepository, string filePath,
            IEnumerable<T> records)
        {
            if (records == null || !records.Any()) return;

            var table = records.ToDataTable();

            using (var textWriter = new StringWriter())
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture);
                config.ShouldQuote = (field, context) => true;
                config.TrimOptions = TrimOptions.InsideQuotes | TrimOptions.Trim;

                using (var writer = new CsvWriter(textWriter, config))
                {
                    if (!await fileRepository.GetFileExistsAsync(filePath).ConfigureAwait(false))
                    {
                        for (var c = 0; c < table.Columns.Count; c++) writer.WriteField(table.Columns[c].ColumnName);

                        writer.NextRecord();
                    }
                    else
                    {
                        writer.Configuration.HasHeaderRecord = false;
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        for (var c = 0; c < table.Columns.Count; c++) writer.WriteField(row[c].ToString());

                        writer.NextRecord();
                    }
                }

                var appendString = textWriter.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(appendString))
                {
                    await fileRepository.AppendAsync(filePath, appendString + Environment.NewLine).ConfigureAwait(false);

                    //Increase the count in the metadata file
                    var metaData = await fileRepository.GetMetaDataAsync(filePath, "RecordCount").ConfigureAwait(false);
                    var count = metaData.ToInt32();
                    count += records.Count();
                    await fileRepository.SetMetaDataAsync(filePath, "RecordCount", count.ToString()).ConfigureAwait(false);
                }
            }
        }

        public static async Task AppendCsvRecordAsync<T>(this IFileRepository fileRepository, string filePath, T record)
        {
            await AppendCsvRecordsAsync(fileRepository, filePath, new[] {record}).ConfigureAwait(false);
        }

        /// <summary>
        ///     pushes a local file to remote if it doesnt already exist
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="remotePath">The path to the remote file</param>
        /// <param name="localPath">The path to the local file (default=</param>
        public static async Task<bool> PushRemoteFileAsync(this IFileRepository fileRepository,
            string fileName,
            string remotePath,
            string localPath = null, bool OverwriteIfNewer = false)
        {
            if (string.IsNullOrWhiteSpace(remotePath)) throw new ArgumentNullException(nameof(remotePath));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            //Check the local file exists
            if (string.IsNullOrWhiteSpace(localPath)) localPath = "App_Data";
            localPath = FileSystem.ExpandLocalPath(localPath);
            localPath = Path.Combine(localPath, fileName);
            var localExists = File.Exists(localPath);
            if (!localExists) throw new FileNotFoundException($"File '{localPath}' does not exist");

            //Create the remote directory if it doesnt already exist
            if (!await fileRepository.GetDirectoryExistsAsync(remotePath).ConfigureAwait(false))
                await fileRepository.CreateDirectoryAsync(remotePath).ConfigureAwait(false);

            //Check if the remote file exists
            remotePath = Path.Combine(remotePath, fileName);
            var remoteExists = await fileRepository.GetFileExistsAsync(remotePath).ConfigureAwait(false);

            //Dont overwrite if remote is newer than local unless explicit override set
            if (remoteExists)
            {
                var remoteLastWriteTime = await fileRepository.GetLastWriteTimeAsync(remotePath).ConfigureAwait(false);
                var localLastWriteTime = File.GetLastWriteTime(localPath); 
                if (remoteLastWriteTime >= localLastWriteTime && !OverwriteIfNewer) return false;
            }

            //Overwrite remote 
            var localContent = File.ReadAllBytes(localPath);
            if (localContent.Length == 0) return false;            
            await fileRepository.WriteAsync(remotePath, localContent).ConfigureAwait(false);
            return true;
        }

        #endregion

        #region Dependency Modules
        public static void AddDependency<T>(this IList<Type> modules) where T : IDependencyModule
        {
            var moduleType = typeof(T);
            var i=modules.IndexOf(moduleType);
            if (i > -1)
            {
                modules.RemoveAt(i);
                modules.Insert(i, moduleType);
            }
            else
                modules.Add(moduleType);
        }

        public static bool RemoveDependency<T>(this IList<Type> modules) where T : IDependencyModule
        {
            return modules.Remove(typeof(T));
        }
        #endregion
    }
}