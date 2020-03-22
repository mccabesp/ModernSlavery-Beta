using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {
        private static GlobalOptions globalOptions = Activator.CreateInstance<GlobalOptions>();
        public PasswordAttribute() : base(globalOptions.PasswordRegex)
        {
            ErrorMessage = globalOptions.PasswordRegexError;
        }

    }
}
