using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    public class ViewModelBinder : ComplexTypeModelBinder
    {
        public ViewModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory) : base(propertyBinders, loggerFactory)
        {

        }

        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            var sessionViewStateAttribute = bindingContext.ModelMetadata.ModelType.GetCustomAttribute<SessionViewStateAttribute>();
            if (sessionViewStateAttribute != null) return UnstashModel(bindingContext);
            
            return ActivatorUtilities.GetServiceOrCreateInstance(bindingContext.HttpContext.RequestServices, bindingContext.ModelType);
        }

        private object UnstashModel(ModelBindingContext bindingContext, bool delete = false)
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

        protected override Task BindProperty(ModelBindingContext bindingContext)
        {
            var propName = bindingContext.ModelMetadata.PropertyName;
            if (propName == null) return base.BindProperty(bindingContext);

            var propInfo = bindingContext.ModelMetadata.ContainerType.GetProperty(propName);
            if (propInfo == null) return base.BindProperty(bindingContext);

            var secureAttribute = propInfo.GetCustomAttributes().FirstOrDefault(attr => typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;
            if (secureAttribute == null) return base.BindProperty(bindingContext);

            IModelBinder modelBinder;
            if (secureAttribute.SecureMethod == SecuredAttribute.SecureMethods.Obfuscate)
                modelBinder = ActivatorUtilities.CreateInstance<ObfuscatedModelBinder>(bindingContext.HttpContext.RequestServices);
            else
                modelBinder = ActivatorUtilities.CreateInstance<EncryptedModelBinder>(bindingContext.HttpContext.RequestServices);

            return modelBinder.BindModelAsync(bindingContext);
        }
    }
}
