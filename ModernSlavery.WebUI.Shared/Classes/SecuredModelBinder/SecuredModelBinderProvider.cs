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

            var viewStateAttribute = context.Metadata.ModelType.GetCustomAttributes().FirstOrDefault(attr => typeof(ViewStateAttribute).IsAssignableFrom(attr.GetType())) as ViewStateAttribute;
            if (viewStateAttribute == null || viewStateAttribute.SecureMethod == ViewStateAttribute.SecureMethods.None) return null;

            if (viewStateAttribute.SecureMethod == ViewStateAttribute.SecureMethods.Obfuscate)
                return new BinderTypeModelBinder(typeof(ObfuscatedModelBinder));

            return new BinderTypeModelBinder(typeof(EncryptedModelBinder));
        }
    }
}
