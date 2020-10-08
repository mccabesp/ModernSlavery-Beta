using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class RegistryHelper
    {
        private static string RegistryKey= $"{Registry.CurrentUser}\\{nameof(ModernSlavery)}Testing";

        public static string GetRegistryKey(string valueName)
        {
            return Registry.GetValue(RegistryKey, valueName,null) as string;
        }

        public static void SetRegistryKey(string valueName, string value)
        {
            Registry.SetValue(RegistryKey, valueName, value);
        }
    }
}
