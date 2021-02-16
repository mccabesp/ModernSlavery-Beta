using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.BusinessDomain.DevOps.Models
{
    public class ResourceGroupModel
    {
        public string ResourceGroupName { get; set; }
        public WebModel Website { get; set; }
        public WebjobsModel Webjobs { get; set; }
        public CacheModel Cache { get; set; }
        public DatabaseModel Database { get; set; }
        public FilesModel Files { get; set; }
        public QueuesModel Queues { get; set; }
        public SearchModel Search { get; set; }
        public LogsModel Logs { get; set; }
    }
}
