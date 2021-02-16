using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Admin.Models
{
    [Serializable]
    public class UploadViewModel
    {
        public List<Upload> Uploads { get; set; } = new List<Upload>();

        [Serializable]
        public class Upload
        {
            [BindNever] public string Type { get; set; }
            [BindNever] public string Title { get; set; }
            [BindNever] public string Description { get; set; }

            [BindNever]public string Filename => Path.GetFileName(Filepath);

            [BindNever] public string Filepath { get; set; }
            public DateTime Modified { get; set; }

            public int DatabaseCount { get; set; }
            public bool FileExists { get; set; }
        }
    }
}