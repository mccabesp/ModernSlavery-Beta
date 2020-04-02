using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {
        private static readonly SharedOptions sharedOptions = Activator.CreateInstance<SharedOptions>();

        public PasswordAttribute() : base(sharedOptions.PasswordRegex)
        {
            ErrorMessage = sharedOptions.PasswordRegexError;
        }
    }
}