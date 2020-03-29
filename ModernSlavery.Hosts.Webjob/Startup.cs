using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

namespace ModernSlavery.Hosts.Webjob
{
    public class Startup:IWebJobsStartup
    {
        public Startup()
        {

        }

        public void Configure(IWebJobsBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
