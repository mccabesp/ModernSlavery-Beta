using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited =true)]
    public abstract class XssValidationAttribute:Attribute, IModelValidator
    {
        protected readonly string ErrorMessageString;
        protected readonly string XssMessageString;
        public XssValidationAttribute(string errorMessageString, string xssMessageString=null):base()
        {
            ErrorMessageString = errorMessageString;
            XssMessageString = string.IsNullOrWhiteSpace(xssMessageString) ? errorMessageString : xssMessageString;
        }
        private static XssValidator _xssValidator;

        public string ErrorMessage { get; protected set; }

        public abstract IEnumerable<ModelValidationResult> Validate(ModelValidationContext context);

        protected bool XssValidate(ModelValidationContext context, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return default;
            _xssValidator ??= context.ActionContext.HttpContext.RequestServices.GetRequiredService<XssValidator>();
            var result = _xssValidator.Validate(value);
            if (result != default)
            {
                ErrorMessage = ResolveXssMessage(result.badChars);
                _xssValidator.LogViolation(context.ActionContext.HttpContext, context.ModelMetadata.GetModelFullName(), result.badChars, result.position, value);
                return false;
            }
            return true;
        }

        protected string ResolveXssMessage(string badChars)
        {
            var obj = new { badChars };
            return obj.Resolve(XssMessageString);
        }

        public virtual string FormatErrorMessage(string name)
        {
            return FormatError(ErrorMessageString, name);
        }
        public virtual string FormatError(string error,string name)
        {
            return string.Format(error, name);
        }
    }
}
