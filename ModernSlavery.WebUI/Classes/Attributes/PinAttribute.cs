using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core;

namespace ModernSlavery.WebUI.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {

        public PinAttribute() : base(Global.PinRegex)
        {
            ErrorMessage = Global.PinRegexError;
        }

    }
}
