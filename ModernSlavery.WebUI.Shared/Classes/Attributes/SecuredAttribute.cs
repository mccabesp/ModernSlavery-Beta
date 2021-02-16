using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class SecuredAttribute : TextAttribute
    {
        public readonly SecureMethods SecureMethod;
        public SecuredAttribute(SecureMethods secureMethod = SecureMethods.Encrypt) : base($"{Text.AlphaNumericChars}{(secureMethod == SecureMethods.Encrypt ? "+/=" : "")}")
        {
            SecureMethod = secureMethod;
        }
        private static IObfuscator _obfuscator;

        public enum SecureMethods
        {
            Encrypt = 1,
            Obfuscate = 2
        }

        public void Unsecure(ModelBindingContext bindingContext, IModelMetadataProvider metadataProvider, string rawValue)
        {
            var modelValidationContext = new ModelValidationContext(bindingContext.ActionContext, bindingContext.ModelMetadata, metadataProvider, null, rawValue);
            var validationResults = Validate(modelValidationContext);

            //Return 400 Bad Request;
            var errorResult = validationResults.FirstOrDefault();
            if (errorResult != null) throw new HttpException(System.Net.HttpStatusCode.BadRequest, errorResult.Message, rawValue);

            object value = rawValue;
            if (!string.IsNullOrWhiteSpace(rawValue))
            {
                var attemptedValue = rawValue;
                if (SecureMethod == SecureMethods.Obfuscate)
                    if (bindingContext.ModelMetadata.ModelType == typeof(string))
                        value = rawValue;
                    else
                    {
                        value = Deobfuscate(bindingContext, rawValue);
                        attemptedValue = value.ToString();
                        if (bindingContext.ModelMetadata.ModelType == typeof(string))
                            value = attemptedValue;
                        else
                            value = Convert.ChangeType(value, bindingContext.ModelMetadata.ModelType);
                    }
                else
                {
                    attemptedValue = Encryption.Decrypt(rawValue);
                    value = !bindingContext.ModelMetadata.ModelType.IsSimpleType() ? JsonConvert.DeserializeObject(attemptedValue, bindingContext.ModelMetadata.ModelType) : attemptedValue;
                }
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, rawValue, attemptedValue);
            }
            bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
            bindingContext.Result = ModelBindingResult.Success(value);
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.ActionContext.ModelState.ContainsKey(context.ModelMetadata.GetModelFullName())) return Enumerable.Empty<ModelValidationResult>();
            return base.Validate(context);
        }

        private object Deobfuscate(ModelBindingContext bindingContext, string originalValue)
        {
            _obfuscator = _obfuscator ?? bindingContext.HttpContext.RequestServices.GetRequiredService<IObfuscator>();
            var newValue = _obfuscator.DeObfuscate(originalValue);
            return newValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ObfuscatedAttribute : SecuredAttribute
    {
        public ObfuscatedAttribute() : base(SecureMethods.Obfuscate)
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class EncryptedAttribute : SecuredAttribute
    {
        public EncryptedAttribute() : base(SecureMethods.Encrypt)
        {

        }
    }
}
