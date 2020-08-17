using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Hosts.Webjob
{
    [Options("Webjobs")]
    public class WebjobsOptions : Dictionary<String, WebjobsOptions.WebjobOption>, IOptions
    {
        public WebjobsOptions() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
        public bool IsDisabled(string webjobName)
        {
            return !IsEnabled(webjobName);
        }

        public bool IsEnabled(string webjobName)
        {
            var webjob = this.ContainsKey(webjobName) ? this[webjobName] : null;

            if (webjob != null)
                switch (webjob.Action)
                {
                    case WebjobOption.Actions.Disable:
                        return VirtualDateTime.Now < webjob.StartDate || VirtualDateTime.Now > webjob.EndDate;
                    case WebjobOption.Actions.Enable:
                        return VirtualDateTime.Now > webjob.StartDate && VirtualDateTime.Now < webjob.EndDate;
                }

            return true;
        }

        public class WebjobOption
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