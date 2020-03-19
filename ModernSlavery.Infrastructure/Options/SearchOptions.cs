using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Storage.Blob.Protocol;
using ModernSlavery.Core.Models;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Options
{
    [Options("Search")]
    public class SearchOptions: IOptions
    {
        public string AzureServiceName { get; set; }
        public string AzureApiAdminKey { get; set; }
        public string AzureApiQueryKey { get; set; }
        public string EmployerIndexName { get; set; } = nameof(EmployerSearchModel);
        public string SicCodeIndexName { get; set; } = nameof(SicCodeSearchModel);
        public bool Disabled { get; set; }
        public bool CacheResults { get; set; }
    }
}
