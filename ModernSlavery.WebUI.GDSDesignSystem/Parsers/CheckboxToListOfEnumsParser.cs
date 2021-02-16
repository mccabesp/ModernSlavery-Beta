using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Parsers
{
    public class CheckboxToListOfEnumsParser
    {
        internal static void ParseAndValidate(
            GovUkViewModel model,
            PropertyInfo property,
            HttpRequest httpRequest)
        {
            var propertyName = property.Name;
            var parameterValues = httpRequest.Form[propertyName];

            ThrowIfPropertyTypeIsNotListOfEnums(property);

            var enumType = TypeHelpers.GetGenericTypeFromGenericListType(property.PropertyType);
            ThrowIfAnyValuesAreInvalid(parameterValues, enumType);

            SetPropertyValue(model, property, parameterValues);

            if (IsTooFewSelected(property, parameterValues) ||
                IsTooManySelected(property, parameterValues))
            {
                AddInvalidNumberOfSelectedItemsErrorMessage(model, property, parameterValues);
                return;
            }

            model.ValueWasSuccessfullyParsed(property);
        }

        private static void SetPropertyValue(GovUkViewModel model, PropertyInfo property, StringValues parameterValues)
        {
            var enumType = TypeHelpers.GetGenericTypeFromGenericListType(property.PropertyType);

            var newListOfEnums = Activator.CreateInstance(property.PropertyType);
            var methodInfo = newListOfEnums.GetType().GetMethod("Add");

            foreach (var parameterValue in parameterValues)
            {
                var valueAsEnum = Enum.Parse(enumType, parameterValue);
                methodInfo.Invoke(newListOfEnums, new[] {valueAsEnum});
            }

            property.SetValue(model, newListOfEnums);
        }

        private static void AddInvalidNumberOfSelectedItemsErrorMessage(
            GovUkViewModel model,
            PropertyInfo property,
            StringValues parameterValues)
        {
            var responsesRangeAttribute =
                property.GetSingleCustomAttribute<GovUkValidateCheckboxNumberOfResponsesRangeAttribute>();
            var displayNameForErrorsAttribute = property.GetSingleCustomAttribute<GovUkDisplayNameForErrorsAttribute>();

            var propertyNameForErrorMessage = displayNameForErrorsAttribute?.NameWithinSentence ?? property.Name;

            string errorMessage;
            if (IsTooFewSelected(property, parameterValues))
            {
                if (parameterValues.Count == 0 &&
                    responsesRangeAttribute.ErrorMessageIfNothingSelected != null)
                    errorMessage = responsesRangeAttribute.ErrorMessageIfNothingSelected;
                else
                    errorMessage = $"Select at least {responsesRangeAttribute.MinimumSelected} " +
                                   $"options for {propertyNameForErrorMessage}";
            }
            else // Implicitly, the user must have selected too many options
            {
                errorMessage = $"Select at most {responsesRangeAttribute.MaximumSelected} " +
                               $"options for {propertyNameForErrorMessage}";
            }

            model.AddErrorFor(property, errorMessage);
        }

        private static bool IsTooFewSelected(PropertyInfo property, StringValues parameterValues)
        {
            var responsesRangeAttribute =
                property.GetSingleCustomAttribute<GovUkValidateCheckboxNumberOfResponsesRangeAttribute>();

            var isRangeEnforced = responsesRangeAttribute != null &&
                                  responsesRangeAttribute.MinimumSelected.HasValue;

            var tooFewSelected = isRangeEnforced &&
                                 parameterValues.Count < responsesRangeAttribute.MinimumSelected.Value;

            return tooFewSelected;
        }

        private static bool IsTooManySelected(PropertyInfo property, StringValues parameterValues)
        {
            var responsesRangeAttribute =
                property.GetSingleCustomAttribute<GovUkValidateCheckboxNumberOfResponsesRangeAttribute>();

            var isRangeEnforced = responsesRangeAttribute != null &&
                                  responsesRangeAttribute.MaximumSelected.HasValue;

            var tooManySelected = isRangeEnforced &&
                                  parameterValues.Count > responsesRangeAttribute.MaximumSelected.Value;

            return tooManySelected;
        }

        private static void ThrowIfPropertyTypeIsNotListOfEnums(PropertyInfo property)
        {
            if (!TypeHelpers.IsListOfEnums(property.PropertyType))
                throw new ArgumentException(
                    "CheckboxToListOfEnumsParser can only be used on List<Enum> properties, " +
                    $"but was actually used on property [{property.Name}] of type [{property.PropertyType.FullName}] "
                );
        }

        private static void ThrowIfAnyValuesAreInvalid(StringValues parameterValues, Type enumType)
        {
            try
            {
                foreach (var parameterValue in parameterValues) Enum.Parse(enumType, parameterValue);
            }
            catch (Exception ex)
            {
                var sentValues = string.Join(", ", parameterValues);
                var allowedValues = string.Join(",", Enum.GetNames(enumType));

                throw new ArgumentException(
                    $"User sent invalid value for Enum type [{enumType.Name}] " +
                    $"sent values [{sentValues}] allowed values are [{allowedValues}]",
                    ex
                );
            }
        }
    }
}