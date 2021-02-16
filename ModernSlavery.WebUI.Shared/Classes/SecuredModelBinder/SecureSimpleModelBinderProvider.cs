using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Linq;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    public class SecureSimpleModelBinderProvider : IModelBinderProvider
    {
        private static IModelBinderProvider _fallbackModelProvider;

        public SecureSimpleModelBinderProvider(IModelBinderProvider fallbackModelProvider)
        {
            _fallbackModelProvider = fallbackModelProvider ?? throw new ArgumentNullException(nameof(fallbackModelProvider));
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Metadata.IsComplexType) return null;

            var modelMetadata = context.Metadata as DefaultModelMetadata;
            var securedAttribute = modelMetadata?.Attributes.Attributes?.OfType<SecuredAttribute>().FirstOrDefault();
            if (securedAttribute != null)return new SecureSimpleModelBinder(_fallbackModelProvider.GetBinder(context), context.MetadataProvider);
            return null;
        }
    }
}
