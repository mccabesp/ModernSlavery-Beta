using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Parsers
{
    internal static class RadioToNullableEnumParser
    {
        internal static void ParseAndValidate(
            GovUkViewModel model,
            PropertyInfo property,
            HttpRequest httpRequest)
        {
            var propertyName = $"GovUk_Radio_{property.Name}";
            var parameterValues = httpRequest.Form[propertyName];

            ThrowIfPropertyTypeIsNotNullableEnum(property);
            ParserHelpers.ThrowIfMoreThanOneValue(parameterValues, property);

            if (ParserHelpers.IsValueRequiredAndMissing(property, parameterValues))
            {
                ParserHelpers.AddRequiredAndMissingErrorMessage(model, property);
                return;
            }

            if (parameterValues.Count > 0)
            {
                var parameterValueAsString = parameterValues[0];

                var parameterAsEnum = ParseParameterAsEnum(parameterValueAsString, property);

                property.SetValue(model, parameterAsEnum);
            }

            model.ValueWasSuccessfullyParsed(property);
        }

        private static void ThrowIfPropertyTypeIsNotNullableEnum(PropertyInfo property)
        {
            if (!TypeHelpers.IsNullableEnum(property.PropertyType))
                throw new ArgumentException(
                    "RadioToNullableEnumParser can only be used on Nullable Enum properties, " +
                    $"but was actually used on property [{property.Name}] of type [{property.PropertyType.FullName}] "
                );
        }

        private static object ParseParameterAsEnum(string parameterValueAsString, PropertyInfo property)
        {
            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
            object parameterAsEnum;
            try
            {
                parameterAsEnum = Enum.Parse(underlyingType, parameterValueAsString);
            }
            catch (Exception ex)
            {
                var allowedValues = string.Join(",", Enum.GetNames(underlyingType));
                throw new ArgumentException(
                    $"User sent invalid value for Enum type [{property.Name}] " +
                    $"sent value [{parameterValueAsString}] allowed values are [{allowedValues}]",
                    ex
                );
            }

            return parameterAsEnum;
        }
    }
}