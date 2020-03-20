using ModernSlavery.SharedKernel.Options;

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


        public string Server2 { get; set; }

        public int Port2 { get; set; } = 25;

        public string Username2 { get; set; }

        public string Password2 { get; set; }
    }
}