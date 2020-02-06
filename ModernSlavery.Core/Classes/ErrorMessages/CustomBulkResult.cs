using System.Collections.Concurrent;

namespace ModernSlavery.Core.Classes.ErrorMessages
{
    public class CustomBulkResult<T>
    {

        public ConcurrentBag<T> ConcurrentBagOfSuccesses { get; set; }
        public ConcurrentBag<CustomResult<T>> ConcurrentBagOfErrors { get; set; }
        public bool Failed => !ConcurrentBagOfErrors.IsEmpty;

    }
}
