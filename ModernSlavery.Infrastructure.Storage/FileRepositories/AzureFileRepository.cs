using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Core;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;

namespace ModernSlavery.Infrastructure.Storage.FileRepositories
{
    public class AzureFileRepository : IFileRepository
    {
        private readonly CloudFileDirectory _rootDir;

        private readonly StorageOptions _storageOptions;

        public AzureFileRepository(StorageOptions storageOptions, IRetryPolicy retryPolicy = null)
        {
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
            if (string.IsNullOrWhiteSpace(storageOptions.AzureConnectionString))
                throw new ArgumentNullException(nameof(storageOptions.AzureConnectionString));
            if (string.IsNullOrWhiteSpace(storageOptions.AzureShareName))
                throw new ArgumentNullException(nameof(storageOptions.AzureShareName));

            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(storageOptions.AzureConnectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            var fileClient = storageAccount.CreateCloudFileClient();
            fileClient.DefaultRequestOptions.RetryPolicy =
                retryPolicy ?? new LinearRetry(TimeSpan.FromMilliseconds(500), 10); //Maximum of 5 second wait 

            var share = fileClient.GetShareReference(storageOptions.AzureShareName);

            _rootDir = share.GetRootDirectoryReference();
        }

        public string RootDir => _rootDir.Name;

        public async Task<IEnumerable<string>> GetDirectoriesAsync(string directoryPath,
            string searchPattern = null,
            bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) directoryPath = _rootDir.Name;

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            var directory = await GetDirectoryAsync(directoryPath);
            if (directory == null || !await directory.ExistsAsync())
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var token = new FileContinuationToken();
            var items = await directory.ListFilesAndDirectoriesSegmentedAsync(token);

            var directories = new List<string>();

            foreach (var fileDir in items.Results)
                if (fileDir is CloudFileDirectory)
                {
                    var dir = (CloudFileDirectory) fileDir;
                    if (string.IsNullOrWhiteSpace(searchPattern) || dir.Name.Like(searchPattern))
                        directories.Add(Url.Combine(directoryPath, dir.Name));

                    if (recursive)
                        directories.AddRange(await GetDirectoriesAsync(Url.Combine(directoryPath, dir.Name),
                            searchPattern, recursive));
                }

            return directories;
        }

        public async Task CreateDirectoryAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            directoryPath = directoryPath.TrimI(@"/\");
            var dirs = directoryPath.SplitI(@"/\");
            if (dirs.Length < 1) return;

            var directory = _rootDir;

            foreach (var dir in dirs)
            {
                var file = directory.GetFileReference(dir);
                if (file != null && await file.ExistsAsync()) return;

                directory = directory.GetDirectoryReference(dir);
                if (directory != null) await directory.CreateIfNotExistsAsync();
            }
        }

        public async Task<bool> GetDirectoryExistsAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            var dir = await GetDirectoryAsync(directoryPath);
            return dir != null && await dir.ExistsAsync();
        }

        public async Task<bool> GetFileExistsAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var directory = await GetDirectoryAsync(Url.GetDirectoryName(filePath));
            if (directory == null || !await directory.ExistsAsync()) return false;

            var file = await GetFileAsync(filePath);
            return file != null && await file.ExistsAsync();
        }

        public async Task<DateTime> GetLastWriteTimeAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'");

            return file.Properties.LastModified.Value.LocalDateTime;
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'");

            await file.FetchAttributesAsync();
            return file.Properties.Length;
        }


        public async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            await file?.DeleteIfExistsAsync();
            var metaPath = filePath + ".metadata";
            var metaFile = await GetFileAsync(metaPath);
            await metaFile?.DeleteIfExistsAsync();
        }

        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            sourceFilePath = Url.DirToUrlSeparator(sourceFilePath);
            destinationFilePath = Url.DirToUrlSeparator(destinationFilePath);

            var sourceCloudFile = await GetFileAsync(sourceFilePath);
            if (sourceCloudFile == null || !await sourceCloudFile.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{sourceFilePath}'");

            var destinationCloudFile = await GetFileAsync(destinationFilePath);
            if (!overwrite && await destinationCloudFile.ExistsAsync())
                throw new FileNotFoundException($"Destination file already exists '{destinationFilePath}'");

            await DeleteFileAsync(destinationFilePath);

            await destinationCloudFile.StartCopyAsync(sourceCloudFile);
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null,
            bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            var directory = await GetDirectoryAsync(directoryPath);
            if (directory == null || !await directory.ExistsAsync())
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var files = new List<string>();
            var token = new FileContinuationToken();
            var items = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (var fileDir in items.Results)
                if (fileDir is CloudFile)
                {
                    var file = (CloudFile) fileDir;
                    if (string.IsNullOrWhiteSpace(searchPattern) || file.Name.Like(searchPattern))
                        files.Add(Url.Combine(directoryPath, file.Name));
                }
                else if (recursive)
                {
                    var dir = (CloudFileDirectory) fileDir;
                    files.AddRange(await GetFilesAsync(Url.Combine(directoryPath, dir.Name), searchPattern, recursive));
                }

            return files;
        }

        public async Task<bool> GetAnyFileExistsAsync(string directoryPath, string searchPattern = null,
            bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            var directory = await GetDirectoryAsync(directoryPath);
            if (directory == null || !await directory.ExistsAsync())
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var files = new List<string>();
            var token = new FileContinuationToken();
            var items = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
            foreach (var fileDir in items.Results)
                if (fileDir is CloudFile)
                {
                    var file = (CloudFile) fileDir;
                    if (string.IsNullOrWhiteSpace(searchPattern) || file.Name.Like(searchPattern)) return true;
                }
                else if (recursive)
                {
                    var dir = (CloudFileDirectory) fileDir;
                    if (await GetAnyFileExistsAsync(Url.Combine(directoryPath, dir.Name), searchPattern, recursive))
                        return true;
                }

            return false;
        }

        public async Task<string> ReadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'");

            return await file.DownloadTextAsync();
        }

        public async Task ReadAsync(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'");

            await file.DownloadToStreamAsync(stream);
        }

        public async Task<byte[]> ReadBytesAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'");

            await file.FetchAttributesAsync();
            var bytes = new byte[file.Properties.Length];
            await file.DownloadToByteArrayAsync(bytes, 0);
            return bytes;
        }

        public async Task<DataTable> ReadDataTableAsync(string filePath)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            var fileContent = await ReadAsync(filePath);
            return fileContent.ToDataTable();
        }

        public async Task AppendAsync(string filePath, string text)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

            //Ensure the directory exists
            var directory = Url.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !await GetDirectoryExistsAsync(directory))
                await CreateDirectoryAsync(directory);

            var buffer = Encoding.UTF8.GetBytes(text);
            var file = await GetFileAsync(filePath);
            if (await file.ExistsAsync())
            {
                await file.FetchAttributesAsync();
                await file.ResizeAsync(file.Properties.Length + buffer.Length);
                using (var fileStream = await file.OpenWriteAsync(null))
                {
                    fileStream.Seek(buffer.Length * -1, SeekOrigin.End);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                await file.UploadFromByteArrayAsync(buffer, 0, buffer.Length);
            }

            await file.FetchAttributesAsync();
        }

        public async Task WriteAsync(string filePath, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            //Ensure the directory exists
            var directory = Url.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !await GetDirectoryExistsAsync(directory))
                await CreateDirectoryAsync(directory);

            var file = await GetFileAsync(filePath);

            var stream = new SyncMemoryStream(bytes, 0, bytes.Length);
            await file.UploadFromStreamAsync(stream);
        }

        public async Task WriteAsync(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            //Ensure the directory exists
            var directory = Url.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !await GetDirectoryExistsAsync(directory))
                await CreateDirectoryAsync(directory);

            var file = await GetFileAsync(filePath);
            await file.UploadFromStreamAsync(stream);
        }

        public async Task WriteAsync(string filePath, FileInfo uploadFile)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            if (!uploadFile.Exists) throw new FileNotFoundException(nameof(uploadFile));

            //Ensure the directory exists
            var directory = Url.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !await GetDirectoryExistsAsync(directory))
                await CreateDirectoryAsync(directory);

            var file = await GetFileAsync(filePath);

            try
            {
                await file.UploadFromFileAsync(uploadFile.FullName);
            }
            catch (Exception ex)
            {
#warning Remove this after bug 'The specifed resource name contains invalid characters.' fixed
                throw new Exception(
                    $"{nameof(filePath)}:'{filePath}', {nameof(uploadFile.FullName)}:'{uploadFile.FullName}'", ex);
            }
        }

        public string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Url.Combine(_rootDir.Name, filePath);

            filePath = Url.DirToUrlSeparator(filePath);
            return filePath;
        }

        public async Task<IDictionary<string, string>> LoadMetaDataAsync(string filePath)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);

            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'", filePath);

            return file.Metadata;
        }

        public async Task<string> GetMetaDataAsync(string filePath, string key)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            var metaData = await LoadMetaDataAsync(filePath);
            return metaData.ContainsKey(key) ? metaData[key] : null;
        }

        public async Task SetMetaDataAsync(string filePath, string key, string value)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            var metaData = await LoadMetaDataAsync(filePath);

            if (metaData.ContainsKey(key) && metaData[key] == value) return;

            if (!string.IsNullOrWhiteSpace(value))
                metaData[key] = value;
            else if (metaData.ContainsKey(key)) metaData.Remove(key);

            await SaveMetaDataAsync(filePath, metaData);
        }

        public async Task SaveMetaDataAsync(string filePath, IDictionary<string, string> metaData)
        {
            filePath = Url.DirToUrlSeparator(filePath);

            var file = await GetFileAsync(filePath);
            if (file == null || !await file.ExistsAsync())
                throw new FileNotFoundException($"Cannot find file '{filePath}'", filePath);

            //Set the new values
            foreach (var key in metaData.Keys) file.Metadata[key] = metaData[key];

            //Remove the old values
            foreach (var key in file.Metadata.Keys.Except(metaData.Keys)) file.Metadata.Remove(key);

            await file.SetMetadataAsync();
        }


        private async Task<CloudFileDirectory> GetDirectoryAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            directoryPath = Url.DirToUrlSeparator(directoryPath);

            directoryPath = directoryPath.TrimI(@"/\");
            return string.IsNullOrWhiteSpace(directoryPath) ? _rootDir : _rootDir.GetDirectoryReference(directoryPath);
        }

        private async Task<CloudFile> GetFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            filePath = Url.DirToUrlSeparator(filePath);

            filePath = filePath.TrimI(@"/\");
            return _rootDir.GetFileReference(filePath);
        }
    }
}