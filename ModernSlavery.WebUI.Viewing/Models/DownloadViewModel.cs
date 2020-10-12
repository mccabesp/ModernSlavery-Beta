using System;
using System.Collections.Generic;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class DownloadViewModel
    {
        public string BackUrl { get; set; }
        public List<Download> Downloads { get; set; }

        public class Download
        {
            public string Title { get; set; }
            public string Count { get; set; }
            public string Size { get; set; }
            public string Url { get; set; }
            public string Extension { get; set; }
        }
    }
}