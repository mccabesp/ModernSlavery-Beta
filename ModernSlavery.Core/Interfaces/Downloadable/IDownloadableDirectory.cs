using System.Collections.Generic;

namespace ModernSlavery.Core.Interfaces.Downloadable
{
    public interface IDownloadableDirectory : IDownloadableItem
    {

        List<IDownloadableItem> DownloadableItems { get; }

    }
}
