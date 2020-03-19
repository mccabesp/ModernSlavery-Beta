using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;

namespace ModernSlavery.Infrastructure.Data
{
    public class SqlRepository : IDataRepository, IDataTransaction
    {
        private IDbContextTransaction Transaction;

        private bool TransactionStarted;

        public SqlRepository(IDbContext context)
        {
            DbContext = context;
        }

        public IDbContext DbContext { get; }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return GetEntities<TEntity>();
        }

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().CountAsync(predicate);
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity> FirstOrDefaultByAscendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().OrderBy(keySelector).FirstOrDefaultAsync(filterPredicate);
        }

        public async Task<TEntity> FirstOrDefaultByDescendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().OrderByDescending(keySelector).FirstOrDefaultAsync(filterPredicate);
        }

        public async Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            if (predicate == null) return await DbContext.Set<TEntity>().ToListAsync();
            return await DbContext.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public async Task<List<TEntity>> ToListAscendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            if (filterPredicate == null) return await DbContext.Set<TEntity>().OrderBy(keySelector).ToListAsync();
            return await DbContext.Set<TEntity>().Where(filterPredicate).OrderBy(keySelector).ToListAsync();
        }

        public async Task<List<TEntity>> ToListDescendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            if (filterPredicate == null)
                return await DbContext.Set<TEntity>().OrderByDescending(keySelector).ToListAsync();
            return await DbContext.Set<TEntity>().Where(filterPredicate).OrderByDescending(keySelector).ToListAsync();
        }

        public TEntity Get<TEntity>(params object[] keyValues) where TEntity : class
        {
            return GetEntities<TEntity>().Find(keyValues);
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Add(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            if (TransactionStarted && Transaction == null) Transaction = DbContext.GetDatabase().BeginTransaction();

            await DbContext.SaveChangesAsync();
        }

        public void UpdateChangesInBulk<TEntity>(IEnumerable<TEntity> listOfOrganisations) where TEntity : class
        {
            DbContext.UpdateChangesInBulk(listOfOrganisations);
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }

        public async Task BeginTransactionAsync(Func<Task> delegateAction)
        {
            if (Transaction != null) throw new Exception("Another transaction has already been started");

            var database = DbContext.GetDatabase();
            var strategy = database.CreateExecutionStrategy();

            TransactionStarted = true;
            try
            {
                await strategy.Execute(delegateAction);
                if (Transaction != null)
                    throw new TransactionException("An SQL transaction has started which you must commit or rollback");
            }
            finally
            {
                TransactionStarted = false;
            }
        }

        public void CommitTransaction()
        {
            if (Transaction == null) throw new Exception("Cannot commit a transaction which has not been started");

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public void RollbackTransaction()
        {
            if (Transaction == null) throw new Exception("Cannot rollback a transaction which has not been started");

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }

        public IDbContext GetDbContext()
        {
            return DbContext;
        }

        public DbSet<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            return DbContext.Set<TEntity>();
        }
    }
}