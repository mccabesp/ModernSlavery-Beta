using System;
using System.Reflection;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob
{
    //Allows disabling of named webjob methods in DisabledWebJobs section of appsettings.json
    //This only applies to timed-class WebJob:WebJob
    public class DisableWebJobProvider
    {
        private readonly WebJobsOptions _webjobOptions;
        public DisableWebJobProvider(WebJobsOptions webjobOptions)
        {
            _webjobOptions = webjobOptions;
        }

        public bool IsDisabled(MethodInfo method)
        {
            //Check using the full name first
            var methodName = method.Name;

            if (!_webjobOptions.ContainsKey(method.Name) && method.IsAsyncMethod())
            {
                var i = method.Name.LastIndexOf("Async", StringComparison.OrdinalIgnoreCase);
                if (i > 0) methodName = method.Name.Substring(0, i);
            }

            return _webjobOptions.IsDisabled(methodName);
        }
    }
}