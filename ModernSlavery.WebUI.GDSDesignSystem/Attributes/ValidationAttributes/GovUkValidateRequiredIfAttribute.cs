using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Shared.Classes.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GovUkValidateRequiredIfAttribute : ValidationAttribute
    {
        private string PropertyName { get; set; }
        private string ErrorMessage { get; set; }
        private object DesiredValue { get; set; }

        public GovUkValidateRequiredIfAttribute(string propertyName, object desiredvalue, string errormessage)
        {
            PropertyName = propertyName;
            DesiredValue = desiredvalue;
            ErrorMessage = errormessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value != null) { return ValidationResult.Success; }

            var instance = context.ObjectInstance;
            var type = instance.GetType();
            var propertyvalue = type.GetProperty(PropertyName).GetValue(instance, null);
            if (propertyvalue != DesiredValue)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}
