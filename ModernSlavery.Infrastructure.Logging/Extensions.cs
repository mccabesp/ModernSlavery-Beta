using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Extensions;
using Serilog;
using Serilog.Core;

namespace ModernSlavery.Infrastructure.Logging
{
    public static class Extensions
    {
        public static void SetupSerilogLogger(this IConfiguration config)
        {
            Logger log;

            if (config["environment"].EqualsI("Development"))
                log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            else
                log = new LoggerConfiguration().WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                    .CreateLogger();

            Log.Logger = log;
            Log.Information("Serilog logger setup complete");
        }
    }
}
