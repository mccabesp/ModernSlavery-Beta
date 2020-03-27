using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernSlavery.WebUI.GDSDesignSystem.Helpers
{
    internal static class ExpressionHelpers
    {
        internal static PropertyInfo GetPropertyFromExpression<TModel, TProperty>(
            Expression<Func<TModel, TProperty>> propertyLambdaExpression)
        {
            var memberExpression = propertyLambdaExpression.Body as MemberExpression;
            return memberExpression.Member as PropertyInfo;
        }

        public static TProperty GetPropertyValueFromModelAndExpression<TModel, TProperty>(
            TModel model,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression)
        {
            var compiledExpression = propertyLambdaExpression.Compile();
            var currentPropertyValue = compiledExpression(model);
            return currentPropertyValue;
        }
    }
}