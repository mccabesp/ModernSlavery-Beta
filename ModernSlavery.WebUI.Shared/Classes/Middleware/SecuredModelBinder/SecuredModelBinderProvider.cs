using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware.SecureModelBinder
{
    public class SecuredModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.IsComplexType) return null;

            var propName = context.Metadata.PropertyName;
            if (propName == null) return null;

            var propInfo = context.Metadata.ContainerType.GetProperty(propName);
            if (propInfo == null) return null;

            var secureAttribute = context.Metadata.ModelType.GetCustomAttributes().FirstOrDefault(attr=> typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;
            if (secureAttribute == null) return null;

            if (secureAttribute.SecureMethod == SecuredAttribute.SecureMethods.Obfuscate)
                return new BinderTypeModelBinder(typeof(ObfuscatedModelBinder));

            return new BinderTypeModelBinder(typeof(EncryptedModelBinder));
        }
    }
}
