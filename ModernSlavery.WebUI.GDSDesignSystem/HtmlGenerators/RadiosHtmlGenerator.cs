using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Helpers;
using ModernSlavery.WebUI.GDSDesignSystem.Models;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    internal static class RadiosHtmlGenerator
    {
        public static IHtmlContent GenerateHtml<TModel, TProperty>(
            IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> propertyLambdaExpression,
            FieldsetViewModel fieldsetOptions = null,
            HintViewModel hintOptions = null)
            where TModel : GovUkViewModel
        {
            var property = ExpressionHelpers.GetPropertyFromExpression(propertyLambdaExpression);
            ThrowIfPropertyTypeIsNotNullableEnum(property);
            var propertyName = property.Name;

            var model = htmlHelper.ViewData.Model;
            var currentlySelectedValue =
                ExpressionHelpers.GetPropertyValueFromModelAndExpression(model, propertyLambdaExpression);

            var enumType = Nullable.GetUnderlyingType(typeof(TProperty));
            var allEnumValues = Enum.GetValues(enumType);


            var radios = allEnumValues
                .OfType<object>()
                .Select(enumValue =>
                {
                    var isEnumValueCurrentlySelected = enumValue.ToString() == currentlySelectedValue.ToString();
                    var radioLabelText = GetRadioLabelText(enumType, enumValue);

                    return new RadioItemViewModel
                    {
                        Value = enumValue.ToString(),
                        Id = $"GovUk_Radio_{propertyName}_{enumValue}",
                        Checked = isEnumValueCurrentlySelected,
                        Label = new LabelViewModel
                        {
                            Text = radioLabelText
                        }
                    };
                })
                .Cast<ItemViewModel>()
                .ToList();

            var radiosViewModel = new RadiosViewModel
            {
                Name = $"GovUk_Radio_{propertyName}",
                IdPrefix = $"GovUk_{propertyName}",
                Items = radios,
                Fieldset = fieldsetOptions,
                Hint = hintOptions
            };
            if (model.HasErrorFor(property))
                radiosViewModel.ErrorMessage = new ErrorMessageViewModel
                {
                    Text = model.GetErrorFor(property)
                };

            return htmlHelper.Partial("/Components/Radios.cshtml", radiosViewModel);
        }

        private static void ThrowIfPropertyTypeIsNotNullableEnum(PropertyInfo property)
        {
            if (!TypeHelpers.IsNullableEnum(property.PropertyType))
                throw new ArgumentException(
                    "GovUkRadiosFor can only be used on Nullable Enum properties, " +
                    $"but was actually used on property [{property.Name}] of type [{property.PropertyType.FullName}] "
                );
        }

        private static string GetRadioLabelText(Type enumType, object enumValue)
        {
            var textFromAttribute = GovUkRadioCheckboxLabelTextAttribute.GetValueForEnum(enumType, enumValue);

            var radioLabel = textFromAttribute ?? enumValue.ToString();

            return radioLabel;
        }
    }
}