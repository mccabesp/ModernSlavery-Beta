using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public static partial class Extensions
    {

        public static string FormatError(this ValidationAttribute attribute, string error, string displayName)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return error;
            }

            string par1 = null;
            string par2 = null;

            if (attribute is RangeAttribute)
            {
                par1 = ((RangeAttribute)attribute).Minimum.ToString();
                par2 = ((RangeAttribute)attribute).Maximum.ToString();
            }
            else if (attribute is MinLengthAttribute)
            {
                par1 = ((MinLengthAttribute)attribute).Length.ToString();
            }
            else if (attribute is MaxLengthAttribute)
            {
                par1 = ((MaxLengthAttribute)attribute).Length.ToString();
            }
            else if (attribute is StringLengthAttribute)
            {
                par1 = ((StringLengthAttribute)attribute).MinimumLength.ToString();
                par2 = ((StringLengthAttribute)attribute).MaximumLength.ToString();
            }

            return string.Format(error, displayName, par1, par2);
        }

    }
}
