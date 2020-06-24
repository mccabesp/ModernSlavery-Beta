using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static SharedOptions GetSharedOptions(this IHtmlHelper htmlHelper)
        {
            return (SharedOptions) htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(SharedOptions));
        }


        public static async Task<IHtmlContent> PartialModelAsync<T>(this IHtmlHelper htmlHelper, T viewModel)
        {
            // extract the parial path from the model class attr
            var partialPath = viewModel.GetAttribute<PartialAttribute>().PartialPath;
            return await htmlHelper.PartialAsync(partialPath, viewModel);
        }

        public static HtmlString PageIdentifier(this IHtmlHelper htmlHelper)
        {
            var sharedOptions = htmlHelper.GetSharedOptions();
            return new HtmlString(
                $"Date:{VirtualDateTime.Now}, Version:{sharedOptions.Version}, File Date:{sharedOptions.AssemblyDate.ToLocalTime()}, Environment:{sharedOptions.Environment}, Machine:{Environment.MachineName}, Instance:{sharedOptions.Website_Instance_Id}, {sharedOptions.AssemblyCopyright}");
        }

        public static HtmlString ToHtml(this IHtmlHelper htmlHelper, string text)
        {
            text = htmlHelper.Encode(text);
            text = text.Replace(Environment.NewLine, "<br/>");
            return new HtmlString(text);
        }

        public static HtmlString SetErrorClass<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            string errorClassName,
            string noErrorClassName = null)
        {
            var expressionText = htmlHelper.GetExpressionText(expression);
            var fullHtmlFieldName = htmlHelper.ViewContext.ViewData
                .TemplateInfo.GetFullHtmlFieldName(expressionText);

            return SetErrorClass(htmlHelper, fullHtmlFieldName, errorClassName, noErrorClassName);
        }

        public static HtmlString SetErrorClass<TModel>(
            this IHtmlHelper<TModel> htmlHelper,
            string fullHtmlFieldName,
            string errorClassName,
            string noErrorClassName = null)
        {
            var state = htmlHelper.ViewData.ModelState[fullHtmlFieldName];

            if (!string.IsNullOrWhiteSpace(noErrorClassName))
                return state == null || state.Errors.Count == 0
                    ? new HtmlString(noErrorClassName)
                    : new HtmlString(errorClassName);

            return state == null || state.Errors.Count == 0 ? HtmlString.Empty : new HtmlString(errorClassName);
        }


        public static HtmlString HiddenFor<TModel>(this IHtmlHelper<TModel> helper, params string[] propertyNames)
        {
            var hidden = new List<string>();
            var model = helper.ViewData.Model;
            foreach (var propertyName in propertyNames)
            {
                var propertyValue = model.GetProperty<string>(propertyName);
                hidden.Add(helper.Hidden(propertyName, propertyValue).ToString());
            }

            return new HtmlString(string.Join("\r\n", hidden));
        }

        #region Asset Bundles

        public static HtmlString UnpackBundle(this IHtmlHelper htmlHelper, string bundlePath, string media = "")
        {
            var bundle = Bundle.ReadBundleFile("bundleconfig.json", bundlePath);
            if (bundle == null) return null;

            var outputString = bundlePath.EndsWith(".js")
                ? bundle.InputFiles.Select(inputFile => $"<script src='/{inputFile}' type='text/javascript'></script>")
                : bundle.InputFiles.Select(inputFile =>
                    $"<link rel='stylesheet' type='text/css' media='{media}' href='/{inputFile}' />");

            return new HtmlString(string.Join("\n", outputString));
        }

        #endregion

        #region Validation messages

        public static async Task<IHtmlContent> CustomValidationSummaryAsync(this IHtmlHelper helper,
            bool excludePropertyErrors = true,
            string validationSummaryMessage = "The following errors were detected",
            object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return await helper.PartialAsync("_ValidationSummary");
        }
        public static string GetExpressionText<TModel, TResult>(
         this IHtmlHelper<TModel> htmlHelper,
         Expression<Func<TModel, TResult>> expression)
        {
            var expresionProvider = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;

            return expresionProvider.GetExpressionText(expression);
        }

        private static Dictionary<string, object> CustomAttributesFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes = null)
        {
            var containerType = typeof(TModel);

            var propertyName = htmlHelper.GetExpressionText(expression);
            var propertyInfo = containerType.GetPropertyInfo(propertyName);

            var displayNameAttribute =
                propertyInfo?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as
                    DisplayNameAttribute;
            var displayAttribute =
                propertyInfo?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            var displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName :
                displayAttribute != null ? displayAttribute.Name : propertyName;

            string par1 = null;
            string par2 = null;

            var htmlAttr = htmlAttributes.ToPropertyDictionary();
            if (propertyInfo != null)
                foreach (ValidationAttribute attribute in propertyInfo.GetCustomAttributes(typeof(ValidationAttribute),
                    false))
                {
                    var validatorKey =
                        $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    var customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null) continue;

                    //Set the message from the description
                    if (attribute.ErrorMessage != customError.Description)
                        attribute.ErrorMessage = customError.Description;

                    //Set the inline error message
                    var errorMessageString = Misc.GetPropertyValue(attribute, "ErrorMessageString") as string;
                    if (string.IsNullOrWhiteSpace(errorMessageString)) errorMessageString = attribute.ErrorMessage;

                    //Set the summary error message
                    if (customError.Title != errorMessageString) errorMessageString = customError.Title;

                    //Set the display name
                    if (!string.IsNullOrWhiteSpace(customError.DisplayName) && customError.DisplayName != displayName)
                    {
                        if (displayAttribute != null)
                            Misc.SetPropertyValue(displayAttribute, "Name", customError.DisplayName);

                        displayName = customError.DisplayName;
                    }

                    string altAttr = null;
                    if (attribute is RequiredAttribute)
                    {
                        altAttr = "data-val-required-alt";
                    }
                    else if (attribute is CompareAttribute)
                    {
                        altAttr = "data-val-equalto-alt";
                    }
                    else if (attribute is RegularExpressionAttribute)
                    {
                        altAttr = "data-val-regex-alt";
                    }
                    else if (attribute is RangeAttribute)
                    {
                        altAttr = "data-val-range-alt";
                        par1 = ((RangeAttribute) attribute).Minimum.ToString();
                        par2 = ((RangeAttribute) attribute).Maximum.ToString();
                    }
                    else if (attribute is DataTypeAttribute)
                    {
                        var type = ((DataTypeAttribute) attribute).DataType.ToString().ToLower();
                        switch (type)
                        {
                            case "password":
                                continue;
                            case "emailaddress":
                                type = "email";
                                break;
                            case "phonenumber":
                                type = "phone";
                                break;
                        }

                        altAttr = $"data-val-{type}-alt";
                    }
                    else if (attribute is MinLengthAttribute)
                    {
                        altAttr = "data-val-minlength-alt";
                        par1 = ((MinLengthAttribute) attribute).Length.ToString();
                    }
                    else if (attribute is MaxLengthAttribute)
                    {
                        altAttr = "data-val-maxlength-alt";
                        par1 = ((MaxLengthAttribute) attribute).Length.ToString();
                    }
                    else if (attribute is StringLengthAttribute)
                    {
                        altAttr = "data-val-length-alt";
                        par1 = ((StringLengthAttribute) attribute).MinimumLength.ToString();
                        par2 = ((StringLengthAttribute) attribute).MaximumLength.ToString();
                    }

                    htmlAttr[altAttr.TrimSuffix("-alt")] =
                        string.Format(attribute.ErrorMessage, displayName, par1, par2);
                    htmlAttr[altAttr] = string.Format(errorMessageString, displayName, par1, par2);
                }

            return htmlAttr;
        }

        public static IHtmlContent CustomEditorFor<TModel, TProperty>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            object htmlAttributes = null)
        {
            var htmlAttr = helper.CustomAttributesFor(expression, htmlAttributes);

            return helper.EditorFor(expression, null, new {htmlAttributes = htmlAttr});
        }

        public static IHtmlContent CustomRadioButtonFor<TModel, TProperty>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TProperty>> expression,
            object value,
            object htmlAttributes = null)
        {
            var htmlAttr = helper.CustomAttributesFor(expression, htmlAttributes);

            return helper.RadioButtonFor(expression, value, htmlAttr);
        }

        #endregion
    }
}