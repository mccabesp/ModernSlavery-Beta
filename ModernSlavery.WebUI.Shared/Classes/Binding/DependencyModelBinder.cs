using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.Binding
{
    public class DependencyModelBinder : ComplexTypeModelBinder
    {

        public DependencyModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory): base(propertyBinders, loggerFactory)
        {

        }
        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            var model = bindingContext.HttpContext.RequestServices.GetService(bindingContext.ModelType);
            if (model == null) model = ActivatorUtilities.CreateInstance(bindingContext.HttpContext.RequestServices, bindingContext.ModelType);

            return model;
        }
    }
}
