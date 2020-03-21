using System;

namespace ModernSlavery.WebUI.Models.Register
{
    [Serializable]
    public class VerifyViewModel
    {

        public long UserId { get; set; }
        public bool Resend { get; set; }
        public string EmailAddress { get; set; }
        public bool Sent { get; set; }

    }
}
