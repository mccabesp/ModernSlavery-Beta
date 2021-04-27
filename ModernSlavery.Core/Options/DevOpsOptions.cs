using ModernSlavery.Core.Attributes;

namespace ModernSlavery.Core.Options
{
    [Options("DevOps")]
    public class DevOpsOptions : IOptions
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string TenantId { get; set; }

        public string SubscriptionId { get; set; }

        public string BackupContainer { get; set; } = "sql-backups";

        public bool AllowTriggerWebjobs { get; set; }
        public bool AllowDatabaseBackup { get; set; }
        public bool AllowBackupDownload { get; set; }
        public bool AllowBackupDelete { get; set; }

        public bool AllowDatabaseRestore { get; set; }
        public bool AllowDatabaseReset { get; set; }
        public bool AllowDeleteDownloadFiles { get; set; }
        public bool AllowDeleteDraftFiles { get; set; }
        public bool AllowDeleteAuditLogFiles { get; set; }
        public bool AllowClearQueues { get; set; }
        public bool AllowClearAppInsights { get; set; }
        public bool AllowClearCache { get; set; }
        public bool AllowResetSearch { get; set; }
        public bool AllowDeleteLocalLogs { get; set; }
        public bool AllowDeleteSettingsDump { get; set; }

        public bool HasCredentials()
        {
            return !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret) && !string.IsNullOrWhiteSpace(TenantId);
        }
        public void Validate()
        {

        }
    }
}
