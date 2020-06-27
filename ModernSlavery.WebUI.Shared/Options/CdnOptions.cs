using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("CDN")]
    public class CdnOptions: IOptions
    {
        private string _Endpoint;
        public string Endpoint { get=>_Endpoint; set=> _Endpoint= value==null || !value.StartsWithI("http") ? value?.Trim(' ', '/', '\\', ':') : $"https://{value.Trim(' ', '/', '\\', ':')}";}

        public string GetEndpoint(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new NullReferenceException(nameof(relativePath));
            if (string.IsNullOrWhiteSpace(Endpoint)) return relativePath;
            return $"{Endpoint}/{relativePath.Trim(' ', '/', '\\', ':')}";
        }

        public string GetFallback(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new NullReferenceException(nameof(relativePath));
            if (!string.IsNullOrWhiteSpace(Endpoint)) return relativePath;
            return null;
        }
    }
}