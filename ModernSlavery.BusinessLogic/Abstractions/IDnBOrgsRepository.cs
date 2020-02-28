using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;

namespace ModernSlavery.BusinessLogic.Abstractions
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
