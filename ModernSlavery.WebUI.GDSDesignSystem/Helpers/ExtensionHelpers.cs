using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Helpers
{
    public static class ExtensionHelpers
    {
        public static TAttributeType GetSingleCustomAttribute<TAttributeType>(this MemberInfo property)
            where TAttributeType : Attribute
        {
            return property.GetCustomAttributes(typeof(TAttributeType)).SingleOrDefault() as TAttributeType;
        }

        public static string GetCurrentValue<TModel, TProperty>(
            TModel model,
            PropertyInfo property,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression)
            where TModel : GovUkViewModel
        {
            var propertyValue =
                ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);
            if (model.HasSuccessfullyParsedValue(property))
                return propertyValue.ToString();
            if (propertyValue != null) return propertyValue.ToString();

            var parameterName = $"GovUk_Text_{property.Name}";
            var unparsedValues = model.GetUnparsedValues(parameterName);

            var unparsedValueOrNull = unparsedValues.Count > 0 ? unparsedValues[0] : null;
            return unparsedValueOrNull;
        }
    }
}