using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    [Options("CompaniesHouse")]
    public class CompaniesHouseOptions : IOptions
    {
        public string ApiKey { get; set; }
        public string ApiServer { get; set; }
        public string CompanyNumberRegexError { get; set; } = "Company number must contain 8 characters only";
        public int MaxRecords { get; set; } = 400;

        public void Validate() 
        {
            var exceptions = new List<Exception>();
            if (string.IsNullOrWhiteSpace(ApiKey)) exceptions.Add(new ConfigurationErrorsException("CompaniesHouse:ApiKey cannot be empty"));
            if (string.IsNullOrWhiteSpace(ApiServer)) exceptions.Add(new ConfigurationErrorsException("CompaniesHouse:ApiServer cannot be empty"));

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

    }
}