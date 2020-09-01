using System;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IDataTransaction
    {
        /// <summary>
        ///     Creates a transaction for aggregating data update operations
        /// </summary>
        /// <param name="transactionFunc"></param>
        Task ExecuteTransactionAsync(Func<Task> transactionFunc);

        void BeginTransaction();

        void CommitTransaction();

        void RollbackTransaction();
    }
}