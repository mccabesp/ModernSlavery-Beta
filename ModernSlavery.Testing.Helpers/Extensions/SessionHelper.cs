using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class SessionHelper
    {
        //Returns a value from session
        public static void GetSessionVariable(this IHost host, string sessionId, string key)
        {
            throw new NotImplementedException();
        }

        //Sets a value to session
        public static void SetSessionVariable(this IHost host, string sessionId, string key, string value)
        {
            throw new NotImplementedException();
        }

        //Clears a value from session
        public static void ClearSessionVariable(this IHost host, string sessionId, string key)
        {
            throw new NotImplementedException();
        }
    }
}
