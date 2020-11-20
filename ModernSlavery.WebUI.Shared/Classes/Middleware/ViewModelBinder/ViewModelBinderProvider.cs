using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ModernSlavery.WebUI.Shared.Classes.Middleware.ViewModelBinder;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware.ClassModelBinder
{
    public class ViewModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.Metadata.IsComplexType) return null;

            var viewModelAttribute=context.Metadata.ModelType.GetCustomAttribute<ViewModelAttribute>();
            if (viewModelAttribute == null) return null;

            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            for (var i = 0; i < context.Metadata.Properties.Count; i++)
            {
                var property = context.Metadata.Properties[i];
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            var loggerFactory = context.Services.GetService<ILoggerFactory>();
            return new ViewModelBinder(viewModelAttribute, propertyBinders, loggerFactory);
        }
    }
}
