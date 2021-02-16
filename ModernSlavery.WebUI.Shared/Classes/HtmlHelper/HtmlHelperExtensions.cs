using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;
using ModernSlavery.WebUI.Shared.Models;
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
        public static IHtmlContent AddViewState<TModel>(this IHtmlHelper<TModel> htmlHelper)
        {
            var modelType = typeof(TModel);
            var formViewStateAttribute = modelType.GetCustomAttribute<FormViewStateAttribute>();
            if (formViewStateAttribute==null) throw new ArgumentException($"[{nameof(FormViewStateAttribute)}] required on class '{modelType.Name}'", modelType.Name);
            if (modelType.GetCustomAttribute<SessionViewStateAttribute>() != null) throw new Exception($"Cannot have both [{nameof(FormViewStateAttribute)}] and [{nameof(SessionViewStateAttribute)}] on class '{modelType.Name}'");

            var viewModel = htmlHelper.ViewData.Model;

            var properties = modelType.GetProperties(BindingFlags.Public);
            var includeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttribute<IncludeViewStateAttribute>(false) !=null);
            var excludeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttribute<ExcludeViewStateAttribute>(false) != null);
            if (!includeProperties.Any() && !excludeProperties.Any()) throw new Exception($"You must add at least one [{nameof(IncludeViewStateAttribute)}] or [{nameof(ExcludeViewStateAttribute)}] to the ViewModel '{modelType.Name}' public properties to specify which are to be included in ViewState");
            CheckViewStateMutex(modelType);

            includeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttribute<IncludeViewStateAttribute>(true) != null);
            excludeProperties = properties.Where(propertyInfo => propertyInfo.GetCustomAttribute<ExcludeViewStateAttribute>(true) != null);
            if (excludeProperties.Any()) includeProperties = properties.Except(excludeProperties);

            var complexProperties = includeProperties.Where(propertyInfo => propertyInfo.PropertyType.IsSimpleType());
            if (complexProperties.Any()) throw new Exception($"You must add [{nameof(ExcludeViewStateAttribute)}] to the following complex properties : {complexProperties.Select(p=>p.Name).ToDelimitedString()} if type {modelType.Name}");

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

        private static void CheckViewStateMutex(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public);
            var includeProperties = properties.Any(propertyInfo => propertyInfo.GetCustomAttribute<IncludeViewStateAttribute>(false) != null);
            var excludeProperties = properties.Any(propertyInfo => propertyInfo.GetCustomAttribute<ExcludeViewStateAttribute>(false) != null);
            if (includeProperties && excludeProperties) throw new Exception($"You cannot have both [{nameof(IncludeViewStateAttribute)}] and [{nameof(ExcludeViewStateAttribute)}] on the ViewModel '{type.Name}'");
            if (type.BaseType!=null) CheckViewStateMutex(type.BaseType);
        }

        public static IHtmlContent HiddenSecured<TModel>(this IHtmlHelper<TModel> htmlHelper, string propertyName)
        {
            var viewModelType = typeof(TModel);
            if (!typeof(BaseViewModel).IsAssignableFrom(viewModelType)) throw new Exception($"Cannot add hidden secured form field for class '{viewModelType.Name}' which is not derived from class '{nameof(BaseViewModel)}'");
            if (viewModelType.GetCustomAttribute<SessionViewStateAttribute>() != null) throw new ArgumentException($"Cannot add hidden secured form field for class '{viewModelType.Name}' which has [{nameof(SessionViewStateAttribute)}]", propertyName);

            var propertyInfo = viewModelType.GetProperty(propertyName);
            if (propertyInfo == null) throw new ArgumentOutOfRangeException(nameof(propertyName), $"Type '{viewModelType.Name}' does not have property named '{propertyName}'");

            var propertyModelType = propertyInfo.PropertyType;
            var viewModel = htmlHelper.ViewData.Model;

            if (propertyInfo.PropertyType.GetCustomAttribute<ExcludeViewStateAttribute>() != null) throw new ArgumentException($"You cannot have [{nameof(ExcludeViewStateAttribute)}] on hidden secured form field", propertyName);

            
            var secureAttribute = propertyInfo.GetCustomAttributes().FirstOrDefault(attr => typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;
            if (secureAttribute == null) throw new ArgumentException($"[{nameof(SecuredAttribute)}] is required in class '{viewModelType.Name}' for hidden secured form field property", propertyName);

            var propertyModel = propertyInfo.GetValue(viewModel);
            if (propertyModel!=null)
            {
                string hiddenValue;
                if (secureAttribute.SecureMethod == SecuredAttribute.SecureMethods.Obfuscate)
                {
                    if (!propertyInfo.PropertyType.IsIntegerType()) throw new ArgumentException($"Cannot use obfuscation on non-integer type {propertyInfo.PropertyType.Name} property '{viewModelType.Name}.{propertyName}' for hidden secured form field", propertyName);
                    _obfuscator = _obfuscator ?? htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IObfuscator>();
                    hiddenValue = _obfuscator.Obfuscate(propertyModel.ToString());
                }
                else
                {
                    hiddenValue = propertyInfo.PropertyType.IsSimpleType() ? propertyModel.ToString() : Json.SerializeObject(propertyModel);
                    hiddenValue = Encryption.Encrypt(hiddenValue);
                }
                return htmlHelper.Hidden(propertyName, hiddenValue);
            }
            return new HtmlString(string.Empty);
        }

        public static IHtmlContent HiddenSecuredFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
        {
            _modelExpressionProvider = _modelExpressionProvider ?? htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelExpressionProvider>();
            var modelExpression = _modelExpressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);

            return htmlHelper.HiddenSecured(modelExpression.Metadata.Name);
        }

        public static IHtmlContent Replace(this IHtmlHelper htmlHelper, string content, string oldValue, string newValue)
        {
            if (!string.IsNullOrWhiteSpace(content))content = htmlHelper.Encode(content);
            return new HtmlString(content.ReplaceI(oldValue, newValue));
        }

        public static IHtmlContent Linebreak(this IHtmlHelper htmlHelper, IEnumerable<string> lines, string prefix = null)
        {
            return new HtmlString(lines.ToDelimitedString(prefix+"<br/>",htmlEncode:true));
        }

        public static IHtmlContent Linebreak(this IHtmlHelper htmlHelper, string content, string originalDelimiter="\r\n", string prefix=null)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            if (string.IsNullOrEmpty(originalDelimiter)) throw new ArgumentNullException(nameof(originalDelimiter));

            var lines = content.SplitI(originalDelimiter);
            return htmlHelper.Linebreak(lines,prefix);
        }

        public static IHtmlContent ReplaceFor<TModel,TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, string oldValue, string newValue)
        {
            _modelExpressionProvider = _modelExpressionProvider ?? htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelExpressionProvider>();
            var modelExpression = _modelExpressionProvider.CreateModelExpression(htmlHelper.ViewData, expression);
            if (modelExpression.Model is string modelAsString)return htmlHelper.Replace(modelAsString, oldValue, newValue);
            throw new ArgumentException("Expression does not return a string");
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