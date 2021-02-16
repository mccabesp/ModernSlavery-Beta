using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public class SqlRepository : IDataRepository, IDataTransaction
    {
        private IDbContextTransaction Transaction;

        private bool TransactionStarted;
        private readonly IServiceProvider _serviceProvider;

        public SqlRepository(IDbContext context, IServiceProvider serviceProvider)
        {
            DbContext = context;
            _serviceProvider = serviceProvider;
        }

        public IDbContext DbContext { get; private set; }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            return GetEntities<TEntity>();
        }

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().AnyAsync(predicate).ConfigureAwait(false);
        }

        public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().CountAsync(predicate).ConfigureAwait(false);
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public async Task<TEntity> SingleOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().SingleOrDefaultAsync(predicate).ConfigureAwait(false);
        }

        public async Task<TEntity> FirstOrDefaultByAscendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().OrderBy(keySelector).FirstOrDefaultAsync(filterPredicate).ConfigureAwait(false);
        }

        public async Task<TEntity> FirstOrDefaultByDescendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            return await DbContext.Set<TEntity>().OrderByDescending(keySelector).FirstOrDefaultAsync(filterPredicate).ConfigureAwait(false);
        }

        public async Task<List<TEntity>> ToListAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            if (predicate == null) return await DbContext.Set<TEntity>().ToListAsync().ConfigureAwait(false);
            return await DbContext.Set<TEntity>().Where(predicate).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TEntity>> ToListAscendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            if (filterPredicate == null) return await DbContext.Set<TEntity>().OrderBy(keySelector).ToListAsync().ConfigureAwait(false);
            return await DbContext.Set<TEntity>().Where(filterPredicate).OrderBy(keySelector).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<TEntity>> ToListDescendingAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, Expression<Func<TEntity, bool>> filterPredicate = null)
            where TEntity : class
        {
            if (filterPredicate == null)
                return await DbContext.Set<TEntity>().OrderByDescending(keySelector).ToListAsync().ConfigureAwait(false);
            return await DbContext.Set<TEntity>().Where(filterPredicate).OrderByDescending(keySelector).ToListAsync().ConfigureAwait(false);
        }

        public TEntity Get<TEntity>(params object[] keyValues) where TEntity : class
        {
            return GetEntities<TEntity>().Find(keyValues);
        }

        public async Task<TEntity> GetAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            return await GetEntities<TEntity>().FindAsync(keyValues).ConfigureAwait(false);
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Add(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Update(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            if (TransactionStarted && Transaction == null) Transaction = DbContext.GetDatabase().BeginTransaction();

            await DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, bool setOutputIdentity = false, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            if (TransactionStarted && Transaction == null) Transaction = DbContext.GetDatabase().BeginTransaction();

            await DbContext.BulkInsertAsync(entities,setOutputIdentity,batchSize,timeout).ConfigureAwait(false);
        }

        public async Task BulkDeleteAsync<TEntity>(IEnumerable<TEntity> entities, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            if (TransactionStarted && Transaction == null) Transaction = DbContext.GetDatabase().BeginTransaction();

            await DbContext.BulkDeleteAsync(entities, batchSize, timeout).ConfigureAwait(false);
        }

        public async Task BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entities, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            if (TransactionStarted && Transaction == null) Transaction = DbContext.GetDatabase().BeginTransaction();

            await DbContext.BulkUpdateAsync(entities, batchSize,timeout).ConfigureAwait(false);
        }

        public async Task ExecuteTransactionAsync(Func<Task> delegateAction)
        {
            if (Transaction != null) throw new Exception("Another transaction has already been started");

            var database = DbContext.GetDatabase();
            var strategy = database.CreateExecutionStrategy();

            try
            {
                TransactionStarted = true;
                await strategy.ExecuteAsync(delegateAction).ConfigureAwait(false);
                if (Transaction != null)throw new TransactionException("An SQL transaction has started which you must commit or rollback");
            }
            finally
            {
                TransactionStarted = false;
            }
        }

        public void BeginTransaction()
        {
            if (Transaction != null) throw new Exception("Another transaction has already been started");
            Transaction = DbContext.GetDatabase().BeginTransaction();
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
            if (Transaction == null) return;

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

        public int? GetCommandTimeout()
        {
            return DbContext.GetDatabase().GetCommandTimeout();
        }

        public void SetCommandTimeout(int? timeout)
        {
            DbContext.GetDatabase().SetCommandTimeout(timeout);
        }

        public void Reload()
        {
            DbContext = ActivatorUtilities.CreateInstance<DatabaseContext>(_serviceProvider);
        }
    }
}