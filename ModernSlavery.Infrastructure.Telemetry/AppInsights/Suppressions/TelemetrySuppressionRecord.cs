using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions
{
    public class TelemetrySuppressionRecord
    {
        public enum TelemetryTypes
        {
            Unknown=0,
            Availability,
            Dependency,
            Event,
            Exception,
            Metric,
            PageViewPerformance,
            PageView,
            Request,
            Trace,
        }

        public enum FilterActionTypes
        {
            Unknown=0,
            None, //Do not change telemetry - used for notes on 
            Delete, //Do not push to App Insights
            Success, //Push to App Insights but mark as successful
            Fail //Push to App Insights but mark as failure
        }

        public TelemetryTypes TelemetryType { get; set; } = TelemetryTypes.Unknown;
        public string ApplicationRole { get; set; }
        public string Target { get; set; }
        public string Operation { get; set; }
        public string ResultCode { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public FilterActionTypes Action { get; set; } = FilterActionTypes.Unknown;
        public string Justification { get; set; }

        public bool IsMatch(ITelemetry telemetry)
        {
            var filterCount = 0;

            //Check the telemetry type
            if (TelemetryType!= TelemetryTypes.Unknown && (++filterCount) > 0 && !Matches($"{TelemetryType}Telemetry", telemetry.GetType().Name)) return false;
            
            //Check the cloud role name
            if (!string.IsNullOrWhiteSpace(ApplicationRole) && (++filterCount) > 0 && !Matches(ApplicationRole,telemetry.Context.Cloud.RoleName)) return false;

            //Check the Target
            if (!string.IsNullOrWhiteSpace(Target) && (++filterCount) > 0 && !Matches(Target, telemetry.GetPropertyString($"{nameof(Target)}"))) return false;

            //Check the Operation
            if (!string.IsNullOrWhiteSpace(Operation) && (++filterCount) > 0 && !Matches(Operation, telemetry.Context.Operation.Name,telemetry.GetPropertyString($"Name"))) return false;

            //Check the ResultCode
            if (!string.IsNullOrWhiteSpace(ResultCode) && (++filterCount) > 0 && !Matches(ResultCode, telemetry.GetPropertyString($"{nameof(ResultCode)}"), telemetry.GetPropertyString($"ResponseCode"))) return false;

            //Check the Severity
            if (!string.IsNullOrWhiteSpace(Severity) && (++filterCount) > 0 && !Matches(Severity, telemetry.GetPropertyString($"{nameof(Severity)}"))) return false;

            //Check the Message
            if (!string.IsNullOrWhiteSpace(Message) && (++filterCount) > 0 && !Matches(Message, telemetry.GetPropertyString($"{nameof(Message)}"))) return false;

            //Check the properties
            if (Properties!=null && Properties.Count>0 && telemetry is ISupportProperties propertiesTelemetry && (++filterCount) > 0 && !Matches(Properties, propertiesTelemetry.Properties)) return false;

            return filterCount >0;
        }

        private bool Matches(string filterProperty, params string[] telemetryProperties)
        {
            var filter = filterProperty.Trim().TrimStart('^', '~', '%','[').TrimEnd('$',']');

            foreach (var telemetryProperty in telemetryProperties.Where(t=>!string.IsNullOrWhiteSpace(t)))
            {
                if (filterProperty.StartsWith("[") && filterProperty.EndsWith("]"))
                {
                    if (Regex.IsMatch(telemetryProperty, filter, RegexOptions.IgnoreCase)) return true;
                }
                else if (filterProperty.StartsWith("%"))
                {
                    if (telemetryProperty.Like(filter)) return true;
                }
                else if (filterProperty.StartsWith("~"))
                {
                    if (telemetryProperty.ContainsI(filter)) return true; 
                }
                else if (filterProperty.StartsWith("^") || filterProperty.EndsWith("$"))
                {
                    if (filterProperty.StartsWith("^") && !telemetryProperty.StartsWithI(filter)) continue;
                    if (filterProperty.EndsWith("$") && !telemetryProperty.EndsWithI(filter)) continue;
                    return true;
                }
                else if (telemetryProperty.EqualsI(filterProperty))
                    return true;
            }

            return false;
        }

        private bool Matches(IDictionary<string, string> filterProperties, IDictionary<string, string> telemetryProperties)
        {
            foreach (var key in filterProperties.Keys)
            {
                if (!telemetryProperties.ContainsKey(key) || !Matches(telemetryProperties[key], telemetryProperties[key])) return false;
            }
            return true;
        }

    }
}
