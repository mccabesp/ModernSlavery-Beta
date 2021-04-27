using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.Core.Entities
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }

        public AuditedAction Action { get; set; }
        public long? OrganisationId { get; set; }
        public long? OriginalUserId { get; set; }
        public long? ImpersonatedUserId { get; set; }
        public DateTime CreatedDate { get; set; } = VirtualDateTime.Now;
        
        public string Details { get; set; }

        public virtual User ImpersonatedUser { get; set; }
        public virtual Organisation Organisation { get; set; }
        public virtual User OriginalUser { get; set; }

        [NotMapped]
        public Dictionary<string, string> DetailsDictionary
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(string.IsNullOrEmpty(Details)
                ? "{}"
                : Details);
            set => Details = Json.SerializeObject(value);
        }
    }
}