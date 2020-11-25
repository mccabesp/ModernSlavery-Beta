using System;
using System.Collections.Generic;
using System.Configuration;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Messaging
{
    [Options("Email:Providers:Smtp")]
    public class SmtpEmailOptions : IOptions
    {
        public bool? Enabled { get; set; } = true;

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string ReplyEmail { get; set; }

        public string Server { get; set; }

        public int Port { get; set; } = 25;

        public string Username { get; set; }

        public string Password { get; set; }

        public void Validate() 
        {
            if (Enabled==false) return;

            var exceptions = new List<Exception>();

            if (string.IsNullOrWhiteSpace(SenderEmail)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(SenderEmail)}"));
            if (string.IsNullOrWhiteSpace(Server)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(Server)}"));
            if (string.IsNullOrWhiteSpace(Username)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(Username)}"));
            if (string.IsNullOrWhiteSpace(Password)) exceptions.Add(new ConfigurationErrorsException($"Missing {nameof(Password)}"));

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

    }
}