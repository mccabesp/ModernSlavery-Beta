﻿using System;
using ModernSlavery.Extensions;
using ModernSlavery.Entities.Enums;

namespace ModernSlavery.Entities
{
    public class UserStatus
    {

        public long UserStatusId { get; set; }
        public long UserId { get; set; }
        public UserStatuses Status { get; set; }
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual User User { get; set; }

    }
}
