using System;
using System.Collections.Generic;

namespace ModernSlavery.Entities
{
    public partial class UserInfoView
    {
        public long UserId { get; set; }
        public string StatusId { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusDetails { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string JobTitle { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactPhoneNumber { get; set; }
        public DateTime? EmailVerifySendDate { get; set; }
        public DateTime? EmailVerifiedDate { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? ResetSendDate { get; set; }
        public int ResetAttempts { get; set; }
        public DateTime? VerifyAttemptDate { get; set; }
        public int VerifyAttempts { get; set; }
    }
}
