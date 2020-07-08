using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {
        public static SharedOptions SharedOptions;

        public PinAttribute() : base(SharedOptions.PinRegex)
        {
            ErrorMessage = SharedOptions.PinRegexError;
        }
    }
}