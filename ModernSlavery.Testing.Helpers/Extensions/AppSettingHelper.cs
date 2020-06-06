using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AppSettingHelper
    {
        //Returns a value from session
        public static void GetAppSetting(this IHost host, string sessionId, string key)
        {
            throw new NotImplementedException();
        }

        //Sets a value to session
        public static void SetAppSetting(this IHost host, string sessionId, string key, string value)
        {
            throw new NotImplementedException();
        }

        //Clears a value from session
        public static void ClearAppSetting(this IHost host, string key)
        {
            throw new NotImplementedException();
        }

        public static void SetSessionTimeOut(this IHost host, int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
