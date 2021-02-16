using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.BusinessDomain.DevOps.Models
{
    public class DatabaseModel
    {
        public class DatabaseTableModel
        {
            public string TableName { get; set; }
            public int RecordCount { get; set; }
            public DateTime LatestRecord { get; set; }
        }
        public string DatabaseName { get; set; }
        public int DatabaseSize { get; set; }

        public List<DatabaseTableModel> Tables = new List<DatabaseTableModel>();
    }
}
