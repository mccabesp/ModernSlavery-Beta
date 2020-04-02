using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CompanyNumberAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^[0-9A-Za-z]{8}$";
        private readonly SharedOptions _sharedOptions;

        public CompanyNumberAttribute() : base(pattern)
        {
            _sharedOptions = Activator.CreateInstance<SharedOptions>();
            ErrorMessage = _sharedOptions.CompanyNumberRegexError;
        }
    }
}