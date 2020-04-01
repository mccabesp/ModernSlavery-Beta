using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    public class CheckboxItemHtmlGenerator
    {
        public static IHtmlContent GenerateHtml<TModel>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, bool>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            Conditional conditional = null,
            bool disabled = false)
        {
            var property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);
            var propertyName = property.Name;

            var model = htmlHelper.ViewData.Model;
            var isChecked = ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);

            if (labelOptions != null) labelOptions.For = propertyName;

            var checkboxItemViewModel = new CheckboxItemViewModel
            {
                Id = propertyName,
                Name = propertyName,
                Value = true.ToString(),
                Label = labelOptions,
                Hint = hintOptions,
                Conditional = conditional,
                Disabled = disabled,
                Checked = isChecked
            };

            return htmlHelper.Partial("~/Partials/CheckboxItem.cshtml", checkboxItemViewModel);
        }
    }
}