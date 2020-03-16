using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface IShortCodesRepository
    {
        Task<List<ShortCodeModel>> GetAllShortCodesAsync();
        Task ClearAllShortCodesAsync();
    }
}