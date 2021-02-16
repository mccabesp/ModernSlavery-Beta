using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ModernSlavery.Core.Options
{
    [Options("CompaniesHouse")]
    public class CompaniesHouseOptions : IOptions
    {
        public string[] GetApiKeys() => ApiKey.SplitI();
        public string ApiKey { get; set; }
        public string ApiServer { get; set; }
        public string CompanyNumberRegexError { get; set; } = "Company number must contain 8 characters only";
        public int MaxPageSize { get; set; } = 100;
        public int MaxResponseCompanies { get; set; } = 400;
        public int MaxApiCallsPerFiveMins { get; set; } = 600;//The maximum allowed updates in a 5 min interval per ApiKey
        public int UpdateHours { get; set; } = 24;//How often to check for updates
        public RetryPolicyTypes RetryPolicy { get; set; } = RetryPolicyTypes.None;
        public void Validate()
        {
            var exceptions = new List<Exception>();
            if (string.IsNullOrWhiteSpace(ApiKey) || GetApiKeys().Length==0) exceptions.Add(new ConfigurationErrorsException("CompaniesHouse:ApiKey cannot be empty"));
            if (RetryPolicy == RetryPolicyTypes.None) exceptions.Add(new ConfigurationErrorsException("CompaniesHouse:RetryPolicy must be Linear or Exponential"));
            if (string.IsNullOrWhiteSpace(ApiServer)) exceptions.Add(new ConfigurationErrorsException("CompaniesHouse:ApiServer cannot be empty"));

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

    }
}