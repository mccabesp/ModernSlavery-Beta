using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HtmlHelper;
using ModernSlavery.WebUI.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    public class ViewModelBinder : ComplexTypeModelBinder
    {
        public const string ViewStateKey = "__ViewState";
        public ViewModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory) : base(propertyBinders, loggerFactory)
        {

        }

        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            var sessionViewStateAttribute = bindingContext.ModelMetadata.ModelType.GetCustomAttribute<SessionViewStateAttribute>();
            if (sessionViewStateAttribute != null) return GetObjectFromSession(bindingContext);

            var formViewStateAttribute = bindingContext.ModelMetadata.ModelType.GetCustomAttribute<FormViewStateAttribute>();
            if (formViewStateAttribute != null) return GetObjectFromForm(bindingContext);

            return ActivatorUtilities.GetServiceOrCreateInstance(bindingContext.HttpContext.RequestServices, bindingContext.ModelType);
        }

        private object GetObjectFromForm(ModelBindingContext bindingContext, bool delete = false)
        {
            var modelType = bindingContext.ModelType;
            
            if (!bindingContext.HttpContext.Request.Form.ContainsKey(ViewStateKey)) throw new Exception($"No viewstate form field '{ViewStateKey}' posted for view model '{modelType.Name}'. You must call @Html.{nameof(HtmlHelperExtensions.AddViewState)}() in posted form.");
            var viewState = bindingContext.HttpContext.Request.Form[ViewStateKey].ToString();
            if (string.IsNullOrWhiteSpace(viewState)) throw new Exception($"Missing viewstate content in form field '{ViewStateKey}' for view model '{modelType.Name}'.");

            var json = Encryption.Decrypt(viewState);
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject(json, modelType);
            return result;
        }

        private object GetObjectFromSession(ModelBindingContext bindingContext, bool delete = false)
        {
            var session = bindingContext.ActionContext.HttpContext.RequestServices.GetService<IHttpSession>();

            var controllerType = GetControllerType(bindingContext.ActionContext);
            var modelType = bindingContext.ModelType;

            var keyName = $"{controllerType}:{modelType}:Model";
            var json = session[keyName].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject(json, modelType);
            if (delete) session.Remove(keyName);
            return result;
        }

        private Type GetControllerType(ActionContext actionContext)
        {
            var controllerActionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
            var controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
            return controllerTypeInfo.AsType();
        }
    }
}
