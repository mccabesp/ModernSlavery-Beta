using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CompanyNumberAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^[0-9A-Za-z]{8}$";
        private GlobalOptions _globalOptions;
        public CompanyNumberAttribute() : base(pattern)
        {
            _globalOptions = Activator.CreateInstance<GlobalOptions>();
            ErrorMessage = _globalOptions.CompanyNumberRegexError;
        }
    }
}
