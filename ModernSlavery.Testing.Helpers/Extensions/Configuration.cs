using AngleSharp;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class Configuration
    {
        /// <summary>
        /// Creates the configuration for a load of filenames.
        /// </summary>
        /// <param name="filenames">A list of json filenames. If empty uses 'Assemblyname.json' and 'App.json'</param>
        /// <returns>A set of key/value application configuration properties</returns>
        public static IConfiguration GetJsonSettings(params string[]filenames)
        {
            var configBuilder = new ConfigurationBuilder();

            if (filenames == null || !filenames.Any())
            {
                configBuilder.AddJsonFile($"{Assembly.GetEntryAssembly().GetName().Name}.json", true, false);
                configBuilder.AddJsonFile("App.json", true, false);
            }

            foreach (var filename in filenames)
            {
                configBuilder.AddJsonFile(filename, false, false);
            }
            return configBuilder.Build();
        }
    }
}
