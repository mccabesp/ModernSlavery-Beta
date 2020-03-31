using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("Features")]
    public class FeatureSwitchOptions : Dictionary<String,FeatureSwitchOptions.Feature>,IOptions
    {
        public FeatureSwitchOptions():base(StringComparer.OrdinalIgnoreCase)
        {

        }
        public bool IsDisabled(string featureName)
        {
            return !IsEnabled(featureName);
        }

        public bool IsEnabled(string featureName)
        {
            var feature = this.ContainsKey(featureName) ? this[featureName] : null;

            if (feature == null) return true;

            if (feature.Action == Feature.Actions.Disable)
                return VirtualDateTime.Now < feature.StartDate || VirtualDateTime.Now > feature.EndDate;

            return VirtualDateTime.Now > feature.StartDate && VirtualDateTime.Now < feature.EndDate;
        }

        public class Feature
        {
            public enum Actions
            {
                Enable,
                Disable
            }
            public Actions Action { get; set; }
            public DateTime StartDate { get; set; } = DateTime.MinValue;
            public DateTime EndDate { get; set; } = DateTime.MaxValue;
        }
    }
}