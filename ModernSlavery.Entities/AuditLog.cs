using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ModernSlavery.Entities.Enums;
using Newtonsoft.Json;

namespace ModernSlavery.Entities
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
