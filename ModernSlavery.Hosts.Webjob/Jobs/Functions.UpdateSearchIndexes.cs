using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Update the search indexes
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateSearchIndexesAsync([TimerTrigger("%UpdateSearchIndexesAsync%", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            if (!_searchOptions.Disabled)
                try
            {
                await UpdateAllSearchIndexesAsync(log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", ex.Message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task UpdateAllSearchIndexesAsync(ILogger log, string userEmail = null, bool force = false)
        {
            log.LogInformation($"-- Started the updating of index {_EmployerSearchRepository.IndexName}");
            await UpdateSearchAsync(log, _EmployerSearchRepository, _EmployerSearchRepository.IndexName, userEmail,
                force).ConfigureAwait(false);
            log.LogInformation($"-- Updating of index {_EmployerSearchRepository.IndexName} completed");
            log.LogInformation($"-- Started the updating of index {_SicCodeSearchRepository.IndexName}");
            await UpdateSearchAsync(log, _SicCodeSearchRepository, _SicCodeSearchRepository.IndexName, userEmail,
                force).ConfigureAwait(false);
            log.LogInformation($"-- Updating of index {_SicCodeSearchRepository.IndexName} completed");
        }

        public async Task UpdateSearchAsync(ILogger log, string userEmail = null, bool force = false)
        {
            await UpdateSearchAsync(log, _EmployerSearchRepository, _EmployerSearchRepository.IndexName, userEmail,
                force).ConfigureAwait(false);
        }

        private async Task UpdateSearchAsync<T>(ILogger log,
            ISearchRepository<T> searchRepositoryToUpdate,
            string indexNameToUpdate,
            string userEmail = null,
            bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateSearchAsync)))
            {
                log.LogInformation("The set of running jobs already contains 'UpdateSearch'");
                return;
            }

            try
            {
                await searchRepositoryToUpdate.CreateIndexIfNotExistsAsync(indexNameToUpdate).ConfigureAwait(false);

                if (typeof(T) == typeof(EmployerSearchModel))
                    await AddDataToIndexAsync(log).ConfigureAwait(false);
                else if (typeof(T) == typeof(SicCodeSearchModel))
                    await AddDataToSicCodesIndexAsync(log).ConfigureAwait(false);
                else
                    throw new ArgumentException($"Type {typeof(T)} is not a valid type.");

                if (force && !string.IsNullOrWhiteSpace(userEmail))
                    try
                    {
                        await _Messenger.SendMessageAsync(
                            "UpdateSearchIndexes complete",
                            userEmail,
                            "The update of the search indexes completed successfully.").ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "UpdateSearch: An error occurred trying to send an email");
                    }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateSearchAsync));
            }
        }

        private async Task AddDataToSicCodesIndexAsync(ILogger log)
        {
            var listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync(log).ConfigureAwait(false);

            if (listOfSicCodeRecords == null || !listOfSicCodeRecords.Any())
            {
                log.LogInformation($"No records to be added to index {_SicCodeSearchRepository.IndexName}");
                return;
            }

            await _SicCodeSearchRepository.RefreshIndexDataAsync(listOfSicCodeRecords).ConfigureAwait(false);
        }

        private async Task AddDataToIndexAsync(ILogger log)
        {
            var allOrgs = _SharedBusinessLogic.DataRepository.GetAll<Organisation>();
            var allOrgsList = await allOrgs.ToListAsync().ConfigureAwait(false);

            //Remove the test organisations
            if (!string.IsNullOrWhiteSpace(_SharedBusinessLogic.SharedOptions.TestPrefix))
                allOrgsList.RemoveAll(
                    o => o.OrganisationName.StartsWithI(_SharedBusinessLogic.SharedOptions.TestPrefix));

            var lookupResult = _searchBusinessLogic.LookupSearchableOrganisations(allOrgsList);
            if (Debugger.IsAttached) lookupResult = lookupResult.Take(100);
            var listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync(log).ConfigureAwait(false);
            var selection = lookupResult.Select(o => _OrganisationBusinessLogic.CreateEmployerSearchModel(o, false, listOfSicCodeRecords));
            var selectionList = selection.ToList();

            if (selectionList.Any()) await _EmployerSearchRepository.RefreshIndexDataAsync(selectionList).ConfigureAwait(false);
        }

        private async Task<List<SicCodeSearchModel>> GetListOfSicCodeSearchModelsFromFileAsync(ILogger log)
        {
            List<SicCodeSearchModel> listOfSicCodeRecords = null;

            try
            {
                var files = await _SharedBusinessLogic.FileRepository.GetFilesAsync(
                    _SharedBusinessLogic.SharedOptions.DataPath, Filenames.SicSectorSynonyms).ConfigureAwait(false);
                var sicSectorSynonymsFilePath = files.OrderByDescending(f => f).FirstOrDefault();

                #region Pattern

                var rootDirMightIndicateCurrentLocation =
                    string.IsNullOrEmpty(_SharedBusinessLogic.FileRepository.RootDir)
                        ? "root"
                        : $"path {_SharedBusinessLogic.FileRepository.RootDir}";
                var fileInPathMessage =
                    $"pattern {Filenames.SicSectorSynonyms} in {rootDirMightIndicateCurrentLocation}.";
                if (string.IsNullOrEmpty(sicSectorSynonymsFilePath))
                {
                    var unableToFindPathMessage = $"Unable to find {fileInPathMessage}";
                    log.LogError(unableToFindPathMessage);
                    return null;
                }

                log.LogInformation($"Found {fileInPathMessage}");

                #endregion

                #region File exist

                var fileExists =
                    await _SharedBusinessLogic.FileRepository.GetFileExistsAsync(sicSectorSynonymsFilePath).ConfigureAwait(false);
                if (!fileExists)
                {
                    var fileDoesNotExistMessage = $"File does not exist {sicSectorSynonymsFilePath}.";
                    log.LogError(fileDoesNotExistMessage);
                    return null;
                }

                log.LogInformation($"File exists {sicSectorSynonymsFilePath}");

                #endregion

                var csv =
                    await _SharedBusinessLogic.FileRepository.ReadCSVAsync<SicCodeSearchModel>(
                        sicSectorSynonymsFilePath).ConfigureAwait(false);

                listOfSicCodeRecords = csv.OrderBy(o => o.SicCodeId).ToList();

                #region Records found

                if (!listOfSicCodeRecords.Any())
                {
                    var noRecordsFoundMessage = $"No records found in '{sicSectorSynonymsFilePath}'";
                    log.LogError(noRecordsFoundMessage);
                    return listOfSicCodeRecords;
                }

                log.LogInformation($"Number of records found {listOfSicCodeRecords.Count}");

                #endregion
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetListOfSicCodeSearchModelsFromFile function");
            }

            return listOfSicCodeRecords;
        }
    }
}