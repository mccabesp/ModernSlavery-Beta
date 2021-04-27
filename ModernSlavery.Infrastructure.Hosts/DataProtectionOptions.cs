using System;
using System.Collections.Generic;
using System.Configuration;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Hosts
{
    [Options("DataProtection")]
    public class DataProtectionOptions : IOptions
    {
        public string Type { get; set; }
        public string AzureConnectionString { get; set; }
        public string KeyName { get; set; } = "DataProtection-Keys";
        public string ApplicationDiscriminator { get; set; }
        public string Container { get; set; } = "data-protection";
        public string KeyFilepath { get; set; } = "keys.xml";

        public void Validate()
        {
            var exceptions = new List<Exception>();
            
            switch (Type?.ToLower())
            {
                case "redis":
                    if (string.IsNullOrWhiteSpace(AzureConnectionString))
                        exceptions.Add(new ConfigurationErrorsException($"Invalid or missing setting 'DataProtection:{nameof(AzureConnectionString)}'"));

                    if (string.IsNullOrWhiteSpace(KeyName))
                        exceptions.Add(new ConfigurationErrorsException($"Invalid or missing setting 'DataProtection:{nameof(KeyName)}'"));
                    break;
                case "blob":
                    //Use blob storage to persist data protection keys equivalent to old MachineKeys
                    if (string.IsNullOrWhiteSpace(AzureConnectionString))
                        exceptions.Add(new ConfigurationErrorsException($"Invalid or missing setting 'DataProtection:{nameof(AzureConnectionString)}'"));

                    if (string.IsNullOrWhiteSpace(Container))
                        exceptions.Add(new ConfigurationErrorsException($"Invalid or missing setting 'DataProtection:{nameof(Container)}'"));

                    if (string.IsNullOrWhiteSpace(KeyFilepath))
                        exceptions.Add(new ConfigurationErrorsException($"Invalid or missing setting 'DataProtection:{nameof(KeyFilepath)}'"));

                    break;
                case "memory":
                case "none":
                    break;
                default:
                    throw new Exception($"Unrecognised DataProtection:{nameof(Type)}='{Type}'");
            }

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

    }
}