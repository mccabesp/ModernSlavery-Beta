using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery
{
    public interface IShortCodesRepository
    {
        Task<List<ShortCodeModel>> GetAllShortCodesAsync();
        Task ClearAllShortCodesAsync();
    }
}