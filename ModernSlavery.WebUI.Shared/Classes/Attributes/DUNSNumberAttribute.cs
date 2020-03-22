using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DUNSNumberAttribute : RegularExpressionAttribute
    {

        private const string pattern = @"^[0-9]{9}$";
        private GlobalOptions _globalOptions;
        public DUNSNumberAttribute() : base(pattern)
        {
            _globalOptions = Activator.CreateInstance<GlobalOptions>();
            ErrorMessage = _globalOptions.CompanyNumberRegexError;
        }

    }
}
