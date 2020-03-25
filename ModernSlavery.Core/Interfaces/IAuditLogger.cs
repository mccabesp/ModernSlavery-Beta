using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IAuditLogger
    {
        Task WriteAsync(IEnumerable<object> records);
        Task WriteAsync(object record);
    }
}