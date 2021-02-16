using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModernSlavery.WebUI.Account.Models
{
    [Serializable]
    public class VerifyViewModel
    {
        public long UserId { get; set; }
        public bool Resend { get; set; }
        [BindNever]public string EmailAddress { get; set; }
        public bool Sent { get; set; }
    }
}