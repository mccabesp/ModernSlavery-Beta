using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Submission;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ModernSlavery.Testing.Helpers.Extensions
{

    public static class FileHelper
    {
        public static async Task DeleteDraftsAsync(this IHost host)
        {
            var fileRepository = host.GetFileRepository();
            var submissionOptions = host.Services.GetService<SubmissionOptions>();

            if (!await fileRepository.GetDirectoryExistsAsync(submissionOptions.DraftsPath)) return;

            var files = await fileRepository.GetFilesAsync(submissionOptions.DraftsPath);
            foreach (var file in files)
            {
                await fileRepository.DeleteFileAsync(file);
            }
        }

        public static async Task DeleteDraftAsync(this IHost host, string organisationIdentifier, long reportingDeadlineYear)
        {
            var fileRepository = host.GetFileRepository();
            var submissionOptions = host.Services.GetService<SubmissionOptions>();
            
            var obfuscator = host.Services.GetService<IObfuscator>();
            long organisationId = obfuscator.DeObfuscate(organisationIdentifier);

            var filePattern =$"{organisationId}_{reportingDeadlineYear}.*";

            var files = await fileRepository.GetFilesAsync(submissionOptions.DraftsPath, filePattern);
            foreach (var file in files)
            {
                await fileRepository.DeleteFileAsync(file);
            }
        }
    }
}
