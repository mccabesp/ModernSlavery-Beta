using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Extensions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    public class EncryptedModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
            
            var originalValue = valueProviderResult.FirstValue;
            if (!string.IsNullOrWhiteSpace(originalValue))
            {
                var decryptedValue = Encryption.Decrypt(originalValue);
                var newValue = bindingContext.ModelType.IsSimpleType() ? originalValue : JsonConvert.DeserializeObject(decryptedValue, bindingContext.ModelType);
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, newValue, originalValue);
                bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
                bindingContext.Result = ModelBindingResult.Success(newValue);
            }

            return Task.CompletedTask;
        }
    }
}
