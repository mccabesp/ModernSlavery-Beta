using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using static ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions.TelemetrySuppressionRecord;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions
{
    [Options("ApplicationInsights:Suppressions")]
    public class TelemetrySuppressionOptions : List<TelemetrySuppressionRecord>, IOptions
    {
        public TelemetrySuppressionOptions() : base()
        {

        }

        public void Validate() 
        {
            var exceptions = new List<Exception>();

            foreach(var record in this)
            {
                //Ensure valid telemetry type
                if (record.TelemetryType== TelemetryTypes.Unknown) exceptions.Add(new ConfigurationErrorsException($"{nameof(TelemetrySuppressionRecord.TelemetryType)} must be {Enums.GetEnumDescriptions(TelemetryTypes.Unknown).ToDelimitedString(", ")}."));
                
                if (record.Action==FilterActionTypes.Unknown)
                    //Ensure valid action type
                    exceptions.Add(new ConfigurationErrorsException($"{nameof(TelemetrySuppressionRecord.Action)} must be {Enums.GetEnumDescriptions(FilterActionTypes.Unknown).ToDelimitedString(", ")}."));
                else if (record.Action.IsAny(FilterActionTypes.Success, FilterActionTypes.Fail) && !record.TelemetryType.IsAny(TelemetryTypes.Availability, TelemetryTypes.Dependency, TelemetryTypes.Request))
                    //Ensure only force of success or failure for Availability, Dependency or Request telemetry types
                    exceptions.Add(new ConfigurationErrorsException($"{nameof(TelemetrySuppressionRecord.Action)} can not be '{FilterActionTypes.Success}' or '{FilterActionTypes.Fail}' for {record.TelemetryType}Telemetry."));

                //Ensure evey suppression has a justification
                if (string.IsNullOrWhiteSpace(record.Justification)) exceptions.Add(new ConfigurationErrorsException($"{nameof(TelemetrySuppressionRecord.Justification)} cannot be empty."));

            }
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }

        }
        public IEnumerable<TelemetrySuppressionRecord> GetSuppressions(ITelemetry telemetry)
        {
            return this.Where(sr => sr.IsMatch(telemetry));
        }

        public FilterActionTypes ExecuteSuppressions(ITelemetry telemetry)
        {
            var suppressions = GetSuppressions(telemetry);

            if (!suppressions.Any())return FilterActionTypes.Unknown;

            //If any of the suppression is a delete then return the Delete action
            if (suppressions.Any(s => s.Action == FilterActionTypes.Delete)) return FilterActionTypes.Delete;

            var suppressionRecord = suppressions.FirstOrDefault(s => s.Action.IsAny(FilterActionTypes.Success, FilterActionTypes.Fail));

            if (suppressionRecord!=null)
            {
                //Force success or failure of the telemetry
                var successProperty = telemetry.GetType().GetProperty($"Success");
                if (successProperty != null)successProperty.SetValue(telemetry, suppressionRecord.Action == FilterActionTypes.Success);
            }
            else
            {
                suppressionRecord = suppressions.First();
            }

            //Add the suppression justification to the telemetry properties
            if (telemetry is ISupportProperties propertiesTelemetry) propertiesTelemetry.Properties["Suppression"] = $"{suppressionRecord.Action}: {suppressionRecord.Justification}";

            return suppressionRecord.Action;
        }
    }
}
