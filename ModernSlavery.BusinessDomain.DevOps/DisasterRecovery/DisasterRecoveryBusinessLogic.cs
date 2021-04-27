using Autofac.Features.AttributeFilters;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
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
using ModernSlavery.Infrastructure.Azure.DevOps;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernSlavery.BusinessDomain.DevOps.Testing
{
    public interface IDisasterRecoveryBusinessLogic
    {
        string GetSqlServerName();
        IAsyncEnumerable<string> ListDatabaseBackupsAsync();
        Task<string> CreateDatabaseBacPacAsync(string sourceDatabase);
        Task<string> CreateDatabaseDacPacAsync(string sourceDatabase);
        Task RestoreDatabaseAsync(string storageUri, string targetDatabase);
        Task<bool> DeleteDatabaseBackupAsync(string storageUri);
        Task<Stream> GetBackDownloadAsync(string storageUri);
        IAsyncEnumerable<string> ListDatabasesAsync();
    }

    public class DisasterRecoveryBusinessLogic : IDisasterRecoveryBusinessLogic
    {
        #region Dependencies
        private readonly Core.Options.DevOpsOptions _devOpsOptions;
        private readonly StorageOptions _storageOptions;
        private readonly DatabaseOptions _databaseOptions;
        private readonly SqlManager _sqlManager;
        private readonly IAzureStorageManager _azureStorageManager;

        private readonly string _sqlServerName;
        private readonly string _sqlDatabaseName;
        private readonly string _storageAccessKey;
        private readonly string _administratorLogin;
        private readonly string _administratorPassword;

        private readonly string _backupContainerName;
        #endregion

        #region Constructors
        public DisasterRecoveryBusinessLogic(Core.Options.DevOpsOptions devOpsOptions, StorageOptions storageOptions, DatabaseOptions databaseOptions, SqlManager sqlManager, IAzureStorageManager azureStorageManager)
        {
            _devOpsOptions = devOpsOptions;
            _storageOptions = storageOptions;
            _databaseOptions = databaseOptions;
            _sqlManager = sqlManager;
            _azureStorageManager = azureStorageManager;

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            _sqlServerName = sqlConnectionStringBuilder.DataSource?.AfterFirst(":").BeforeLast(",").BeforeFirst(".");
            _sqlDatabaseName = sqlConnectionStringBuilder.InitialCatalog;
            _administratorLogin = sqlConnectionStringBuilder.UserID;
            _administratorPassword = sqlConnectionStringBuilder.Password;

            _storageAccessKey = _azureStorageManager.AccessKey;
            _backupContainerName = _devOpsOptions.BackupContainer;
        }
        #endregion

        public string GetSqlServerName()
        {
            return _sqlServerName;
        }

        public string GetSqlDatabaseName()
        {
            return _sqlDatabaseName;
        }

        public async IAsyncEnumerable<string> ListDatabasesAsync()
        {
            yield return _sqlDatabaseName;

            var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

            foreach (var database in sqlServer.Databases.List().Where(db=> !db.Name.EqualsI(_sqlDatabaseName,"master")))
                yield return database.Name;
        }

        public async IAsyncEnumerable<string> ListDatabaseBackupsAsync()
        {
            await _azureStorageManager.CreateContainerAsync(_backupContainerName);

            var backups = _azureStorageManager.ListBlobUrisAsync(_backupContainerName);
            await foreach (var backup in backups)
                yield return backup.ToString();
        }

        public async Task<string> CreateDatabaseDacPacAsync(string sourceDatabase)
        {
            var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

            var sqlDatabase = sqlServer?.Databases.List().FirstOrDefault(db => db.Name.EqualsI(sourceDatabase));

            if (sqlDatabase == null) throw new Exception($"Cannot find database '{sourceDatabase}' on server database '{_sqlServerName}'");

            var dirStorageUri = await _azureStorageManager.GetContainerDirectoryUriAsync(_backupContainerName);

            var now = VirtualDateTime.Now;
            var storageUri = Url.Combine(dirStorageUri.ToString().TrimI("/"), $"{_sqlDatabaseName}-{now.ToString("yyyy-M-d-HH-mm")}.dacpac");

            var connectionStringBuilder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            connectionStringBuilder.InitialCatalog = sourceDatabase;
            var dacServiceInstance = new DacServices(connectionStringBuilder.ToString());

            var dacOptions = new DacExtractOptions {
                ExtractAllTableData = true,
                IgnoreExtendedProperties = true,
                IgnorePermissions = true,
                IgnoreUserLoginMappings = true
            };

            using (var blobStream = new MemoryStream())
            {
                dacServiceInstance.Extract(blobStream, sourceDatabase, sourceDatabase, new Version("1.0.0"),extractOptions: dacOptions);
                blobStream.Position = 0;
                await _azureStorageManager.UploadBlobStreamAsync(blobStream, _backupContainerName, new Uri(storageUri));
            }
            return storageUri;
        }

        //public async Task<string> CreateDatabaseBacPacFluentAsync(string sourceDatabase)
        //{
        //    var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

        //    var sqlDatabase = sqlServer?.Databases.List().FirstOrDefault(db => db.Name.EqualsI(sourceDatabase));

        //    if (sqlDatabase == null) throw new Exception($"Cannot find database '{sourceDatabase}' on server database '{_sqlServerName}'");
        //    var dirStorageUri = await _azureStorageManager.GetContainerDirectoryUriAsync(_backupContainerName);
        //    if (dirStorageUri == null) throw new Exception($"Cannot find container '{_backupContainerName}'");

        //    var now = VirtualDateTime.Now;
        //    var storageUri = Url.Combine(dirStorageUri.ToString().TrimI("/"), $"{_sqlDatabaseName}-{now.ToString("yyyy-M-d-HH-mm")}.bacpac");
        //    var result = _sqlManager.CreateBackup(sqlDatabase, storageUri, _storageAccessKey, _administratorLogin, _administratorPassword);
        //    if (result.status != "Completed") throw new Exception($"Status {result.status}: {result.errorMessage}");
        //    return storageUri;
        //}

        public async Task<string> CreateDatabaseBacPacAsync(string sourceDatabase)
        {
            var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

            var sqlDatabase = sqlServer?.Databases.List().FirstOrDefault(db => db.Name.EqualsI(sourceDatabase));

            if (sqlDatabase == null) throw new Exception($"Cannot find database '{sourceDatabase}' on server database '{_sqlServerName}'");

            var dirStorageUri = await _azureStorageManager.GetContainerDirectoryUriAsync(_backupContainerName);

            var now = VirtualDateTime.Now;
            var storageUri = Url.Combine(dirStorageUri.ToString().TrimI("/"), $"{_sqlDatabaseName}-{now.ToString("yyyy-M-d-HH-mm")}.bacpac");

            var connectionStringBuilder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            connectionStringBuilder.InitialCatalog = sourceDatabase;
            var dacServiceInstance = new DacServices(connectionStringBuilder.ToString());

            using (var blobStream = new MemoryStream())
            {
                dacServiceInstance.ExportBacpac(blobStream, sourceDatabase);
                blobStream.Position = 0;
                await _azureStorageManager.UploadBlobStreamAsync(blobStream, _backupContainerName, new Uri(storageUri));
            }
            return storageUri;
        }

        public async Task RestoreDatabaseAsync(string storageUri, string targetDatabase)
        {
            var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

            var sqlDatabase = sqlServer?.Databases.List().FirstOrDefault(db => db.Name.EqualsI(targetDatabase));

            if (sqlDatabase == null) throw new Exception($"Cannot find database '{targetDatabase}' on server database '{_sqlServerName}'");

            if (string.IsNullOrWhiteSpace(storageUri)) throw new ArgumentNullException(nameof(storageUri));
            var extension = Path.GetExtension(storageUri);
            if (extension.EqualsI(".dacpac"))
                await RestoreDatabaseDacPacAsync(storageUri, targetDatabase);
            else if (extension.EqualsI(".bacpac"))
                await RestoreDatabaseBacPacAsync(storageUri, targetDatabase);
            else
                throw new ArgumentException($"Invalid extension '{extension}'. Should be '.dacpac' or '.bacpac'", nameof(storageUri));
        }

        private void RestoreDatabaseBacPac(string storageUri, string targetDatabase)
        {
            if (string.IsNullOrWhiteSpace(storageUri)) throw new ArgumentNullException(nameof(storageUri));
            var extension = Path.GetExtension(storageUri);
            if (!extension.EqualsI(".bacpac")) throw new ArgumentException($"Invalid extension '{extension}'. Should be '.bacpac'", nameof(storageUri));

            var sqlServer = _sqlManager.GetSqlServer(_sqlServerName);

            var sqlDatabase = sqlServer?.Databases.List().FirstOrDefault(db => db.Name.EqualsI(targetDatabase));

            if (sqlDatabase == null) throw new Exception($"Cannot find database '{targetDatabase}' on server database '{_sqlServerName}'");

            var result = _sqlManager.RestoreBackup(sqlDatabase, storageUri, _storageAccessKey, _administratorLogin, _administratorPassword);
            if (result.status != "Completed") throw new Exception($"Status {result.status}: {result.errorMessage}");
        }

        private async Task RestoreDatabaseDacPacAsync(string storageUri, string targetDatabase)
        {
            if (string.IsNullOrWhiteSpace(storageUri)) throw new ArgumentNullException(nameof(storageUri));
            var extension = Path.GetExtension(storageUri);
            if (!extension.EqualsI(".dacpac")) throw new ArgumentException($"Invalid extension '{extension}'. Should be '.dacpac'", nameof(storageUri));

            var dacOptions = new DacDeployOptions();
            dacOptions.BlockOnPossibleDataLoss = false;
            dacOptions.CreateNewDatabase = true;

            var connectionStringBuilder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            connectionStringBuilder.InitialCatalog = targetDatabase;
            var dacServiceInstance = new DacServices(connectionStringBuilder.ToString());

            using var blobStream = await _azureStorageManager.OpenBlobStreamAsync(_backupContainerName, new Uri(storageUri));
            using var dacpac = DacPackage.Load(blobStream);
            dacServiceInstance.Deploy(dacpac, targetDatabase, upgradeExisting: true, options: dacOptions);
        }

        private async Task RestoreDatabaseBacPacAsync(string storageUri, string targetDatabase)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
            connectionStringBuilder.InitialCatalog = targetDatabase;
            var dacServiceInstance = new DacServices(connectionStringBuilder.ToString());

            using var blobStream = await _azureStorageManager.OpenBlobStreamAsync(_backupContainerName, new Uri(storageUri));
            using var bacpac = BacPackage.Load(blobStream);

            dacServiceInstance.ImportBacpac(bacpac, targetDatabase);
        }

        public async Task<Stream> GetBackDownloadAsync(string storageUri)
        {
            return await _azureStorageManager.OpenBlobStreamAsync(_backupContainerName, new Uri(storageUri));
        }

        public async Task<bool> DeleteDatabaseBackupAsync(string storageUri)
        {
            return await _azureStorageManager.DeleteBlobAsync(_backupContainerName, new Uri(storageUri));
        }
    }
}