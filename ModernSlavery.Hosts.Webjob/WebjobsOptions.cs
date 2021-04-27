using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Hosts.Webjob
{
    [Options("WebJobs")]
    public class WebJobsOptions : Dictionary<String, WebJobsOptions.WebJobOption>, IOptions
    {
        public WebJobsOptions() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
        public bool IsDisabled(string webjobName)
        {
            return !IsEnabled(webjobName);
        }

        public int KeepConsoleLogDays { get; set; } = -1;

        public bool IsEnabled(string webjobName)
        {
            var webjob = this.ContainsKey(webjobName) ? this[webjobName] : null;

            if (webjob != null)
                switch (webjob.Action)
                {
                    case WebJobOption.Actions.Disable:
                        return VirtualDateTime.Now < webjob.StartDate || VirtualDateTime.Now > webjob.EndDate;
                    case WebJobOption.Actions.Enable:
                        return VirtualDateTime.Now > webjob.StartDate && VirtualDateTime.Now < webjob.EndDate;
                }

            return true;
        }

        public class WebJobOption
        {
            public enum Actions : byte
            {
                Enable = 0,
                Disable = 1
            }
            public Actions? Action { get; set; }
            public DateTime StartDate { get; set; } = DateTime.MinValue;
            public DateTime EndDate { get; set; } = DateTime.MaxValue;

            public string Trigger { get; set; }
        }
    }
}