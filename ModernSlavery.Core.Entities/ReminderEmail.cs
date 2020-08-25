using ModernSlavery.Core.Extensions;
using System;

namespace ModernSlavery.Core.Entities
{
    public class ReminderEmail
    {
        public long ReminderEmailId { get; set; }

        public long UserId { get; set; }
        public virtual User User { get; set; }

        public SectorTypes SectorType { get; set; }

        public DateTime DateSent { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}