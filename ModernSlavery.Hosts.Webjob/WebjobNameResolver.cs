using Microsoft.Azure.WebJobs;
using System;

namespace ModernSlavery.Hosts.Webjob
{
    public class WebJobNameResolver : INameResolver
    {
        private readonly WebJobsOptions _webjobsOptions;
        public WebJobNameResolver(WebJobsOptions webjobsOptions)
        {
            _webjobsOptions = webjobsOptions;
        }

        public string Resolve(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName)) throw new ArgumentNullException(nameof(triggerName));

            var webjobName = triggerName;

            if (!_webjobsOptions.ContainsKey(webjobName))
            {
                var i = webjobName.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase);
                if (i > 0) webjobName = webjobName.Substring(0, i);
            }

            return _webjobsOptions.ContainsKey(webjobName) ? _webjobsOptions[webjobName].Trigger : triggerName;
        }
    }
}
