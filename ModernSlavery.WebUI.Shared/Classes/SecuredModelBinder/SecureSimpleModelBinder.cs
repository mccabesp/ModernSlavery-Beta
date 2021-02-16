using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    public class SecureSimpleModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;
        private readonly IModelMetadataProvider _metadataProvider;

        public SecureSimpleModelBinder(IModelBinder fallbackBinder, IModelMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
            _fallbackBinder = fallbackBinder ?? throw new ArgumentNullException(nameof(fallbackBinder));
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (valueProviderResult != ValueProviderResult.None && valueProviderResult.FirstValue is string valueAsString)
            {
                //Remove leading/trailing whitespace
                valueAsString = valueAsString?.Trim();

                if (!string.IsNullOrWhiteSpace(valueAsString))
                {
                    //Decrypt/Deserialize or Deobfuscate the string
                    var modelMetadata = bindingContext.ModelMetadata as DefaultModelMetadata;
                    var securedAttribute = modelMetadata?.Attributes.Attributes?.OfType<SecuredAttribute>().FirstOrDefault();
                    if (securedAttribute != null)
                    {
                        securedAttribute.Unsecure(bindingContext, _metadataProvider, valueAsString);
                        return;
                    }
                }
            }

            await _fallbackBinder.BindModelAsync(bindingContext);
        }
    }
}
