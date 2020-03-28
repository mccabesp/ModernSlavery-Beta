using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("DataProtection")]
    public class BasicAuthenticationOptions : IOptions
    {
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
