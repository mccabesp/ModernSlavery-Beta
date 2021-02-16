using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ModernSlavery.Core.Options
{
    [Options("PostcodeChecker")]
    public class PostcodeCheckerOptions : IOptions
    {
        public string ApiServer { get; set; }
        public RetryPolicyTypes RetryPolicy { get; set; } = RetryPolicyTypes.None;
        public void Validate()
        {
            var exceptions = new List<Exception>();
            if (RetryPolicy == RetryPolicyTypes.None) exceptions.Add(new ConfigurationErrorsException("PostcodeChecker:RetryPolicy must be Linear or Exponential"));
            if (string.IsNullOrWhiteSpace(ApiServer)) exceptions.Add(new ConfigurationErrorsException("PostcodeChecker:ApiServer cannot be empty"));

            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

    }
}