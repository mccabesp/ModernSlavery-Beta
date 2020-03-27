using System;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoCacheAttribute : ResponseCacheAttribute
    {
        public NoCacheAttribute()
        {
            NoStore = true;
            Location = ResponseCacheLocation.None;
        }
    }
}