using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Testing.Helpers.Extensions
{

    public static class FileHelper
    {
        public static async Task DeleteDraftsAsync(this IServiceProvider serviceProvider)
        {
            var testBusinessLogic = serviceProvider.GetTestBusinessLogic();
            await testBusinessLogic.DeleteDownloadFilesAsync().ConfigureAwait(false);
        }

        public static async Task DeleteDraftAsync(this IServiceProvider serviceProvider, string organisationIdentifier, long reportingDeadlineYear)
        {
            var testBusinessLogic = serviceProvider.GetTestBusinessLogic();
            await testBusinessLogic.DeleteDraftFilesAsync(organisationIdentifier, reportingDeadlineYear).ConfigureAwait(false);
        }
    }
}
