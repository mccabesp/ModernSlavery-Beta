using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware.SecureModelBinder
{
    public class ObfuscatedModelBinder : IModelBinder
    {
        private readonly IObfuscator _obfuscator;

        public ObfuscatedModelBinder(IObfuscator obfuscator)
        {
            _obfuscator = obfuscator;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

            var value = valueProviderResult.FirstValue;
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = _obfuscator.DeObfuscate(value).ToString();
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, new ValueProviderResult(value, valueProviderResult.Culture));
                bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
            }

            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }
}
