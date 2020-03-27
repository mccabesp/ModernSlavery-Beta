using System.Reflection;
using Microsoft.AspNetCore.Http;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Parsers
{
    public class TextParser
    {
        public static void ParseAndValidate(
            GovUkViewModel model,
            PropertyInfo property,
            HttpRequest httpRequest)
        {
            var parameterName = $"GovUk_Text_{property.Name}";

            var parameterValues = httpRequest.Form[parameterName];

            ParserHelpers.ThrowIfMoreThanOneValue(parameterValues, property);
            ParserHelpers.SaveUnparsedValueFromRequestToModel(model, httpRequest, parameterName);

            if (ParserHelpers.IsValueRequiredAndMissingOrEmpty(property, parameterValues))
            {
                ParserHelpers.AddRequiredAndMissingErrorMessage(model, property);
                return;
            }

            if (parameterValues.Count > 0)
            {
                var parameterValue = parameterValues[0];

                if (ExceedsCharacterCount(property, parameterValue))
                {
                    AddExceedsCharacterCountErrorMessage(model, property);
                    return;
                }

                property.SetValue(model, parameterValue);
            }

            model.ValueWasSuccessfullyParsed(property);
        }

        private static void AddExceedsCharacterCountErrorMessage(GovUkViewModel model, PropertyInfo property)
        {
            var characterCountAttribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();
            var displayNameForErrorsAttribute = property.GetSingleCustomAttribute<GovUkDisplayNameForErrorsAttribute>();

            string errorMessage;
            if (displayNameForErrorsAttribute != null)
                errorMessage =
                    $"{displayNameForErrorsAttribute.NameAtStartOfSentence} must be {characterCountAttribute.MaxCharacters} characters or fewer";
            else
                errorMessage = $"{property.Name} must be {characterCountAttribute.MaxCharacters} characters or fewer";

            model.AddErrorFor(property, errorMessage);
        }

        private static bool ExceedsCharacterCount(PropertyInfo property, string parameterValue)
        {
            var characterCountAttribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();

            var characterCountInForce = characterCountAttribute != null;

            if (characterCountInForce)
            {
                var parameterLength = parameterValue.Length;
                var maximumLength = characterCountAttribute.MaxCharacters;

                var exceedsCharacterCount = parameterLength > maximumLength;
                return exceedsCharacterCount;
            }

            return false;
        }
    }
}