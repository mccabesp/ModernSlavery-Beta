using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.Parsers
{
    public static class GovUkRequestParser
    {
        public static void ParseAndValidateParameters<TModel, TProperty>(
            this TModel model,
            HttpRequest httpRequest,
            params Expression<Func<TModel, TProperty>>[] propertyLambdaExpressions)
            where TModel : GovUkViewModel
        {
            foreach (var propertyLambdaExpression in propertyLambdaExpressions)
                ParseAndValidateParameter(model, httpRequest, propertyLambdaExpression);
        }

        private static void ParseAndValidateParameter<TModel, TProperty>(
            TModel model,
            HttpRequest httpRequest,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression)
            where TModel : GovUkViewModel
        {
            var property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);

            //ThrowIfPropertyHasNonDefaultValue(model, property);

            if (TypeHelpers.IsNullableEnum(typeof(TProperty)))
                RadioToNullableEnumParser.ParseAndValidate(model, property, httpRequest);
            else if (TypeHelpers.IsListOfEnums(typeof(TProperty)))
                CheckboxToListOfEnumsParser.ParseAndValidate(model, property, httpRequest);
            else if (typeof(TProperty) == typeof(string))
                TextParser.ParseAndValidate(model, property, httpRequest);
            else if (typeof(TProperty) == typeof(int?))
                NullableIntParser.ParseAndValidate(model, property, httpRequest);
        }

        [Obsolete("This is not required as values are bound from inputs with same name and not prefixed with GovUk_")]
        private static void ThrowIfPropertyHasNonDefaultValue(object model, PropertyInfo property)
        {
            var currentValue = property.GetValue(model);
            var defaultValueOfThisType = TypeHelpers.GetDefaultValue(property.PropertyType);

            if (currentValue != defaultValueOfThisType)
                throw new Exception(
                    $"GovUkRequestParser is trying to set [{property.Name}] on object [{model.GetType()}]. " +
                    $"This property already has a value [{currentValue}], which is concerning (maybe a security problem). " +
                    "This value should only be set in one place (either directly by an HTTP parameter, or by one of the GovUk Parsers)."
                );
        }
    }
}