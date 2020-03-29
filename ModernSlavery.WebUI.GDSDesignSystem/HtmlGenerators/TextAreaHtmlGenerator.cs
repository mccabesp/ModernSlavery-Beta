using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    internal static class TextAreaHtmlGenerator
    {
        internal static IHtmlContent GenerateHtml<TModel>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, string>> propertyLambdaExpression,
            int? rows = null,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null
        )
            where TModel : GovUkViewModel
        {
            var property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);

            var propertyName = property.Name;

            var model = htmlHelper.ViewData.Model;

            var currentValue = ExtensionHelpers.GetCurrentValue(model, property, propertyLambdaExpression);


            var textAreaViewModel = new TextAreaViewModel
            {
                Name = $"GovUk_Text_{propertyName}",
                Id = $"GovUk_{propertyName}",
                Value = currentValue,
                Rows = rows,
                Label = labelOptions,
                Hint = hintOptions,
                FormGroup = formGroupOptions
            };

            if (model.HasErrorFor(property))
                textAreaViewModel.ErrorMessage = new ErrorMessageViewModel {Text = model.GetErrorFor(property)};

            return htmlHelper.Partial("/Components/Textarea.cshtml", textAreaViewModel);
        }
    }
}