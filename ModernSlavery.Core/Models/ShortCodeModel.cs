using System;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class ShortCodeModel
    {
        public string ShortCode { get; set; }
        public string Path { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}