using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class PinAttribute : RegularExpressionAttribute
    {
        private static GlobalOptions _globalOptions= Activator.CreateInstance<GlobalOptions>();

        public PinAttribute() : base(_globalOptions.PinRegex)
        {
            ErrorMessage = _globalOptions.PinRegexError;
        }

    }
}
