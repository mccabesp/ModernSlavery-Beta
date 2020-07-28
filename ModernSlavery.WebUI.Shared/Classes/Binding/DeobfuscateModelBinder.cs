using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Interfaces;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    public class DeobfuscateModelBinder : IModelBinder
    {
        private readonly IObfuscator _obfuscator;

        public DeobfuscateModelBinder(IObfuscator obfuscator)
        {
            _obfuscator = obfuscator;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult =
                bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(
                    bindingContext.ModelName, valueProviderResult);

            long result = _obfuscator.DeObfuscate(valueProviderResult.FirstValue);

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
}
