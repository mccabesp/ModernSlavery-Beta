using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModernSlavery.Core.Extensions
{
    public static class Concurrency
    {
        public static async Task<T> WhenAny<T>(this IEnumerable<Task<T>> tasks,
            CancellationTokenSource cancellationToken,
            Func<T, bool> predicate)
        {
            var taskList = tasks.ToList();

            Task<T> completedTask = null;

            taskList.ForEach(t => t.Start());

            while (taskList.Count > 0)
            {
                completedTask = await Task.WhenAny(taskList).ConfigureAwait(false);
                taskList.Remove(completedTask);

                if (predicate(await completedTask.ConfigureAwait(false)))
                {
                    cancellationToken.Cancel(false);
                    break;
                }

                completedTask = null;
            }

            return completedTask == null ? default : completedTask.Result;
        }
    }
}