﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
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
        public static void SetShowEmailVerifyLink(this IHost host, bool value)
        {
            var testOptions = host.Services.GetRequiredService<TestOptions>();
            testOptions.ShowEmailVerifyLink = value;
        }
        public static void DisableLockoutProtection(this IHost host, bool value)
        {
            var testOptions = host.Services.GetRequiredService<TestOptions>();
            testOptions.DisableLockoutProtection = value;
        }
    }
}
