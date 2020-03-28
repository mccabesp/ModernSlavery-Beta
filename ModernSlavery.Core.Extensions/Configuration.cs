using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ModernSlavery.Core.Extensions
{
    public static partial class Extrensions
    {
        public static T GetValueOrDefault<T>(this IConfiguration config, string key, T defaultValue = default)
        {
            var value = config[key];
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return config.GetValue<T>(key, defaultValue);
        }
    }
}
