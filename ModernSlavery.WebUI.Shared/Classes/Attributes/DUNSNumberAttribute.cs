using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DUNSNumberAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^[0-9]{9}$";
        public static SharedOptions SharedOptions;

        public DUNSNumberAttribute() : base(pattern)
        {
            ErrorMessage = SharedOptions.CompanyNumberRegexError;
        }
    }
}