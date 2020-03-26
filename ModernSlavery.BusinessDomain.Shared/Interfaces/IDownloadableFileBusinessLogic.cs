using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Interfaces.Downloadable;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IDownloadableFileBusinessLogic
    {
        Task<DownloadableFileModel> GetFileRemovingSensitiveInformationAsync(string filePath);

        Task<IEnumerable<IDownloadableItem>> GetListOfDownloadableItemsFromPathAsync(
            string processedLogsPath);
    }
}