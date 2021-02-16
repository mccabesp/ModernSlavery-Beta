using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class WebUrlAttribute : XssValidationAttribute
    {
        private readonly UriKind _uriKind;
        private const string AuthorityPattern = @"^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)";
        private const string PathAndQueryPattern = @"(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$";
        private static string AbsoluteUrlPattern = @$"{AuthorityPattern}{PathAndQueryPattern}";
        private static string RelativeUrlPattern = @$"^{PathAndQueryPattern}";

        public WebUrlAttribute(UriKind uriKind = UriKind.Absolute) : base(GetErrorMessage(uriKind), "Invalid characters: {badChars}")
        {
            _uriKind = uriKind;
        }

        private static string GetErrorMessage(UriKind uriKind)
        {
            switch (uriKind)
            {
                case UriKind.Absolute:
                    return "Enter a valid fully-qualified http, https, or ftp URL.";
                case UriKind.Relative:
                    return "Enter a valid relative URL.";
                case UriKind.RelativeOrAbsolute:
                default:
                    return "Enter a valid fully-qualified http, https, ftp or relative URL.";
            }
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var valueAsString = context.Model as string;
            if (string.IsNullOrWhiteSpace(valueAsString)) yield break;

            var isValid = Uri.TryCreate(valueAsString, _uriKind, out _);

            if (isValid)
                switch (_uriKind)
                {
                    case UriKind.Absolute:
                        isValid = Regex.IsMatch(valueAsString, AbsoluteUrlPattern, RegexOptions.IgnoreCase);
                        break;
                    case UriKind.Relative:
                        isValid = Regex.IsMatch(valueAsString, RelativeUrlPattern, RegexOptions.IgnoreCase);
                        break;
                    case UriKind.RelativeOrAbsolute:
                    default:
                        isValid = Regex.IsMatch(valueAsString, AbsoluteUrlPattern) || Regex.IsMatch(valueAsString, RelativeUrlPattern);
                        break;
                }

            if (!isValid)
                ErrorMessage = ErrorMessageString;

            //Log any Xss violation
            else if (!XssValidate(context, valueAsString))
                isValid = false;

            //Return the error messages
            if (!isValid)yield return new ModelValidationResult(null, ErrorMessage);//Model name must be null

        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RelativeUrlAttribute : WebUrlAttribute
    {
        public RelativeUrlAttribute() : base(UriKind.Relative)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AbsoluteUrlAttribute : WebUrlAttribute
    {
        public AbsoluteUrlAttribute() : base(UriKind.Absolute)
        {

        }
    }
}