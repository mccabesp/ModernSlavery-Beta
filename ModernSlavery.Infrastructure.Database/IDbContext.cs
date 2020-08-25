using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ModernSlavery.Infrastructure.Database
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync();
        void UpdateChangesInBulk<TEntity>(IEnumerable<TEntity> listOfOrganisations) where TEntity : class;

        DatabaseFacade GetDatabase();

        bool MigrationsApplied { get; }

        Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, bool setOutputIdentity = false) where TEntity : class;
    }
}