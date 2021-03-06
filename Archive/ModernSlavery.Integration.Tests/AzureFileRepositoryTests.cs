﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using NUnit.Framework;

namespace ModernSlavery.Integration.Tests
{
    [TestFixture]
    public class AzureFileRepositoryTests
    {
        [Test]
        public async Task AzureFileRepository_Append_Csv_Records_Async()
        {
            // Arrange
            var systemFileRepository = new SystemFileRepository(new StorageOptions());

            var searchLogFilename =
                "testSearchLog" + VirtualDateTime.Now.ToString("yyyyMMdd_HHmmssfff") + "_deleteme.csv";

            try
            {
                #region Creating a search log message that will be persisted using IFileRepository

                // SearchParameters must be created so the 'searchParametersSentToTheSearchEngine.ToString()' below creates the queryString to send to Azure.
                // It did happen before that version 9.0 of the library didn't override the 'tostring' and instead of returning "$count=true;$Blah" it was returning "schemdlj" (rendering our search logs useless).
                // Leaving object 'SearchParameters' here serves as a flag that this functionality is still reporting useful info into the logs irrespective of any library updates. 
                var searchParametersSentToTheSearchEngine = new SearchParameters
                {
                    SearchMode = SearchMode.Any,
                    Top = 50,
                    Skip = 100,
                    IncludeTotalResultCount = true,
                    QueryType = QueryType.Simple
                };

                var telemetryProperties = new Dictionary<string, string>
                {
                    {"TimeStamp", new DateTime(2019, 06, 25, 14, 40, 54).ToString("yyyy-MM-dd HH:mm:ss.fff")},
                    {"QueryTerms", "International bank of Japan"},
                    {"ResultCount", "25"},
                    {"SearchType", SearchTypes.ByEmployerName.ToString()},
                    {"SearchParameters", HttpUtility.UrlDecode(searchParametersSentToTheSearchEngine.ToString())}
                };

                var searchLogRecords = new List<Dictionary<string, string>> {telemetryProperties};

                #endregion

                // Act
                await systemFileRepository.AppendCsvRecordsAsync(searchLogFilename, searchLogRecords);
                var actualContent = await systemFileRepository.ReadAsync(searchLogFilename);

                // Assert
                var expectedContent =
                    "\"TimeStamp\",\"QueryTerms\",\"ResultCount\",\"SearchType\",\"SearchParameters\"\r\n\"2019-06-25 14:40:54.000\",\"International bank of Japan\",\"25\",\"ByEmployerName\",\"$count=true&queryType=simple&searchMode=any&$skip=100&$top=50\"\r\n";
                Assert.AreEqual(expectedContent, actualContent);
            }
            finally
            {
                await systemFileRepository.DeleteFileAsync(searchLogFilename);
            }
        }
    }
}