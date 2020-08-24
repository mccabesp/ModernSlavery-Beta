using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ModernSlavery.Hosts.Webjob.WebjobsOptions;

namespace ModernSlavery.Hosts.Webjob
{
    public class WebjobNameResolver : INameResolver
    {
        private readonly WebjobsOptions _webjobsOptions;
        public WebjobNameResolver(WebjobsOptions webjobsOptions)
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
