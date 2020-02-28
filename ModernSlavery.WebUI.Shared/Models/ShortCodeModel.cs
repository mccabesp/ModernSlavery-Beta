using System;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class ShortCodeModel
    {

        public string ShortCode { get; set; }
        public string Path { get; set; }
        public DateTime? ExpiryDate { get; set; }

    }
}
