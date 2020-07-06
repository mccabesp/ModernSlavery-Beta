using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PasswordAttribute : RegularExpressionAttribute
    {
        public static SharedOptions SharedOptions;

        public PasswordAttribute() : base(SharedOptions.PasswordRegex)
        {
            ErrorMessage = SharedOptions.PasswordRegexError;
        }
    }
}