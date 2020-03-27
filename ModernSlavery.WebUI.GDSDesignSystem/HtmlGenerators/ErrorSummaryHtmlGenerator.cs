using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Partials;

namespace ModernSlavery.WebUI.GDSDesignSystem.HtmlGenerators
{
    public static class ErrorSummaryHtmlGenerator
    {
        public static IHtmlContent GenerateHtml<TModel>(
            IHtmlHelper<TModel> htmlHelper,
            string[] orderOfPropertyNamesInTheView)
            where TModel : GovUkViewModel
        {
            var model = htmlHelper.ViewData.Model;

            if (!model.HasAnyErrors()) return null;

            var errors = model.GetAllErrors();

            var orderedPropertyNames = GetErroredPropertyNamesInSpecifiedOrder(errors, orderOfPropertyNamesInTheView);

            var errorSummaryItems = orderedPropertyNames
                .Select(propertyName =>
                    new ErrorSummaryItemViewModel
                    {
                        Href = $"#GovUk_{propertyName}-error",
                        Text = errors[propertyName]
                    })
                .ToList();

            var errorSummaryViewModel = new ErrorSummaryViewModel
            {
                Title = new ErrorSummaryTitle
                {
                    Text = "There is a problem"
                },
                Errors = errorSummaryItems
            };

            return htmlHelper.Partial("/Components/ErrorSummary.cshtml", errorSummaryViewModel);
        }

        private static List<string> GetErroredPropertyNamesInSpecifiedOrder(
            Dictionary<string, string> errors,
            string[] orderOfPropertyNamesInTheView)
        {
            var erroredPropertyNames = errors.Keys.ToList();

            var erroredPropertyNamesWithSpecifiedOrder =
                orderOfPropertyNamesInTheView.Where(p => erroredPropertyNames.Contains(p)).ToList();

            var erroredPropertyNamesWithoutSpecifiedOrder =
                erroredPropertyNames.Except(erroredPropertyNamesWithSpecifiedOrder).ToList();

            var orderedPropertyNames =
                erroredPropertyNamesWithSpecifiedOrder.Concat(erroredPropertyNamesWithoutSpecifiedOrder).ToList();

            return orderedPropertyNames;
        }
    }
}