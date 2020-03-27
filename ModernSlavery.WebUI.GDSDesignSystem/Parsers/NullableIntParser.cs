using System.Reflection;
using Microsoft.AspNetCore.Http;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Parsers
{
    public class NullableIntParser
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

                if (!double.TryParse(parameterValue, out _))
                {
                    ParserHelpers.AddErrorMessageBasedOnPropertyDisplayName(model, property,
                        name => $"{name} must be a number");
                    return;
                }

                int parsedIntValue;
                if (!int.TryParse(parameterValue, out parsedIntValue))
                {
                    ParserHelpers.AddErrorMessageBasedOnPropertyDisplayName(model, property,
                        name => $"{name} must be a whole number");
                    return;
                }

                property.SetValue(model, parsedIntValue);
            }

            model.ValueWasSuccessfullyParsed(property);
        }
    }
}