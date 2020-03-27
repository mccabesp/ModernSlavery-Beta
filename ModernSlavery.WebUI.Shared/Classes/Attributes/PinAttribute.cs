using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {
        private static readonly SharedOptions _sharedOptions = Activator.CreateInstance<SharedOptions>();

        public PinAttribute() : base(_sharedOptions.PinRegex)
        {
            ErrorMessage = _sharedOptions.PinRegexError;
        }
    }
}