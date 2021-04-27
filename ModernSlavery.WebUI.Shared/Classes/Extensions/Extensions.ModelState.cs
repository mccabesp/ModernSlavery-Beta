using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static void SetModelCustomErrors<TModel>(this Controller controller, TModel model = default)
        {
            var modelType = typeof(TModel);
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
                    : modelType.GetPropertyInfo(propertyName);
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
                    displayAttribute != null ? displayAttribute.Name : propertyName.AfterLast(".");

                foreach (var error in modelState.Value.Errors)
                {
                    var title = string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    var description = !string.IsNullOrWhiteSpace(propertyName) ? error.ErrorMessage : null;
                    if (error.ErrorMessage.Like("The value * is not valid for *."))
                    {
                        title = "There's a problem with your values.";
                        description = "The value here is invalid.";
                    }

                    if (!string.IsNullOrWhiteSpace(propertyName))
                    {
                        var metaData = controller.MetadataProvider.GetMetadataForProperty(modelType, propertyName);

                        var modelValidationAttribute = metaData?.ValidatorMetadata.OfType<XssValidationAttribute>().SingleOrDefault(v => v.ErrorMessage == error.ErrorMessage);
                        string validatorKey;
                        CustomErrorMessage customError;
                        if (modelValidationAttribute != null)
                        {
                            validatorKey = $"{modelType.Name}.{propertyName}:{modelValidationAttribute.GetType().Name.TrimSuffix("Attribute")}";
                            customError = CustomErrorMessages.GetValidationError(validatorKey);
                            if (customError == null)
                            {
                                validatorKey = $"{modelType.Name}.{propertyName}:*";
                                customError = CustomErrorMessages.GetValidationError(validatorKey);
                                if (customError != null)
                                {
                                    title = modelValidationAttribute.FormatError(customError.Title, displayName);
                                }
                            }
                            else
                            {
                                title = modelValidationAttribute.FormatError(customError.Title, displayName);
                                description = modelValidationAttribute.FormatError(string.IsNullOrWhiteSpace(customError.Description) ? error.ErrorMessage : customError.Description, displayName);
                            }
                        }
                        else
                        {
                            var attribute = metaData?.ValidatorMetadata.OfType<ValidationAttribute>().SingleOrDefault(v => attributes.Any(a => a.GetType() == v.GetType() && v.ErrorMessage == error.ErrorMessage));
                            if (attribute == null) attribute = attributes.FirstOrDefault(a => a.FormatErrorMessage(displayName) == error.ErrorMessage);
                            if (attribute != null)
                            {
                                validatorKey = $"{modelType.Name}.{propertyName}:{attribute.GetType().Name.TrimSuffix("Attribute")}";
                                customError = CustomErrorMessages.GetValidationError(validatorKey);
                                if (customError != null)
                                {
                                    title = attribute.FormatError(customError.Title, displayName);
                                    description = attribute.FormatError(string.IsNullOrWhiteSpace(customError.Description) ? error.ErrorMessage : customError.Description, displayName);
                                }
                            }
                        }
                    }

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
            CustomError error,
            string propertyName = null,
            object parameters = null)
        {
            AddModelError(modelState, error.Code, propertyName, parameters);
        }

        public static void AddModelError(this ModelStateDictionary modelState,
            int errorCode,
            string propertyName = null,
            object parameters = null, string title=null)
        {
            //Try and get the custom error
            var customError = CustomErrorMessages.GetPageError(errorCode);
            if (customError == null)
                throw new ArgumentException("errorCode", "Cannot find custom error message with this code");

            //Add the error to the modelstate
            if (string.IsNullOrWhiteSpace(title)) title = customError.Title;
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

        public static void AddValidationError(this List<ValidationResult> validationResults, int errorCode,
            string propertyName = null,
            object parameters = null)
        {
            //Try and get the custom error
            var customError = CustomErrorMessages.GetPageError(errorCode);
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
            bool titleExists() => !string.IsNullOrWhiteSpace(title) && validationResults.Any(r => !r.MemberNames.Any() && r.ErrorMessage == title);

            if (!titleExists()) validationResults.Add(new ValidationResult(title));

            if (!string.IsNullOrWhiteSpace(description))
            {
                //If no property then add description as second line of summary
                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    if (!!titleExists()) validationResults.Add(new ValidationResult(description));
                }

                //add the inline message if it doesnt already exist
                else if (!validationResults.Any(r => r.MemberNames.Any(m => m.EqualsI(propertyName)) && r.ErrorMessage == description))
                {
                    validationResults.Add(new ValidationResult(description, new[] { propertyName }.AsEnumerable()));
                }
            }
        }

        public static void SetMultiException(this ModelStateDictionary modelState, Exception ex, string title)
        {
            if (ex is AggregateException aex)
            {
                if (aex.InnerExceptions.Count > 1)
                {
                    modelState.AddModelError("", $"{title}:");
                    foreach (var iex in aex.InnerExceptions)
                        modelState.AddModelError("", $"\t{iex.Message}");

                    return;
                }
                else
                    ex = aex.InnerExceptions.First();
            }

            if (ex is AggregateException aex1)
                modelState.SetMultiException(aex1, title);
            else
                modelState.AddModelError("", $"{title}: {ex.Message}");
        }

    }
}