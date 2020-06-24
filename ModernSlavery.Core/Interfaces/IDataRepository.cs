using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDataRepository : IDataTransaction
    {
        void Delete<TEntity>(TEntity entity) where TEntity : class;

        TEntity Get<TEntity>(params object[] keyValues) where TEntity : class;

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class;
        Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class;

        Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class;

        Task<TEntity> FirstOrDefaultByAscendingAsync<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector,
            Expression<Func<TEntity, bool>> filterPredicate = null) where TEntity : class;

        Task<TEntity> FirstOrDefaultByDescendingAsync<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector,
            Expression<Func<TEntity, bool>> filterPredicate = null) where TEntity : class;

        Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class;

        Task<List<TEntity>> ToListAscendingAsync<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector,
            Expression<Func<TEntity, bool>> filterPredicate = null) where TEntity : class;

        Task<List<TEntity>> ToListDescendingAsync<TEntity, TKey>(Expression<Func<TEntity, TKey>> keySelector,
            Expression<Func<TEntity, bool>> filterPredicate = null) where TEntity : class;

        void Insert<TEntity>(TEntity entity) where TEntity : class;

        void Update<TEntity>(TEntity entity) where TEntity : class;

        Task SaveChangesAsync();

        void UpdateChangesInBulk<TEntity>(IEnumerable<TEntity> listOfOrganisations) where TEntity : class;
    }
}