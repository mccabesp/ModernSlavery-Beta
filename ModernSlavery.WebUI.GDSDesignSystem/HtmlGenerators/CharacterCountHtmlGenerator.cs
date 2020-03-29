using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    internal static class CharacterCountHtmlGenerator
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
            ThrowIfPropertyDoesNotHaveCharacterCountAttribute(property);

            var propertyName = property.Name;

            var model = htmlHelper.ViewData.Model;

            var currentValue = ExtensionHelpers.GetCurrentValue(model, property, propertyLambdaExpression);

            var maximumCharacters = GetMaximumCharacters(property);

            var characterCountViewModel = new CharacterCountViewModel
            {
                Name = $"GovUk_Text_{propertyName}",
                Id = $"GovUk_{propertyName}",
                MaxLength = maximumCharacters,
                Value = currentValue,
                Rows = rows,
                Label = labelOptions,
                Hint = hintOptions,
                FormGroup = formGroupOptions
            };

            if (model.HasErrorFor(property))
                characterCountViewModel.ErrorMessage = new ErrorMessageViewModel {Text = model.GetErrorFor(property)};

            return htmlHelper.Partial("/Components/CharacterCount.cshtml", characterCountViewModel);
        }

        private static void ThrowIfPropertyDoesNotHaveCharacterCountAttribute(PropertyInfo property)
        {
            var attribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();

            if (attribute == null)
                throw new ArgumentException(
                    "GovUkCharacterCountFor can only be used on properties that are decorated with a GovUkValidateCharacterCount attribute. "
                    + $"Property [{property.Name}] on type [{property.DeclaringType.FullName}] does not have this attribute");
        }

        private static int GetMaximumCharacters(PropertyInfo property)
        {
            var attribute = property.GetSingleCustomAttribute<GovUkValidateCharacterCountAttribute>();
            return attribute.MaxCharacters;
        }
    }
}