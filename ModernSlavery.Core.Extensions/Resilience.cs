using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace ModernSlavery.Core.Extensions
{
    public static class Resilience
    {
        /// <summary>
        /// Get a synchronous retry policy for an exception which increases exponentially (best for non-frontend)
        /// </summary>
        /// <typeparam name="T">The exception type to retry</typeparam>
        /// <param name="retryCount">The maximum number of retries (Default=6 totalling 64 seconds)</param>
        /// <param name="filter">(optional)A predicate to filter the exception properties</param>
        /// <returns>The synchronous retry policy</returns>
        public static RetryPolicy GetExponentialRetryPolicy<T>(int retryCount = 6, Func<T, bool> filter = null) where T : Exception
        {
            var jitterer = new Random();
            return Policy.Handle<T>(filter)
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(10, 500)));
        }
        /// <summary>
        /// Get an asynchronous retry policy for an exception which increases exponentially (best for non-frontend)
        /// </summary>
        /// <typeparam name="T">The exception type to retry</typeparam>
        /// <param name="retryCount">The maximum number of retries (Default=6 totalling 64 seconds)</param>
        /// <param name="filter">(optional) A predicate to filter the exception properties</param>
        /// <returns>The synchronous retry policy</returns>
        public static AsyncPolicy GetExponentialAsyncRetryPolicy<T>(int retryCount = 6, Func<T, bool> filter = null) where T : Exception
        {
            var jitterer = new Random();
            return Policy.Handle<T>(filter)
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(10, 500)));
        }

        /// <summary>
        /// Get a synchronous retry policy for an exception which increases linearly (best for frontend) by random amount
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=10)</param>
        /// <param name="minRetryMilliSeconds">The minimum number of milliseconds before each retry</param>
        /// <param name="maxRetryMilliSeconds">The maximum number of milliseconds before each retry</param>
        /// <returns>The synchronous retry policy</returns>
        public static RetryPolicy GetLinearRetryPolicy<T>(int retryCount = 10, int minRetryMilliSeconds = 100, int maxRetryMilliSeconds = 1000, Func<T, bool> filter = null) where T : Exception
        {
            return Policy.Handle<T>(filter)
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(minRetryMilliSeconds, maxRetryMilliSeconds)));
        }

        /// <summary>
        /// Get an asynchronous retry policy for an exception which increases linearly (best for frontend) by random amount
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=10)</param>
        /// <param name="minRetryMilliSeconds">The minimum number of milliseconds before each retry</param>
        /// <param name="maxRetryMilliSeconds">The maximum number of milliseconds before each retry</param>
        /// <returns>The asynchronous retry policy</returns>
        public static AsyncPolicy GetLinearAsyncRetryPolicy<T>(Func<T, bool> filter = null, int retryCount = 10, int minRetryMilliSeconds = 100, int maxRetryMilliSeconds = 1000) where T : Exception
        {
            return Policy.Handle<T>(filter)
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(minRetryMilliSeconds, maxRetryMilliSeconds)));
        }

        /// <summary>
        /// Get a synchronous retry policy for an http request which increases exponentially (best for non-frontend)
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=6 totalling 64 seconds)</param>
        /// <returns>The synchronous retry policy</returns>
        public static RetryPolicy<HttpResponseMessage> GetExponentialRetryPolicy(int retryCount = 6)
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(10, 500)));
        }

        /// <summary>
        /// Get an asynchronous retry policy for an http request which increases exponentially (best for non-frontend)
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=6 totalling 64 seconds)</param>
        /// <returns>The asynchronous retry policy</returns>
        public static AsyncPolicy<HttpResponseMessage> GetExponentialAsyncRetryPolicy(int retryCount = 6)
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(10, 500)));
        }

        /// <summary>
        /// Get a synchronous retry policy for a Http request which increases linearly (best for frontend) by random amount
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=10)</param>
        /// <param name="minRetryMilliSeconds">The minimum number of milliseconds before each retry</param>
        /// <param name="maxRetryMilliSeconds">The maximum number of milliseconds before each retry</param>
        /// <returns>The synchronous retry policy</returns>
        public static RetryPolicy<HttpResponseMessage> GetLinearRetryPolicy(int retryCount = 10, int minRetryMilliSeconds = 100, int maxRetryMilliSeconds = 1000)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(minRetryMilliSeconds, maxRetryMilliSeconds)));
        }

        /// <summary>
        /// Get an asynchronous retry policy for a Http request which increases linearly (best for frontend) by random amount
        /// </summary>
        /// <param name="retryCount">The maximum number of retries (Default=10)</param>
        /// <param name="minRetryMilliSeconds">The minimum number of milliseconds before each retry</param>
        /// <param name="maxRetryMilliSeconds">The maximum number of milliseconds before each retry</param>
        /// <returns>The asynchronous retry policy</returns>
        public static AsyncPolicy<HttpResponseMessage> GetLinearAsyncRetryPolicy(int retryCount = 10, int minRetryMilliSeconds = 100, int maxRetryMilliSeconds = 1000)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(minRetryMilliSeconds, maxRetryMilliSeconds)));
        }
    }
}
