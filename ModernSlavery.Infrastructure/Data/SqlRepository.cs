using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public async Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await DbContext.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await DbContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

        public TEntity Get<TEntity>(params object[] keyValues) where TEntity : class
        {
            return GetEntities<TEntity>().Find(keyValues);
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Add(entity);
        }

        public IDbContext GetDbContext()
        {
            return DbContext;
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            GetEntities<TEntity>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            if (TransactionStarted && Transaction == null)
            {
                Transaction = DbContext.GetDatabase().BeginTransaction();
            }

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

        public DbSet<TEntity> GetEntities<TEntity>() where TEntity : class
        {
            return DbContext.Set<TEntity>();
        }

        public async Task BeginTransactionAsync(Func<Task> delegateAction)
        {
            if (Transaction != null)
            {
                throw new Exception("Another transaction has already been started");
            }

            DatabaseFacade database = DbContext.GetDatabase();
            IExecutionStrategy strategy = database.CreateExecutionStrategy();

            TransactionStarted = true;
            try
            {
                await strategy.Execute(delegateAction);
                if (Transaction != null)
                {
                    throw new TransactionException("An SQL transaction has started which you must commit or rollback");
                }
            }
            finally
            {
                TransactionStarted = false;
            }
        }

        public void CommitTransaction()
        {
            if (Transaction == null)
            {
                throw new Exception("Cannot commit a transaction which has not been started");
            }

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public void RollbackTransaction()
        {
            if (Transaction == null)
            {
                throw new Exception("Cannot rollback a transaction which has not been started");
            }

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }
    }

}
