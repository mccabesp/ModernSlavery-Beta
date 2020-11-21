using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace ModernSlavery.WebUI.Shared.Classes.HtmlHelper
{
    public static partial class HtmlHelperExtensions
    {
        public static SharedOptions GetSharedOptions(this IHtmlHelper htmlHelper)
        {
            return (SharedOptions)htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(SharedOptions));
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

        public static HtmlString SetErrorClass<TModel>(
           this IHtmlHelper<TModel> htmlHelper,
           string[] fullHtmlFieldNames,
           string errorClassName,
           string noErrorClassName = null)
        {
            foreach (var fullHtmlFieldName in fullHtmlFieldNames)
            {
                var state = htmlHelper.ViewData.ModelState[fullHtmlFieldName];
                if (state != null && state.Errors.Count > 0) return new HtmlString(errorClassName);
            }
            return string.IsNullOrWhiteSpace(noErrorClassName) ? HtmlString.Empty : new HtmlString(noErrorClassName);
        }

        private static IObfuscator _obfuscator;
        private static IModelExpressionProvider _modelExpressionProvider;

        /// <summary>
        /// Returns a list of <input type="hidden"> elements for each public property in the underlying view model.
        /// The underlying view model class must have an [ViewModel(StateStores.ViewState)] attribute. 
        /// Properties to include are specified by using the [Include] or [Exclude] attributes but not both.
        /// </summary>
        /// <typeparam name="TModel">The type of the underlying view model</typeparam>
        /// <param name="htmlHelper">The Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper`1 instance this method extends.</param>
        /// <returns>A new Microsoft.AspNetCore.Html.IHtmlContent containing the <input> element.</returns>
        public static IHtmlContent HiddenViewModel<TModel>(this IHtmlHelper<TModel> htmlHelper)
        {
            var modelType = typeof(TModel);
            var viewModelAttribute = modelType.GetCustomAttribute<ViewModelAttribute>();
            if (viewModelAttribute==null) throw new ArgumentException($"[{nameof(ViewModelAttribute)}] required on class '{modelType.Name}'", modelType.Name);
            if (viewModelAttribute.StateStore!=ViewModelAttribute.StateStores.ViewState) throw new Exception($"To store view model in {nameof(ViewModelAttribute.StateStores.ViewState)} you must set [{nameof(ViewModelAttribute)}({nameof(viewModelAttribute.StateStore)}={nameof(ViewModelAttribute.StateStores.ViewState)})] on class '{modelType.Name}'");

            var viewModel = htmlHelper.ViewData.Model;

            var properties = viewModel.GetType().GetProperties(BindingFlags.Public);
            var includeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttributes().Where(attr => typeof(ViewStateAttribute).IsAssignableFrom(attr.GetType())).Cast<ViewStateAttribute>().Any(attr => attr.Action == ViewStateAttribute.ActionTypes.Include));
            var excludeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttributes().Where(attr => typeof(ViewStateAttribute).IsAssignableFrom(attr.GetType())).Cast<ViewStateAttribute>().Any(attr => attr.Action == ViewStateAttribute.ActionTypes.Exclude));

            if (!includeProperties.Any() && !excludeProperties.Any()) throw new Exception($"You must add at least one [{nameof(ViewStateAttribute)}], [{nameof(IncludeAttribute)}] or [{nameof(ExcludeAttribute)}] to the ViewModel '{modelType.Name}' public properties to specify which are to be included in ViewState");
            if (includeProperties.Any() && excludeProperties.Any()) throw new Exception($"You cannot both include and exclude properties from ViewState consider using either [{nameof(IncludeAttribute)}] or [{nameof(ExcludeAttribute)}] but not both");

            if (excludeProperties.Any()) includeProperties = properties.Except(excludeProperties);

            var complexProperties = includeProperties.Where(propertyInfo => propertyInfo.PropertyType.IsSimpleType());
            if (complexProperties.Any()) throw new Exception($"Cannot include the following complex properties in ViewState for view model '': {complexProperties.Select(p=>p.Name).ToDelimitedString()}.");

            var hiddenFields = new List<IHtmlContent>();
            foreach (var propertyInfo in includeProperties)
            {
                var secureAttribute = propertyInfo.GetCustomAttributes().FirstOrDefault(attr => typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;

                if (secureAttribute == null)
                    hiddenFields.Add(htmlHelper.Hidden(propertyInfo.Name));
                else
                    hiddenFields.Add(htmlHelper.HiddenSecured(propertyInfo.Name));
            }

            return hiddenFields.Any() ? new HtmlString(string.Join(Environment.NewLine,hiddenFields)) : null;
        }

        public static IHtmlContent HiddenSecured<TModel>(this IHtmlHelper<TModel> htmlHelper, string propertyName)
        {
            var viewModelType = typeof(TModel);
            var propertyInfo = viewModelType.GetProperty(propertyName);
            if (propertyInfo == null) throw new ArgumentOutOfRangeException(nameof(propertyName), $"Type '{viewModelType.Name}' does not have property named '{propertyName}'");

            var propertyModelType = propertyInfo.PropertyType;
            var viewModel = htmlHelper.ViewData.Model;

            if (!propertyInfo.PropertyType.IsSimpleType()) throw new ArgumentException($"Cannot add hidden secured form field for non-simple type '{viewModelType.Name}.{propertyName}'", propertyName);
            if (viewModelType.GetCustomAttribute<ViewModelAttribute>() == null) throw new ArgumentException($"[{nameof(ViewModelAttribute)}] required on class '{viewModelType.Name}' for hidden secured form fields", propertyName);

            var secureAttribute = propertyInfo.GetCustomAttributes().FirstOrDefault(attr => typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;
            if (secureAttribute == null) throw new ArgumentException($"[{nameof(SecuredAttribute)}], [{nameof(EncryptedAttribute)}] or [{nameof(ObfuscatedAttribute)}] required on property '{viewModelType.Name}.{propertyName}' for hidden secured form field", propertyName);

            var propertyModel = propertyInfo.GetValue(viewModel)?.ToString();

            if (!string.IsNullOrWhiteSpace(propertyModel))
            {
                if (secureAttribute.SecureMethod == SecuredAttribute.SecureMethods.Obfuscate)
                {
                    if (!propertyInfo.PropertyType.IsIntegerType()) throw new ArgumentException($"Cannot use obfuscation on non-integer type {propertyInfo.PropertyType.Name} property '{viewModelType.Name}.{propertyName}' for hidden secured form field", propertyName);
                    _obfuscator = _obfuscator ?? htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IObfuscator>();
                    propertyModel = _obfuscator.Obfuscate(propertyModel);
                }
                else
                {
                    propertyModel = Encryption.Encrypt(propertyModel);
                }
            }
            return htmlHelper.Hidden(propertyName, propertyModel);
        }

        public static IHtmlContent HiddenSecuredFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
        {
            _modelExpressionProvider = _modelExpressionProvider ?? htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelExpressionProvider>();
            var modelExpression = _modelExpressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);

            return htmlHelper.HiddenSecured(modelExpression.Metadata.Name);
        }

        public static HtmlString EnumDisplayNameFor<T>(this T item)
            where T : Enum
        {
            var type = item.GetType();
            var member = type.GetField(item.ToString());
            var displayName = (DisplayAttribute)member.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            if (displayName != null)
            {
                return new HtmlString(displayName.Description);
            }

            return new HtmlString(item.ToString());
        }

        #region Validation messages

        public static async Task<IHtmlContent> CustomValidationSummaryAsync(this IHtmlHelper helper,
            bool excludePropertyErrors = true,
            string validationSummaryMessage = "The following errors were detected",
            object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return await helper.PartialAsync("_CustomValidationSummary");
        }

        public static async Task<IHtmlContent> GovUkValidationSummaryAsync(this IHtmlHelper helper,
            bool excludePropertyErrors = true,
            string validationSummaryMessage = "The following errors were detected",
            object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return await helper.PartialAsync("_GovUkValidationSummary");
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
                        par1 = ((RangeAttribute)attribute).Minimum.ToString();
                        par2 = ((RangeAttribute)attribute).Maximum.ToString();
                    }
                    else if (attribute is DataTypeAttribute)
                    {
                        var type = ((DataTypeAttribute)attribute).DataType.ToString().ToLower();
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
                        par1 = ((MinLengthAttribute)attribute).Length.ToString();
                    }
                    else if (attribute is MaxLengthAttribute)
                    {
                        altAttr = "data-val-maxlength-alt";
                        par1 = ((MaxLengthAttribute)attribute).Length.ToString();
                    }
                    else if (attribute is StringLengthAttribute)
                    {
                        altAttr = "data-val-length-alt";
                        par1 = ((StringLengthAttribute)attribute).MinimumLength.ToString();
                        par2 = ((StringLengthAttribute)attribute).MaximumLength.ToString();
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

            return helper.EditorFor(expression, null, new { htmlAttributes = htmlAttr });
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