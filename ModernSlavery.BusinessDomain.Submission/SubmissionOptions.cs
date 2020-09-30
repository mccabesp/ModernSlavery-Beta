using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ModernSlavery.BusinessDomain.Submission
{
    [Options("Services:Submission")]
    public class SubmissionOptions: IOptions
    {
        public string DraftsPath { get; set; } = "Drafts";
        public int DraftTimeoutMinutes { get; set; } = 20;
        
        /// <summary>
        /// Number of days allowed past deadline to submit (-1 = forever)
        /// </summary>
        public int DeadlineExtensionDays { get; set; }
        /// <summary>
        /// Number of months allowed past deadline to submit (-1 = forever)
        /// </summary>
        public int DeadlineExtensionMonths { get; set; }

        public void Validate() 
        {
            var exceptions = new List<Exception>();
            if (DeadlineExtensionDays == -1 && DeadlineExtensionMonths > 0) exceptions.Add(new ConfigurationErrorsException("Services:Submission:DeadlineExtensionDays cannot be -1 when DeadlineExtensionMonths > 0"));
            if (DeadlineExtensionMonths == -1 && DeadlineExtensionDays > 0) exceptions.Add(new ConfigurationErrorsException("Services:Submission:DeadlineExtensionMonths cannot be -1 when DeadlineExtensionDays > 0"));
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }
    }
}
