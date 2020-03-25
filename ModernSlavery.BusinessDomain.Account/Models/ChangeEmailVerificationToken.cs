using System;

namespace ModernSlavery.BusinessLogic.Account.Models
{
    public class ChangeEmailVerificationToken
    {
        public long UserId { get; set; }

        public string NewEmailAddress { get; set; }

        public DateTime TokenTimestamp { get; set; }
    }
}