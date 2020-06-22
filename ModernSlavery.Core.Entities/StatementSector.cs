using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.WebSites.Models;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Core.Entities
{
    public class StatementSector
    {
        public short StatementSectorTypeId { get; set; }

        public virtual StatementSectorType StatementSectorType { get; set; }

        public long StatementId { get; set; }

        public virtual StatementMetadata Statement { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
    }
}
