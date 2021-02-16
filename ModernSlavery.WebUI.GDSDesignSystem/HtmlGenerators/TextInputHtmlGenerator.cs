using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    internal static class TextInputHtmlGenerator
    {
        internal static IHtmlContent GenerateHtml<TModel, TProperty>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression,
            LabelViewModel labelOptions = null,
            HintViewModel hintOptions = null,
            FormGroupViewModel formGroupOptions = null,
            string classes = null,
            TextInputAppendixViewModel textInputAppendix = null
        )
            where TModel : GovUkViewModel
        {
            var property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);

            var propertyName = property.Name;

            var model = htmlHelper.ViewData.Model;

            var id = $"GovUk_{propertyName}";
            var currentValue = ExtensionHelpers.GetCurrentValue(model, property, propertyLambdaExpression);

            if (labelOptions != null) labelOptions.For = id;

            var textInputViewModel = new TextInputViewModel
            {
                Name = propertyName,
                Id = id,
                Value = currentValue,
                Label = labelOptions,
                Hint = hintOptions,
                FormGroup = formGroupOptions,
                Classes = classes,
                TextInputAppendix = textInputAppendix
            };

            if (model.HasErrorFor(property))
                textInputViewModel.ErrorMessage = new ErrorMessageViewModel {Text = model.GetErrorFor(property)};

            return htmlHelper.Partial("~/Partials/TextInput.cshtml", textInputViewModel);
        }
    }
}