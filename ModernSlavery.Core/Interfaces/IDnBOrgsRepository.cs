using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDnBOrgsRepository
    {
        Task ClearAllDnBOrgsAsync();
        Task<List<DnBOrgsModel>> GetAllDnBOrgsAsync();
        Task ImportAsync(IDataRepository dataRepository, User currentUser);
        Task<List<DnBOrgsModel>> LoadIfNewerAsync();
        Task UploadAsync(List<DnBOrgsModel> newOrgs);
    }
}
