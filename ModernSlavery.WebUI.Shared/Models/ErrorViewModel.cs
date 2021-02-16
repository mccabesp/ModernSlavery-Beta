﻿using System;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Models
{
    [Serializable]
    public class ErrorViewModel
    {
        public int ErrorCode { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string CallToAction { get; set; }
        public string ActionText { get; set; } = "Continue";
        public string ActionUrl { get; set; }

        public int ResponseCode => ErrorCode.Between(400, 599) ? ErrorCode : 500;

    }
}