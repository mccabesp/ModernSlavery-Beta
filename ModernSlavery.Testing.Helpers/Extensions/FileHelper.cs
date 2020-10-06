﻿using Microsoft.Extensions.Hosting;
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
        public static async Task DeleteDraftsAsync(this IServiceProvider serviceProvider)
        {
            var fileRepository = serviceProvider.GetFileRepository();
            var submissionOptions = serviceProvider.GetService<SubmissionOptions>();

            if (!await fileRepository.GetDirectoryExistsAsync(submissionOptions.DraftsPath)) return;

            var files = await fileRepository.GetFilesAsync(submissionOptions.DraftsPath);
            foreach (var file in files)
            {
                await fileRepository.DeleteFileAsync(file);
            }
        }

        public static async Task DeleteDraftAsync(this IServiceProvider serviceProvider, string organisationIdentifier, long reportingDeadlineYear)
        {
            var fileRepository = serviceProvider.GetFileRepository();
            var submissionOptions = serviceProvider.GetService<SubmissionOptions>();

            var obfuscator = serviceProvider.GetService<IObfuscator>();
            long organisationId = obfuscator.DeObfuscate(organisationIdentifier);

            var filePattern = $"{organisationId}_{reportingDeadlineYear}.*";

            var files = await fileRepository.GetFilesAsync(submissionOptions.DraftsPath, filePattern);
            foreach (var file in files)
            {
                await fileRepository.DeleteFileAsync(file);
            }
        }
    }
}
