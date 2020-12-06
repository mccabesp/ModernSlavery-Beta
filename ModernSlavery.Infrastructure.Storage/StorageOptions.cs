using System.IO;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Storage
{
    [Options("Storage")]
    public class StorageOptions : IOptions
    {
        public string AzureConnectionString { get; set; }
        public string AzureShareName { get; set; }
        public string LocalStorageRoot { get; set; }

        public void Validate() 
        {
            LocalStorageRoot = string.IsNullOrWhiteSpace(LocalStorageRoot) ? null : Path.GetFullPath(LocalStorageRoot);
        }
    }
}