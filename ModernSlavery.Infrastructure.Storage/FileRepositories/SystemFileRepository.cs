using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Storage.FileRepositories
{
    public class SystemFileRepository : IFileRepository
    {
        private readonly DirectoryInfo _rootDir;
        private readonly StorageOptions _storageOptions;

        public SystemFileRepository(StorageOptions storageOptions)
        {
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
            if (string.IsNullOrWhiteSpace(storageOptions.LocalStorageRoot))
                throw new ArgumentNullException(nameof(storageOptions.LocalStorageRoot));

            var rootPath = string.IsNullOrWhiteSpace(storageOptions.LocalStorageRoot)
                ? AppDomain.CurrentDomain.BaseDirectory
                : FileSystem.ExpandLocalPath(storageOptions.LocalStorageRoot);
            _rootDir = new DirectoryInfo(storageOptions.LocalStorageRoot);
        }

        public string RootDir => _rootDir.FullName;

        public async Task<IEnumerable<string>> GetDirectoriesAsync(string directoryPath,
            string searchPattern = null,
            bool recursive = false)
        {
            return await Task.Run(() => GetDirectories(directoryPath, searchPattern = null, recursive));
        }

        public async Task<bool> GetFileExistsAsync(string filePath)
        {
            return await Task.Run(() => GetFileExists(filePath));
        }

        public async Task CreateDirectoryAsync(string directoryPath)
        {
            await Task.Run(() => CreateDirectory(directoryPath));
        }

        public async Task<bool> GetDirectoryExistsAsync(string directoryPath)
        {
            return await Task.Run(() => GetDirectoryExists(directoryPath));
        }

        public async Task<DateTime> GetLastWriteTimeAsync(string filePath)
        {
            return await Task.Run(() => GetLastWriteTime(filePath));
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            return await Task.Run(() => GetFileSize(filePath));
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await Task.Run(() => DeleteFile(filePath));
        }

        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            await Task.Run(() => CopyFile(sourceFilePath, destinationFilePath, overwrite));
        }

        public async Task<IEnumerable<string>> GetFilesAsync(string directoryPath, string searchPattern = null,
            bool recursive = false)
        {
            return await Task.Run(() => GetFiles(directoryPath, searchPattern, recursive));
        }

        public async Task<bool> GetAnyFileExistsAsync(string directoryPath, string searchPattern = null,
            bool recursive = false)
        {
            return await Task.Run(() => GetAnyFileExists(directoryPath, searchPattern, recursive));
        }

        public async Task<string> ReadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return await Task.Run(() => System.IO.File.ReadAllText(filePath));
        }

        public async Task ReadAsync(string filePath, Stream stream)
        {
            await Task.Run(() => Read(filePath, stream));
        }

        public async Task<byte[]> ReadBytesAsync(string filePath)
        {
            return await Task.Run(() => ReadBytes(filePath));
        }

        public async Task<DataTable> ReadDataTableAsync(string filePath)
        {
            var fileContent = await GetFilesAsync(filePath);
            return fileContent.FirstOrDefault()?.ToDataTable();
        }


        public async Task AppendAsync(string filePath, string text)
        {
            await Task.Run(() => Append(filePath, text));
        }

        public async Task WriteAsync(string filePath, byte[] bytes)
        {
            await Task.Run(() => Write(filePath, bytes));
        }

        public async Task WriteAsync(string filePath, Stream stream)
        {
            await Task.Run(() => Write(filePath, stream));
        }

        public async Task WriteAsync(string filePath, FileInfo uploadFile)
        {
            await Task.Run(() => Write(filePath, uploadFile));
        }

        public string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return filePath;
        }

        public async Task<IDictionary<string, string>> LoadMetaDataAsync(string filePath)
        {
            return await Task.Run(() => LoadMetaData(filePath));
        }

        public async Task<string> GetMetaDataAsync(string filePath, string key)
        {
            return await Task.Run(() => GetMetaData(filePath, key));
        }

        public Task SetMetaDataAsync(string filePath, string key, string value)
        {
            var metaData = LoadMetaData(filePath);

            if (metaData.ContainsKey(key) && metaData[key] == value) return Task.CompletedTask;

            if (!string.IsNullOrWhiteSpace(value))
                metaData[key] = value;
            else if (metaData.ContainsKey(key)) metaData.Remove(key);

            SaveMetaData(filePath, metaData);

            return Task.CompletedTask;
        }

        private IEnumerable<string> GetDirectories(string directoryPath, string searchPattern = null,
            bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                directoryPath = _rootDir.FullName;
            else if (!Path.IsPathRooted(directoryPath))
                directoryPath = Path.Combine(_rootDir.FullName, directoryPath);
            else if (directoryPath == "\\") directoryPath = _rootDir.FullName;

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var results = string.IsNullOrWhiteSpace(searchPattern)
                ? Directory.GetDirectories(directoryPath, "*.*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : Directory.GetDirectories(
                    directoryPath,
                    searchPattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            return results;
        }

        private bool GetFileExists(string filePath)

        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return System.IO.File.Exists(filePath);
        }

        private void CreateDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);

            var retries = 0;
            Retry:
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private bool GetDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);

            return Directory.Exists(directoryPath);
        }

        private DateTime GetLastWriteTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return System.IO.File.GetLastWriteTime(filePath);
        }

        private long GetFileSize(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return new FileInfo(filePath).Length;
        }

        private void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            var retries = 0;
            Retry:
            try
            {
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                var metaPath = filePath + ".metadata";
                if (System.IO.File.Exists(metaPath)) System.IO.File.Delete(metaPath);
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private void CopyFile(string sourceFilePath, string destinationFilePath, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath)) throw new ArgumentNullException(nameof(sourceFilePath));

            if (!Path.IsPathRooted(sourceFilePath)) sourceFilePath = Path.Combine(_rootDir.FullName, sourceFilePath);

            if (!System.IO.File.Exists(sourceFilePath))
                throw new FileNotFoundException($"Cannot find source file '{sourceFilePath}'");

            if (string.IsNullOrWhiteSpace(destinationFilePath))
                throw new ArgumentNullException(nameof(destinationFilePath));

            if (!Path.IsPathRooted(destinationFilePath))
                destinationFilePath = Path.Combine(_rootDir.FullName, destinationFilePath);

            if (System.IO.File.Exists(destinationFilePath) && !overwrite)
                throw new Exception($"Destination file '{destinationFilePath}' exists.");

            var retries = 0;
            Retry:
            try
            {
                System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                var sourceFileMetaPath = sourceFilePath + ".metadata";
                var destinationFileMetaPath = destinationFilePath + ".metadata";

                if (System.IO.File.Exists(sourceFileMetaPath))
                    System.IO.File.Copy(sourceFileMetaPath, destinationFileMetaPath, true);
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private IEnumerable<string> GetFiles(string filePath, string searchPattern = null, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            if (!Directory.Exists(filePath))
                throw new DirectoryNotFoundException($"Cannot find directory '{filePath}'");

            var results = string.IsNullOrWhiteSpace(searchPattern)
                ? Directory.GetFiles(filePath, "*.*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : Directory.GetFiles(filePath, searchPattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            for (var i = 0; i < results.Length; i++)
                if (results[i].StartsWithI(_rootDir.FullName))
                    results[i] = results[i].Substring(_rootDir.FullName.Length);

            return results;
        }

        private bool GetAnyFileExists(string directoryPath, string searchPattern = null, bool recursive = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var results = string.IsNullOrWhiteSpace(searchPattern)
                ? Directory.GetFiles(directoryPath, "*.*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : Directory.GetFiles(directoryPath, searchPattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            return results.Any();
        }

        private void Read(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            using (Stream fs = System.IO.File.OpenRead(filePath))
            {
                fs.CopyTo(stream);
            }
        }

        private byte[] ReadBytes(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return System.IO.File.ReadAllBytes(filePath);
        }

        private void Append(string filePath, string text)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            //Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var retries = 0;
            Retry:
            try
            {
                System.IO.File.AppendAllText(filePath, text);
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private void Write(string filePath, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            //Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var retries = 0;
            Retry:
            try
            {
                System.IO.File.WriteAllBytes(filePath, bytes);
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private void Write(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            //Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var retries = 0;
            Retry:
            try
            {
                using (var fs = System.IO.File.OpenWrite(filePath))
                {
                    stream.CopyTo(fs);
                }
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private void Write(string filePath, FileInfo uploadFile)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            if (!uploadFile.Exists) throw new FileNotFoundException(nameof(uploadFile));

            //Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var retries = 0;
            Retry:
            try
            {
                System.IO.File.WriteAllText(filePath, System.IO.File.ReadAllText(uploadFile.FullName));
            }
            catch (IOException)
            {
                if (retries >= 10) throw;

                retries++;
                Thread.Sleep(500);
                goto Retry;
            }
        }

        private IDictionary<string, string> LoadMetaData(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException("Cant find file", filePath);

            var metaPath = filePath + ".metadata";
            var metaJson = System.IO.File.Exists(metaPath) ? System.IO.File.ReadAllText(metaPath) : null;
            if (!string.IsNullOrWhiteSpace(metaJson))
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaJson);
                return new Dictionary<string, string>(result, StringComparer.OrdinalIgnoreCase);
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private void SaveMetaData(string filePath, IDictionary<string, string> metaData)

        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            var metaPath = filePath + ".metadata";

            string metaJson = null;
            if (metaData != null && metaData.Count > 0 && metaData.Values.Any(kv => !string.IsNullOrWhiteSpace(kv)))
                metaJson = JsonConvert.SerializeObject(metaData);

            if (!string.IsNullOrWhiteSpace(metaJson))
                System.IO.File.WriteAllText(metaPath, metaJson);
            else if (System.IO.File.Exists(metaPath)) System.IO.File.Delete(metaPath);
        }

        private string GetMetaData(string filePath, string key)
        {
            var metaData = LoadMetaData(filePath);
            return metaData.ContainsKey(key) ? metaData[key] : null;
        }
    }
}