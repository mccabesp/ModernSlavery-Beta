using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core;

namespace ModernSlavery.WebUI.Shared.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {

        public PasswordAttribute() : base(Global.PasswordRegex)
        {
            ErrorMessage = Global.PasswordRegexError;
        }

    }
}
