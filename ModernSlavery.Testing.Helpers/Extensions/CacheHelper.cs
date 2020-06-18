using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class CacheHelper
    {
        //Returns a value from cache
        public static void GetCacheVariable(this IHost host, string key)
        {
            throw new NotImplementedException();
        }

        //Sets a value to cache
        public static void SetCacheVariable(this IHost host, string key, string value)
        {
            throw new NotImplementedException();
        }

        //Clears a value from cache
        public static void ClearCacheVariable(this IHost host, string key)
        {
            throw new NotImplementedException();
        }

        //Clears the security lockout from cache
        public static void ClearLockout()
        {
            throw new NotImplementedException();
        }

    }
}
