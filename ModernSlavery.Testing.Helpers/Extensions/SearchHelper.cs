using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Search;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class SearchHelper
    {
         /// <summary>
         /// Deletes and recreated the latest search index and updates with latest submission information
         /// </summary>
         /// <param name="host"></param>
        public static async Task ResetSearchIndexesAsync(this IHost host)
        {
            var testBusinessLogic = host.Services.GetTestBusinessLogic();
            //Delete and recreate the search index
            await testBusinessLogic.ResetSearchIndexesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Updates with latest submission information to the search index
        /// </summary>
        /// <param name="host"></param>
        public static async Task RefreshSearchDocuments(this IHost host)
        {
            //Recreate the search documents
            var testBusinessLogic = host.Services.GetTestBusinessLogic();
            await testBusinessLogic.RefreshSearchDocumentsAsync().ConfigureAwait(false);
        }
    }
}
