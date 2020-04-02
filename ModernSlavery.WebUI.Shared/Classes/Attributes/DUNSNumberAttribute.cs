using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DUNSNumberAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^[0-9]{9}$";
        private readonly SharedOptions _sharedOptions;

        public DUNSNumberAttribute() : base(pattern)
        {
            _sharedOptions = Activator.CreateInstance<SharedOptions>();
            ErrorMessage = _sharedOptions.CompanyNumberRegexError;
        }
    }
}