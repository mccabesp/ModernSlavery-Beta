using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.BusinessDomain.Submission
{
    [Options("Services:Submission")]
    public class SubmissionOptions: IOptions
    {
        public string DraftsPath { get; set; } = "Drafts";
        public int DraftTimeoutMinutes { get; set; } = 20;
        public int DeadlineExtensionDays { get; set; } = 90;

    }
}
