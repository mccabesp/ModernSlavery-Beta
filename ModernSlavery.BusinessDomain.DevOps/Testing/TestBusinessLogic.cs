using Autofac.Features.AttributeFilters;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Azure.AppInsights;
using ModernSlavery.Infrastructure.Azure.Cache;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.DevOps.Testing
{
    public interface ITestBusinessLogic
    {
        Task ClearAppInsightsAsync();
        Task ClearAppInsightsAsync(string subscriptionId, string resourceGroupName, string resourceName, params string[] tableNames);
        Task ClearAppInsightsAsync(string subscriptionId, string resourceGroupName, string resourceName, string tableName);

        Task ClearAppInsightsAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        Task ClearCacheAsync();

        Task DeleteFilesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteDraftFilesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteDraftFilesAsync(string organisationIdentifier, long reportingDeadlineYear);
        Task ResetDatabaseAsync(bool keepAdministrators = false);
        Task ClearDatabaseAsync(DateTime? startDate = null, DateTime? endDate = null, bool keepAdministrators=false);
        Task ClearQueuesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteStatementsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteRegistrationsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task DeleteUsersAsync(DateTime? startDate = null, DateTime? endDate = null, bool keepAdministrators = false);

        Task SeedDatabaseAsync(int maxRecords = 0);
        Task InitialiseDatabaseWebjobsAsync();

        Task DeleteSearchDocumentsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task ResetSearchIndexesAsync();
        Task RefreshSearchDocumentsAsync();
        void ClearLocalSettingsDump();
        void ClearLocalLogs();
        Task SetIsUkAddressesAsync();
        Task QueueWebjob(string webjobName);
    }

    public class TestBusinessLogic : ITestBusinessLogic
    {
        #region Dependencies
        private readonly AppInsightsManager _appInsightsManager;
        private readonly ApplicationInsightsOptions _appInsightsOptions;
        private readonly IFileRepository _fileRepository;
        private readonly IDataRepository _dataRepository;
        private readonly SharedOptions _sharedOptions;
        private readonly SubmissionOptions _submissionOptions;
        private readonly IObfuscator _obfuscator;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private readonly IScopeBusinessLogic _scopeBusinessLogic;
        private readonly IDataImporter _dataImporter;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly ISearchRepository<OrganisationSearchModel> _searchRepository;
        private readonly IAzureStorageManager _azureStorageManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthorisationBusinessLogic _authorisationBusinessLogic;
        private readonly DistributedCacheManager _cacheManager;
        private readonly IQueue _executeWebjobQueue;

        #endregion

        #region Constructors
        public TestBusinessLogic(AppInsightsManager appInsightsManager, ApplicationInsightsOptions appInsightsOptions, IFileRepository fileRepository, IAzureStorageManager azureStorageManager, IDataRepository dataRepository, SharedOptions sharedOptions, SubmissionOptions submissionOptions, IObfuscator obfuscator, IOrganisationBusinessLogic organisationBusinessLogic, IScopeBusinessLogic scopeBusinessLogic, IDataImporter dataImporter, ISearchBusinessLogic searchBusinessLogic, ISearchRepository<OrganisationSearchModel> searchRepository, IConfiguration configuration, IAuthorisationBusinessLogic authorisationBusinessLogic, DistributedCacheManager cacheManager, [KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue)
        {
            _appInsightsManager = appInsightsManager;
            _appInsightsOptions = appInsightsOptions;
            _azureStorageManager = azureStorageManager;
            _fileRepository = fileRepository;
            _dataRepository = dataRepository;
            _sharedOptions = sharedOptions;
            _submissionOptions = submissionOptions;
            _obfuscator = obfuscator;
            _organisationBusinessLogic = organisationBusinessLogic;
            _scopeBusinessLogic = scopeBusinessLogic;
            _dataImporter = dataImporter;
            _searchBusinessLogic = searchBusinessLogic;
            _searchRepository = searchRepository;
            _configuration = configuration;
            _authorisationBusinessLogic = authorisationBusinessLogic;
            _cacheManager = cacheManager;
            _executeWebjobQueue = executeWebjobQueue;
        }
        #endregion

        private const int _commandTimeout = 500;

        public async Task ClearAppInsightsAsync()
        {
            var resourceInfo=await _appInsightsManager.GetResourceInfoAsync(_appInsightsOptions.InstrumentationKey).ConfigureAwait(false);

            //see https://docs.microsoft.com/en-us/azure/azure-monitor/app/apm-tables
            var tableNames = new[] { "availabilityResults", "browserTimings", "dependencies", "customEvents", "customMetrics", "pageViews", "performanceCounters", "requests", "exceptions", "traces" };
            await ClearAppInsightsAsync(resourceInfo.subscriptionId, resourceInfo.resourceGroupName, resourceInfo.resourceName, tableNames).ConfigureAwait(false);
        }

        public async Task ClearAppInsightsAsync(string subscriptionId, string resourceGroupName, string resourceName, params string[] tableNames)
        {
            var exceptions = new ConcurrentBag<Exception>();
            Parallel.ForEach(tableNames, async tableName => {
                try
                {
                    await ClearAppInsightsAsync(subscriptionId, resourceGroupName, resourceName, tableName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
            if (exceptions.Any()) throw new AggregateException(exceptions);
        }

        public async Task ClearAppInsightsAsync(string subscriptionId, string resourceGroupName, string resourceName, string tableName)
        {
            //Throttle: 50 requests per hour
            var purgeId = await _appInsightsManager.PurgeDataAsync(subscriptionId, resourceGroupName, resourceName, tableName).ConfigureAwait(false);
            var status = await _appInsightsManager.GetPurgeStatusAsync(subscriptionId, resourceGroupName, resourceName, purgeId).ConfigureAwait(false);

            if (!status.EqualsI("completed", "pending")) throw new Exception($"Invalid purge status={status} for table={tableName}");
        }

        #region Queues
        public async Task ClearAppInsightsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            await foreach (var queue in _azureStorageManager.ListQueuesAsync())
                await _azureStorageManager.ClearQueueAsync(queue.name,startDate, endDate).ConfigureAwait(false);
        }
        #endregion

        #region Cache
        public async Task ClearCacheAsync()
        {
            await _cacheManager.ClearCacheAsync().ConfigureAwait(false);
        }
        #endregion

        #region Delete files
        public async Task DeleteFilesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");
            if (startDate != null && endDate != null && startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");

            if (!await _fileRepository.GetDirectoryExistsAsync(_fileRepository.RootPath).ConfigureAwait(false)) return;

            var files = await _fileRepository.GetFilesAsync(_fileRepository.RootPath,recursive:true).ConfigureAwait(false);
            DeleteFiles(files, startDate, endDate);
        }

        public async Task DeleteDraftFilesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            //TODO: date filtering
            if (startDate != null || endDate != null) throw new NotImplementedException();

            if (!await _fileRepository.GetDirectoryExistsAsync(_submissionOptions.DraftsPath).ConfigureAwait(false)) return;

            var files = await _fileRepository.GetFilesAsync(_submissionOptions.DraftsPath).ConfigureAwait(false);

            DeleteFiles(files, startDate, endDate);
        }

        public async Task DeleteDraftFilesAsync(string organisationIdentifier, long reportingDeadlineYear)
        {
            long organisationId = _obfuscator.DeObfuscate(organisationIdentifier);

            var filePattern = $"{organisationId}_{reportingDeadlineYear}.*";

            var files = await _fileRepository.GetFilesAsync(_submissionOptions.DraftsPath, filePattern).ConfigureAwait(false);
            DeleteFiles(files);
        }

        private void DeleteFiles(IEnumerable<string> files, DateTime? startDate = null, DateTime? endDate = null)
        {
            Parallel.ForEach(files, async file =>
            {
                if (file.ContainsI(_sharedOptions.AppDataPath)) return;

                if (startDate != null || endDate != null)
                {
                    var modified = await _fileRepository.GetLastWriteTimeAsync(file).ConfigureAwait(false);

                    if (startDate != null && modified < startDate) return;
                    if (endDate != null && modified > endDate) return;
                }

                await _fileRepository.DeleteFileAsync(file).ConfigureAwait(false);
            });
        }
        #endregion

        #region Database
        /// <summary>
        /// Deletes all records in all tables created after a specified date
        /// </summary>
        /// <param name="host"></param>
        public async Task ClearDatabaseAsync(DateTime? startDate = null, DateTime? endDate = null, bool keepAdministrators = false)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");

            //Get the old timeout
            var oldCommandTimeout = _dataRepository.GetCommandTimeout();

            //Set a new longer 5 min timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(_commandTimeout);

            //Delete the records
            if (startDate != null && endDate != null)
            {
                if (startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");
                await _dataRepository.GetAll<AuditLog>().Where(r => r.CreatedDate >= startDate && r.CreatedDate < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Feedback>().Where(r => r.CreatedDate >= startDate && r.CreatedDate < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate && r.Created < endDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<UserOrganisation>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<ReminderEmail>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                var users = _dataRepository.GetAll<User>().Where(r => r.Created >= startDate && r.Created < endDate);
                if (keepAdministrators) users = users.Where(u => !_authorisationBusinessLogic.IsAdministrator(u));
                await users.BatchDeleteAsync().ConfigureAwait(false);

                await _dataRepository.GetAll<PublicSectorType>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicCode>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicSection>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else if (startDate != null && endDate == null)
            {
                await _dataRepository.GetAll<AuditLog>().Where(r => r.CreatedDate >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Feedback>().Where(r => r.CreatedDate >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
                await _dataRepository.GetAll<UserOrganisation>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<ReminderEmail>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                var users = _dataRepository.GetAll<User>().Where(r => r.Created >= startDate);
                if (keepAdministrators) users = users.Where(u => !_authorisationBusinessLogic.IsAdministrator(u));
                await users.BatchDeleteAsync().ConfigureAwait(false);

                await _dataRepository.GetAll<PublicSectorType>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicCode>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicSection>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else if (startDate == null && endDate != null)
            {
                await _dataRepository.GetAll<AuditLog>().Where(r => r.CreatedDate < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Feedback>().Where(r => r.CreatedDate < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created < endDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false); 
                await _dataRepository.GetAll<UserOrganisation>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<ReminderEmail>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);

                var users = _dataRepository.GetAll<User>().Where(r => r.Created < endDate);
                if (keepAdministrators) users = users.Where(u => !_authorisationBusinessLogic.IsAdministrator(u));
                await users.BatchDeleteAsync().ConfigureAwait(false);

                await _dataRepository.GetAll<PublicSectorType>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicCode>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
                await _dataRepository.GetAll<SicSection>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else
            {
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<AuditLog>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<Feedback>()).ConfigureAwait(false);
                await _dataRepository.GetAll<Organisation>().BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<Statement>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<UserOrganisation>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<Organisation>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<ReminderEmail>()).ConfigureAwait(false);
                if (keepAdministrators)
                {
                    var users = _dataRepository.GetAll<User>().ToList().Where(u => !_authorisationBusinessLogic.IsAdministrator(u));
                    await _dataRepository.BulkDeleteAsync(users);
                }
                else
                {
                    await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<User>()).ConfigureAwait(false);
                }
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<PublicSectorType>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<SicCode>()).ConfigureAwait(false);
                await _dataRepository.BulkDeleteAsync(_dataRepository.GetAll<SicSection>()).ConfigureAwait(false);
            }

            //restore the old timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);

            //Reload the database context
            _dataRepository.Reload();
        }
        
        public async Task DeleteStatementsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");

            //Get the old timeout
            var oldCommandTimeout=_dataRepository.GetCommandTimeout();

            //Set a new longer 5 min timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(_commandTimeout);

            //Delete the records
            if (startDate != null && endDate != null)
            {
                if (startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate && r.Created < endDate).BatchUpdateAsync(o => new Organisation { LatestStatementId = null}).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else if (startDate != null && endDate == null)
            {
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate).BatchUpdateAsync(o => new Organisation { LatestStatementId = null }).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created >= startDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else if (startDate == null && endDate != null)
            {
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created < endDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().Where(r => r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else
            {
                await _dataRepository.GetAll<Organisation>().BatchUpdateAsync(o => new Organisation { LatestStatementId = null}).ConfigureAwait(false);
                await _dataRepository.GetAll<Statement>().BatchDeleteAsync().ConfigureAwait(false);
            }

            //restore the old timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);
        }

        public async Task DeleteRegistrationsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");

            //Get the old timeout
            var oldCommandTimeout = _dataRepository.GetCommandTimeout();

            //Set a new longer 5 min timeout
            if (_commandTimeout!= oldCommandTimeout)_dataRepository.SetCommandTimeout(_commandTimeout);

            //Delete the records
            if (startDate != null && endDate != null)
            {
                if (startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate && r.Created < endDate).BatchDeleteAsync().ConfigureAwait(false);
            }
            else if (startDate != null && endDate == null)
            {
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created >= startDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
            }
            else if (startDate == null && endDate != null)
            {
                await _dataRepository.GetAll<Organisation>().Where(r => r.Created < endDate).BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
            }
            else
            {
                await _dataRepository.GetAll<Organisation>().BatchUpdateAsync(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null }).ConfigureAwait(false);
            }

            //restore the old timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);
        }

        public async Task DeleteUsersAsync(DateTime? startDate = null, DateTime? endDate = null, bool keepAdministrators = false)
        {
            if (startDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} cannot be later than now");
            if (endDate >= DateTime.Now) throw new ArgumentOutOfRangeException(nameof(endDate), $"{nameof(endDate)} cannot be later than now");

            //Get the old timeout
            var oldCommandTimeout = _dataRepository.GetCommandTimeout();

            //Set a new longer 5 min timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(_commandTimeout);

            //Delete the records
            var users = _dataRepository.GetAll<User>();
            if (startDate != null && endDate != null)
            {
                if (startDate >= endDate) throw new ArgumentOutOfRangeException(nameof(startDate), $"{nameof(startDate)} must be less than {nameof(endDate)}");
                users = users.Where(r => r.Created >= startDate && r.Created < endDate);
            }
            else if (startDate != null && endDate == null)
            {
                users = users.Where(r => r.Created >= startDate);
            }
            else if (startDate == null && endDate != null)
            {
                users=users.Where(r => r.Created < endDate);
            }

            if (keepAdministrators) users = users.Where(u => !_authorisationBusinessLogic.IsAdministrator(u));

            await users.BatchDeleteAsync().ConfigureAwait(false);

            //restore the old timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);
        }

        public async Task SeedDatabaseAsync(int maxRecords=0)
        {
            //Get the old timeout
            var oldCommandTimeout = _dataRepository.GetCommandTimeout();

            //Set a new longer 5 min timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(_commandTimeout);

            await _dataImporter.ImportSICSectionsAsync().ConfigureAwait(false);
            await _dataImporter.ImportSICCodesAsync().ConfigureAwait(false);
            await _dataImporter.ImportStatementSectorTypesAsync().ConfigureAwait(false);
            await _dataImporter.ImportPrivateOrganisationsAsync(-1, maxRecords).ConfigureAwait(false);
            await _dataImporter.ImportPublicOrganisationsAsync(-1, maxRecords).ConfigureAwait(false);

            //restore the old timeout
            if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);
        }

        public async Task InitialiseDatabaseWebjobsAsync()
        {
            if (_configuration.IsDevelopment() || _configuration.IsTest())
            {
                //Get the old timeout
                var oldCommandTimeout = _dataRepository.GetCommandTimeout();

                //Set a new longer 5 min timeout
                if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(_commandTimeout);

                await _organisationBusinessLogic.FixLatestAddressesAsync().ConfigureAwait(false);
                await _organisationBusinessLogic.SetUniqueOrganisationReferencesAsync().ConfigureAwait(false);
                await _scopeBusinessLogic.SetPresumedScopesAsync().ConfigureAwait(false);
                await _organisationBusinessLogic.FixLatestScopesAsync().ConfigureAwait(false);

                //restore the old timeout
                if (_commandTimeout != oldCommandTimeout) _dataRepository.SetCommandTimeout(oldCommandTimeout);
            }
            else
            {
                await _executeWebjobQueue.AddMessageAsync(new QueueWrapper($"command=ReferenceOrganisations"));
                await _executeWebjobQueue.AddMessageAsync(new QueueWrapper($"command=SetPresumedScopes"));
                await _executeWebjobQueue.AddMessageAsync(new QueueWrapper($"command=FixLatestAsync"));
            }
        }

        public async Task SetIsUkAddressesAsync()
        {
            await _executeWebjobQueue.AddMessageAsync(new QueueWrapper($"command=SetIsUkAddressesAsync"));
        }

        public async Task ResetDatabaseAsync(bool keepAdministrators = false)
        {
            await ClearDatabaseAsync(keepAdministrators: keepAdministrators).ConfigureAwait(false);
            await SeedDatabaseAsync().ConfigureAwait(false);
            await InitialiseDatabaseWebjobsAsync().ConfigureAwait(false);
        }
        #endregion

        public async Task QueueWebjob(string webjobName)
        {
            await _executeWebjobQueue.AddMessageAsync(new QueueWrapper($"command={webjobName}"));
        }

        #region Search
        /// <summary>
        /// Deletes and recreated the latest search index and updates with latest submission information
        /// </summary>
        /// <param name="host"></param>
        public async Task ResetSearchIndexesAsync()
        {
            //Delete and recreate the search index
            await _searchRepository.DeleteIndexIfExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates with latest submission information to the search index
        /// </summary>
        /// <param name="host"></param>
        public async Task RefreshSearchDocumentsAsync()
        {
            //Recreate the search documents
            await _searchBusinessLogic.RefreshSearchDocumentsAsync().ConfigureAwait(false);
        }

        public async Task DeleteSearchDocumentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var documents=await _searchBusinessLogic.ListSearchDocumentsByTimestampAsync(startDate, endDate).ConfigureAwait(false);
            if (documents.Any()) await _searchBusinessLogic.RemoveSearchDocumentsAsync(documents).ConfigureAwait(false);
        }

        public async Task ClearQueuesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var queues=_azureStorageManager.ListQueuesAsync();
            await foreach (var queue in queues)
                await _azureStorageManager.ClearQueueAsync(queue.name, startDate, endDate).ConfigureAwait(false);
        }

        public void ClearLocalSettingsDump()
        {
            var dumpSettingPath = _configuration["DumpSettingPath"];
            if (!string.IsNullOrWhiteSpace(dumpSettingPath) && File.Exists(dumpSettingPath)) File.Delete(dumpSettingPath);
        }

        public void ClearLocalLogs()
        {
            var logsPath = _configuration["Filepaths:LogFiles"];
            logsPath = FileSystem.ExpandLocalPath(logsPath);
            foreach (var logFile in Directory.GetFiles(logsPath, "*.log"))
            {
                try
                {
                    if (File.Exists(logFile)) File.Delete(logFile);
                }
                catch { }
            }
            foreach (var logFile in Directory.GetFiles(logsPath, "*.xml"))
            {
                try
                {
                    if (File.Exists(logFile)) File.Delete(logFile);
                }
                catch { }
            }
        }
        #endregion

    }
}