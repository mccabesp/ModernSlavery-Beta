using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Hosts.Webjob
{
    //Allows disabling of named webjob methods in DisabledWebjobs section of appsettings.json
    //This only applies to timed-functions
    public class DisableWebjobProvider
    {
        private readonly WebjobsOptions _webjobOptions;
        public DisableWebjobProvider(WebjobsOptions webjobOptions)
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