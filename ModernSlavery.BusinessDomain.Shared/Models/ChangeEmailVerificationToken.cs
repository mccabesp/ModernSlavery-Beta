using System;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class ChangeEmailVerificationToken
    {
        public long UserId { get; set; }

        public string NewEmailAddress { get; set; }

        public DateTime TokenTimestamp { get; set; }
    }
}