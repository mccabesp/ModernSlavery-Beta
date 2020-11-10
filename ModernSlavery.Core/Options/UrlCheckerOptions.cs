using ModernSlavery.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ModernSlavery.Core.Options
{
    [Options("UrlChecker")]
    public class UrlCheckerOptions : IOptions
    {
        /// <summary>
        /// The timeout in seconds for checking a url.
        /// </summary>
        public int Timeout { get; set; }

        public void Validate()
        {
            if (Timeout <= 0)
                throw new ConfigurationErrorsException("UrlChecker:Timeout must be greater than 0");
        }
    }
}
