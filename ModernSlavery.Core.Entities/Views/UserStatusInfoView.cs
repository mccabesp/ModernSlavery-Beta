using System;

namespace ModernSlavery.Core.Entities.Views
{
    public class UserStatusInfoView
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string StatusId { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusDetails { get; set; }
        public string StatusChangedBy { get; set; }
    }
}