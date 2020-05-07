using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.Testing.Helpers
{
    public static class Extensions
    {
        public static string GetWebAuthority(this IHost host)
        {
            return host.Services.GetRequiredService<IServerAddressesFeature>().Addresses.LastOrDefault(); //Last is https://localhost:5001!
        }
    }
}
