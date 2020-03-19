using System;
using System.Linq;
using ModernSlavery.Extensions;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("Features")]

    public class FeatureSwitchOptions : IOptions
    {
        public class Feature
        {
            public enum Actions
            {
                Disable,
                Enable
            }

            public string Name { get; set; }

            public Actions Action { get; set; }
            public DateTime StartDate { get; set; } = DateTime.MinValue;
            public DateTime EndDate { get; set; } = DateTime.MaxValue;
        }

        public Feature[] Features { get; set; }

        public bool IsDisabled(string featureName) => !IsEnabled(featureName);

        public bool IsEnabled(string featureName)
        {
            var feature = Features?.FirstOrDefault(f => f.EqualsI(featureName));

            if (feature==null) return true;

            if (feature.Action==Feature.Actions.Disable) return VirtualDateTime.Now < feature.StartDate || VirtualDateTime.Now > feature.EndDate;

            return VirtualDateTime.Now > feature.StartDate && VirtualDateTime.Now < feature.EndDate;
        }
    }
}
