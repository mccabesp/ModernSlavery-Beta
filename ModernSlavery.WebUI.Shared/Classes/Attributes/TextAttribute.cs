using System;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class TextAttribute : XssValidationAttribute
    {
        private readonly SortedSet<char> _validChars=new SortedSet<char>();
        private readonly ValidCharSets _validCharSets = ValidCharSets.None;

        public TextAttribute(ValidCharSets validCharSets = ValidCharSets.All, string errorMessage = "Invalid characters: {badChars}") : base(errorMessage)
        {
            _validCharSets = validCharSets;
        }

        public TextAttribute(string validChars, string errorMessage = "Invalid characters: {badChars}") : base(errorMessage)
        {
            _validChars.AddRange(validChars);
        }

        [Flags]
        public enum ValidCharSets:byte
        {
            None=0,
            Upper =1,
            Lower =2,
            Number =4,
            Symbol =8,
            Punctuation =16,
            Whitespace =32,
            All=255
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            bool isValid = false;
            if (context.Model is string valueAsString)
            {
                isValid = Validate(context, valueAsString);
            }
            else if (context.Model is IEnumerable<string> enumerableValue)
            {
                isValid = !enumerableValue.Any(valueAsString => !Validate(context, valueAsString));
            }
            else
                isValid = true;

            //Return the error messages
            if (!isValid) yield return new ModelValidationResult(null, ErrorMessage);//Model name must be null
        }

        private bool Validate(ModelValidationContext context, string valueAsString)
        {
            if (string.IsNullOrWhiteSpace(valueAsString)) return true;

            var isValid = true;

            var textChars = new SortedSet<char>(valueAsString);
            var validCharacters = new SortedSet<char>();
            if (_validChars.Any())
                validCharacters.AddRange(textChars.Intersect(_validChars));
            else
            {
                if ((_validCharSets & ValidCharSets.Upper) != 0) validCharacters.AddRange(textChars.Where(c => char.IsUpper(c)));
                if ((_validCharSets & ValidCharSets.Lower) != 0) validCharacters.AddRange(textChars.Where(c => char.IsLower(c)));
                if ((_validCharSets & ValidCharSets.Number) != 0) validCharacters.AddRange(textChars.Where(c => char.IsNumber(c)));
                if ((_validCharSets & ValidCharSets.Symbol) != 0) validCharacters.AddRange(textChars.Where(c => char.IsSymbol(c)));
                if ((_validCharSets & ValidCharSets.Punctuation) != 0) validCharacters.AddRange(textChars.Where(c => char.IsPunctuation(c)));
                if ((_validCharSets & ValidCharSets.Whitespace) != 0) validCharacters.AddRange(textChars.Where(c => char.IsWhiteSpace(c)));
            }

            var invalidCharacters = new SortedSet<char>(textChars.Except(validCharacters));
            if (invalidCharacters.Any())
            {
                isValid = false;
                ErrorMessage = ResolveXssMessage(string.Concat(invalidCharacters));
            }

            //Log any Xss violation
            if (isValid && !XssValidate(context, valueAsString))
                isValid = false;

            return isValid;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public class SecurityCodeAttribute : TextAttribute
    {
        public SecurityCodeAttribute() : base(Text.AlphaNumericChars)
        {
        }
    }

    /// <summary>
    /// This property should never be used and serves as a placeholder for required refactoring
    /// </summary>
    [Obsolete("Properties containing this attribute should be refactored to prevent possible Xss injection")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class IgnoreTextAttribute : ValidationAttribute
    {
        public IgnoreTextAttribute() : base()
        {
        }

        public override bool IsValid(object value)
        {
            return true;
        }
    }

}