using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Admin.Models;

namespace ModernSlavery.WebUI.Admin
{
    // Not really a presenter, more like service layer but relies on VM currently
    public interface IAdminHistory
    {
        public Task<DownloadViewModel> GetHistoryLogs();
    }

    public class AdminHistory : IAdminHistory
    {
        readonly IFileRepository FileRepository;
        readonly SharedOptions SharedOptions;

        public AdminHistory(IFileRepository fileRepository, SharedOptions sharedOptions)
        {
            FileRepository = fileRepository;
            SharedOptions = sharedOptions;
        }

        private string GetLogFileSearchPattern(string logFile)
        {
            return logFile.Remove(logFile.LastIndexOf(".csv")) + "*.csv";
        }

        public async Task<DownloadViewModel> GetHistoryLogs()
        {
            var model = new DownloadViewModel();

            //Ensure the log directory exists
            // do we need to do this???
            if (!await FileRepository.GetDirectoryExistsAsync(SharedOptions.LogPath))
                await FileRepository.CreateDirectoryAsync(SharedOptions.LogPath);

            var registrationLogs = GetLogs(Filenames.RegistrationLog, "Registration History", "Audit history of approved and rejected registrations.");
            await foreach (var d in registrationLogs)
            {
                model.Downloads.Add(d);
            }

            var submissionLogs = GetLogs(Filenames.SubmissionLog, "Submission History", "Audit history of approved and rejected registrations.");
            await foreach (var d in submissionLogs)
            {
                model.Downloads.Add(d);
            }

            var manualLogs = GetLogs(Filenames.ManualChangeLog, "Manual Changes", "Audit history of data changes by admins and system ");
            await foreach (var d in manualLogs)
            {
                model.Downloads.Add(d);
            }

            var userLogs = GetLogs(Filenames.UserLog, "User", "Audit history of changes to user details");
            await foreach (var d in userLogs)
            {
                model.Downloads.Add(d);
            }

            var emailLogs = GetLogs(Filenames.EmailSendLog, "Emails", "Audit history of emails send");
            await foreach (var d in emailLogs)
            {
                model.Downloads.Add(d);
            }

            var searchLogs = GetLogs(Filenames.SearchLog, "Search", "Audit history of statement searches");
            await foreach (var d in searchLogs)
            {
                model.Downloads.Add(d);
            }

            var badSicLogs = GetLogs(Filenames.BadSicLog, "Bad Sic", "Audit history of bad SIC codes imported manually or from Companies house");
            await foreach (var d in badSicLogs)
            {
                model.Downloads.Add(d);
            }

            model.Downloads=model.Downloads.OrderByDescending(d => d.Modified).ThenByDescending(d => d.Filename).ToList();

            return model;
        }

        private async IAsyncEnumerable<DownloadViewModel.Download> GetLogs(string fileName, string title, string description)
        {
            var searchPattern = GetLogFileSearchPattern(fileName);
            var files = await FileRepository.GetFilesAsync(SharedOptions.LogPath, searchPattern, true);

            foreach (var filePath in files)
            {
                var download = await CreateDownload(filePath, title, description);

                yield return download;
            }
        }

        private async Task<DownloadViewModel.Download> CreateDownload(string filePath, string title, string description)
        {
            var download = new DownloadViewModel.Download {
                Type = title,
                Filepath = filePath,
                Title = title,
                Description = description
            };

            if (await FileRepository.GetFileExistsAsync(download.Filepath))
                download.Modified = await FileRepository.GetLastWriteTimeAsync(download.Filepath);

            return download;
        }
    }
}
