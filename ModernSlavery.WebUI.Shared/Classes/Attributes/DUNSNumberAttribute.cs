using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core;

namespace ModernSlavery.WebUI.Shared.Classes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class DUNSNumberAttribute : RegularExpressionAttribute
    {

        private const string pattern = @"^[0-9]{9}$";

        public DUNSNumberAttribute() : base(pattern)
        {
            ErrorMessage = Global.CompanyNumberRegexError;
        }

    }
}
