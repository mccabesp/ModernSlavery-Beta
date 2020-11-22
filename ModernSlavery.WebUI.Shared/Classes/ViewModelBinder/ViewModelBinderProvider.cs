using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ModernSlavery.WebUI.Shared.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace ModernSlavery.WebUI.Shared.Classes.ViewModelBinder
{
    public class ViewModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.Metadata.IsComplexType) return null;

            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            for (var i = 0; i < context.Metadata.Properties.Count; i++)
            {
                var property = context.Metadata.Properties[i];
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            var loggerFactory = context.Services.GetService<ILoggerFactory>();
            if (typeof(BaseViewModel).IsAssignableFrom(context.Metadata.ModelType)) return new ViewModelBinder(propertyBinders, loggerFactory);
            return new ComplexTypeModelBinder(propertyBinders, loggerFactory);            
        }
    }
}
