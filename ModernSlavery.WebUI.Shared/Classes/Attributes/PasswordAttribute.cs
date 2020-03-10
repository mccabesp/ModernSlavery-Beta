using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {

        public PasswordAttribute(GlobalOptions globalOptions) : base(globalOptions.PasswordRegex)
        {
            ErrorMessage = globalOptions.PasswordRegexError;
        }

    }
}
