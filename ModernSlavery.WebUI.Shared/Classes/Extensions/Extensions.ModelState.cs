using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static void CleanModelErrors<TModel>(this Controller controller)
        {
            var containerType = typeof(TModel);
            //Save the old modelstate
            var oldModelState = new ModelStateDictionary();
            foreach (var modelState in controller.ModelState)
            {
                var propertyName = modelState.Key;
                foreach (var error in modelState.Value.Errors)
                {
                    var exists = oldModelState.Any(
                        m => m.Key == propertyName && m.Value.Errors.Any(e => e.ErrorMessage == error.ErrorMessage));

                    //add the inline message if it doesnt already exist
                    if (!exists) oldModelState.AddModelError(propertyName, error.ErrorMessage);
                }
            }

            //Clear the model state ready for refill
            controller.ModelState.Clear();

            foreach (var modelState in oldModelState)
            {
                //Get the property name
                var propertyName = modelState.Key;

                //Get the validation attributes
                var propertyInfo = string.IsNullOrWhiteSpace(propertyName)
                    ? null
                    : containerType.GetPropertyInfo(propertyName);
                var attributes = propertyInfo == null
                    ? null
                    : propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), false)
                        .ToList<ValidationAttribute>();

                //Get the display name
                var displayNameAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as
                        DisplayNameAttribute;
                var displayAttribute =
                    propertyInfo?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as
                        DisplayAttribute;
                var displayName = displayNameAttribute != null ? displayNameAttribute.DisplayName :
                    displayAttribute != null ? displayAttribute.Name : propertyName;

                foreach (var error in modelState.Value.Errors)
                {
                    var title = string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    var description = !string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;

                    if (error.ErrorMessage.Like("The value * is not valid for *."))
                    {
                        title = "There's a problem with your values.";
                        description = "The value here is invalid.";
                    }

                    if (attributes == null || !attributes.Any()) goto addModelError;

                    var attribute =
                        attributes.FirstOrDefault(a => a.FormatErrorMessage(displayName) == error.ErrorMessage);
                    if (attribute == null) goto addModelError;

                    var validatorKey =
                        $"{containerType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                    var customError = CustomErrorMessages.GetValidationError(validatorKey);
                    if (customError == null) goto addModelError;

                    title = attribute.FormatError(customError.Title, displayName);
                    description = attribute.FormatError(customError.Description, displayName);

                    addModelError:

                    //add the summary message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(title)
                        && !controller.ModelState.Any(m =>
                            m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                        controller.ModelState.AddModelError("", title);

                    //add the inline message if it doesnt already exist
                    if (!string.IsNullOrWhiteSpace(description)
                        && !string.IsNullOrWhiteSpace(propertyName)
                        && !controller.ModelState.Any(
                            m => m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                        controller.ModelState.AddModelError(propertyName, description);
                }
            }
        }

        //Removes all but the specified properties from the model state
        public static void Include(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (var key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (properties.ContainsI(key)) continue;

                modelState.Remove(key);
            }
        }

        //Removes all the specified properties from the model state
        public static void Exclude(this ModelStateDictionary modelState, params string[] properties)
        {
            foreach (var key in modelState.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!properties.ContainsI(key)) continue;

                modelState.Remove(key);
            }
        }

        public static void AddModelError(this ModelStateDictionary modelState,
            int errorCode,
            string propertyName = null,
            object parameters = null)
        {
            //Try and get the custom error
            var customError = CustomErrorMessages.GetError(errorCode);
            if (customError == null)
                throw new ArgumentException("errorCode", "Cannot find custom error message with this code");

            //Add the error to the modelstate
            var title = customError.Title;
            var description = customError.Description;

            //Resolve the parameters
            if (parameters != null)
            {
                title = parameters.Resolve(title);
                description = parameters.Resolve(description);
            }

            //add the summary message if it doesnt already exist
            if (!string.IsNullOrWhiteSpace(title) &&
                !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                modelState.AddModelError("", title);

            if (!string.IsNullOrWhiteSpace(description))
            {
                //If no property then add description as second line of summary
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (!string.IsNullOrWhiteSpace(title)
                        && !modelState.Any(m => m.Key == "" && m.Value.Errors.Any(e => e.ErrorMessage == title)))
                        modelState.AddModelError("", title);
                }

                //add the inline message if it doesnt already exist
                else if (!modelState.Any(m =>
                    m.Key.EqualsI(propertyName) && m.Value.Errors.Any(e => e.ErrorMessage == description)))
                {
                    modelState.AddModelError(propertyName, description);
                }
            }
        }
    }
}