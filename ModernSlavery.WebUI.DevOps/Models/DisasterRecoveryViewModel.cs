using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.DevOps.Models
{
    public class DisasterRecoveryViewModel
    {
        public class BackupModel
        {
            public string Name { get; set; }
            public string StorageUri { get; set; }
        }

        public string SqlServerName { get; set; } = "Unknown";
        public string SelectedDatabaseName => SelectedDatabaseIndex > -1 && Databases.Count.Between(1, SelectedDatabaseIndex) ? Databases[SelectedDatabaseIndex] : null;
        public List<string> Backups = new List<string>();
        public List<string> Databases = new List<string>();
        public int SelectedDatabaseIndex { get; set; } = -1;

        public string GetName(int index)
        {
            var backup = Backups.Count<0 || index>=Backups.Count ? null : Backups[index];
            return Path.GetFileName(backup);
        }
    }
}
