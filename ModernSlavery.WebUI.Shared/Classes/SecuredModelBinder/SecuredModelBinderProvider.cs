using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;
using System.Linq;
using System.Reflection;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
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

            var securedAttribute = context.Metadata.ModelType.GetCustomAttributes(true).FirstOrDefault(attr => typeof(SecuredAttribute).IsAssignableFrom(attr.GetType())) as SecuredAttribute;
            if (securedAttribute == null) return null;

            if (securedAttribute.SecureMethod == SecuredAttribute.SecureMethods.Obfuscate)
                return new BinderTypeModelBinder(typeof(ObfuscatedModelBinder));

            return new BinderTypeModelBinder(typeof(EncryptedModelBinder));
        }
    }
}
