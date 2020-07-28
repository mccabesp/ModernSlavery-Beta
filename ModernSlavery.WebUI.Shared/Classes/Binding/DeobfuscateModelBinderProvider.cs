using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.Binding
{
    public class DeobfuscateModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.IsComplexType) return null;

            var propName = context.Metadata.PropertyName;
            if (propName == null) return null;

            var propInfo = context.Metadata.ContainerType.GetProperty(propName);
            if (propInfo == null) return null;

            if (!propInfo.GetCustomAttributes(typeof(IObfuscatedAttribute), false).Any())return null;

            return new BinderTypeModelBinder(typeof(DeobfuscateModelBinder));
        }
    }
}
