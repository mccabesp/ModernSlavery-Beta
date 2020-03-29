using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Infrastructure.Configuration;

namespace ModernSlavery.Hosts.Web
{
    public class Startup:IStartup
    {
        private DependencyBuilder _dependencyBuilder;
        public Startup(DependencyBuilder dependencyBuilder)
        {
            
        }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return null;
        }
        public void Configure(IApplicationBuilder app)
        {
        }

    }
}
