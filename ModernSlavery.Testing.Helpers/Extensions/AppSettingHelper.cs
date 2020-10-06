using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AppSettingHelper
    {
        //Returns a value from configuration
        public static string GetAppSetting(this IHost host, string key)
        {
            var config = host.Services.GetRequiredService<IConfiguration>();
            return config[key];
        }

        //Sets a value to configuration
        public static void SetAppSetting(this IHost host, string key, string value)
        {
            var config = host.Services.GetRequiredService<IConfiguration>();
            config[key] = value;
        }
    }
}
