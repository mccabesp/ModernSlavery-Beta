using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class QueueHelper
    {
        //Returns a value from message queue
        public static void Peek<T>(this IHost host, string sessionId, string queueName, string key)
        {
            throw new NotImplementedException();
        }

        //Sets a value to message queue
        public static void Enqueue<T>(this IHost host, string sessionId, string queueName, T value)
        {
            throw new NotImplementedException();
        }

        //Clears a value from message queue
        public static T Dequeue<T>(this IHost host, string sessionId, string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
