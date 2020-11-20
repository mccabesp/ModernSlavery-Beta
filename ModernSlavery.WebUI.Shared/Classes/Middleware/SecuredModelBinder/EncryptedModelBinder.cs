using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware.SecureModelBinder
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
                value = Encryption.Decrypt(value);
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, new ValueProviderResult(value, valueProviderResult.Culture)); 
                bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
            }

            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }
}
