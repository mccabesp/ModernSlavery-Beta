using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ModernSlavery.Core;
using Newtonsoft.Json;

namespace ModernSlavery.Database.Models
{
    public class AuditLog
    {

        public long AuditLogId { get; set; }
        public AuditedAction Action { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("OriginalUserId")]
        public virtual User OriginalUser { get; set; }

        [ForeignKey("ImpersonatedUserId")]
        public virtual User ImpersonatedUser { get; set; }

        public string Details { get; set; }

        [NotMapped]
        public Dictionary<string, string> DetailsDictionary
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(string.IsNullOrEmpty(Details) ? "{}" : Details);
            set => Details = JsonConvert.SerializeObject(value);
        }

    }
}
