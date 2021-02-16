using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        private static Type[] _textValidationAttributeTypes = new[] { typeof(TextAttribute), typeof(PhoneAttribute), typeof(EmailAddressAttribute), typeof(CompareAttribute), typeof(RegularExpressionAttribute), typeof(UrlAttribute), typeof(WebUrlAttribute), typeof(CreditCardAttribute), typeof(IgnoreTextAttribute)};
        public static IEnumerable<Attribute> GetTextValidationAttributes(this IEnumerable<Attribute> attributes)
        {
            foreach (var va in attributes)
            {
                var attributeType = va.GetType();
                if (_textValidationAttributeTypes.Any(tva => tva.IsAssignableFrom(attributeType))) yield return va;
            }
        }
        public static IEnumerable<Type> GetTextValidationAttributeTypes(this IEnumerable<Type> attributeTypes)
        {
            foreach (var attributeType in attributeTypes)
            {
                if (_textValidationAttributeTypes.Any(tva => tva.IsAssignableFrom(attributeType))) yield return attributeType;
            }
        }
        public static bool HasTextValidationAttribute(this IEnumerable<Attribute> attributes)
        {
            return attributes.GetTextValidationAttributes().Any();
        }
        public static IEnumerable<Attribute> GetValidationAttributes(this IEnumerable<Attribute> attributes)
        {
            var validationAttributeType = typeof(ValidationAttribute);
            foreach (var va in attributes)
            {
                if (validationAttributeType.IsAssignableFrom(va.GetType())) yield return va;
            }
        }
        public static bool HasValidationAttribute(this IEnumerable<Attribute> attributes)
        {
            return attributes.GetTextValidationAttributes().Any();
        }

        public static string GetModelFullName(this ModelMetadata modelMetadata)
        {
            if (modelMetadata.ContainerType != null) return $"{modelMetadata.ContainerType.Name}.{modelMetadata.Name}";
            return modelMetadata.Name;
        }
    }
}