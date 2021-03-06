﻿using System;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class EmailSendLogModel
    {
        public DateTime Date { get; set; } = VirtualDateTime.Now;
        public string Machine { get; set; } = $"Machine:{Environment.MachineName}";
        public string Source { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        public string Message { get; set; }
        public string Details { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}