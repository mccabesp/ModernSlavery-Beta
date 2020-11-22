using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Extensions;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder
{
    public class EncryptedModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
            
            var value = valueProviderResult.FirstValue;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var decryptedValue = Encryption.Decrypt(value);
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value, decryptedValue);
                bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
            }

            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }
}
