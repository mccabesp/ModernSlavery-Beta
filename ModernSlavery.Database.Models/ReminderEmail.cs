using System;
using ModernSlavery.Core;

namespace ModernSlavery.Database.Models
{
    public class ReminderEmail
    {
        public long ReminderEmailId { get; set; }
        
        public long UserId { get; set; }
        
        public SectorTypes SectorType { get; set; }
        
        public DateTime DateSent { get; set; }
    }
}
