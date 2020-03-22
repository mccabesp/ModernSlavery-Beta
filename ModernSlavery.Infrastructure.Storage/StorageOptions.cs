using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Storage
{
    [Options("Security")]
    public class StorageOptions : IOptions
    {
        public string AzureConnectionString { get; set; }
        public string AzureShareName { get; set; }
        public string LocalStorageRoot { get; set; }
    }
}