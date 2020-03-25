using System;
using System.IO;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    [Serializable]
    public class Draft
    {
        #region Private methods

        private string GetDraftFileName(long organisationId, int snapshotYear, string fileExtension)
        {
            return $"{organisationId}_{snapshotYear}.{fileExtension}";
        }

        #endregion

        #region Constructor

        private Draft()
        {
        }

        public Draft(long organisationId, int snapshotYear, string rootPath)
        {
            DraftFilename = GetDraftFileName(organisationId, snapshotYear, "json");
            DraftPath = Path.Combine(rootPath, DraftFilename);


            BackupDraftFilename = GetDraftFileName(organisationId, snapshotYear, "bak");
            BackupDraftPath = Path.Combine(rootPath, BackupDraftFilename);
        }

        public Draft(long organisationId,
            int snapshotYear,
            bool isUserAllowedAccess,
            DateTime? lastWrittenDateTime,
            long lastWrittenByUserId, string rootPath) :
            this(organisationId, snapshotYear, rootPath)
        {
            IsUserAllowedAccess = isUserAllowedAccess;
            LastWrittenDateTime = lastWrittenDateTime;
            LastWrittenByUserId = lastWrittenByUserId;
        }

        #endregion

        #region Public methods

        public string DraftFilename { get; set; }
        public string DraftPath { get; set; }
        public string BackupDraftFilename { get; set; }
        public string BackupDraftPath { get; set; }
        public bool IsUserAllowedAccess { get; set; }
        public DateTime? LastWrittenDateTime { get; set; }
        public long LastWrittenByUserId { get; set; }
        public ReturnViewModel ReturnViewModelContent { get; set; }
        public bool HasDraftBeenModifiedDuringThisSession { get; set; }

        public bool HasContent()
        {
            return ReturnViewModelContent != null;
        }

        #endregion
    }
}