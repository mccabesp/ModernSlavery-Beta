using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Shared.Classes.Middleware
{
    /// <summary>
    /// This filter checks for Xss patterns in modelstate after model binding and before execution of a controller action.
    /// If the modelstate has a XssValidationAttribute then it is presumed to have been processed correctly
    /// Otherwise 
    ///  in Developer environment the code throw an error there there are no TextValidationAttributes or no CustomErrorMessages for those 
    /// if the model contains an xss violation then 
    /// A CustomErrorMessage is looked up using the modelName and any existing ValidationAttributes
    /// If no ValidationAttributes are prestmn on the moden an error is thrown to prompt developers to add an XssValidationAttribute or ValidationAttributes
    /// The modelstate is invalidated using the custom error message 
    /// </summary>
    public class XssValidationFilter : IActionFilter
    {
        private readonly ILogger<XssValidationFilter> _logger;
        private readonly XssValidator _xssValidator;
        private readonly TestOptions _testOptions;
        public XssValidationFilter(ILogger<XssValidationFilter> logger, XssValidator xssValidator, TestOptions testOptions)
        {
            _logger = logger;
            _xssValidator = xssValidator;
            _testOptions = testOptions;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var exceptions = new List<Exception>();
            (object model, Type modelType, string modelName, IEnumerable<Type> attributeTypes, bool actionArgument) modelInfo = default;
            var stringType = typeof(string);
            var isApi = context.Controller.GetAttribute<ApiControllerAttribute>() != null;
            foreach (var entry in context.ModelState)
            {
                object customErrorResult = null;

                if (_testOptions.IsDevelopment())
                {
                    modelInfo = GetModelInfo(context, entry.Key);
                    if (modelInfo == default || modelInfo.modelType == null) throw new ArgumentException($"Cannot find model {entry.Key} on action {context.ActionDescriptor.DisplayName}");
                    if (modelInfo.modelType != stringType && !modelInfo.model.IsEnumerable<string>()) continue;

                    customErrorResult = GetCustomError(context, modelInfo.attributeTypes, modelInfo.modelName, modelInfo.actionArgument, true);
                    if (customErrorResult is Exception exception)
                    {
                        exceptions.Add(exception);
                        continue;
                    }
                    if (customErrorResult is BadRequestResult badRequestResult)
                    {
                        if (isApi)
                            context.Result = badRequestResult;
                        else
                            context.Result = new HttpBadRequestResult();
                        return;
                    }

                    //Dont bother revalidating xss
                    if (modelInfo.attributeTypes.Any(a => typeof(XssValidationAttribute).IsAssignableFrom(a))) continue;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(entry.Value.AttemptedValue)) continue;

                    //if an action argument failed validation the return BadRequest
                    if (entry.Value.ValidationState == ModelValidationState.Invalid)
                    {
                        if (context.ActionArguments.ContainsKey(entry.Key))
                        {
                            if (isApi)
                                context.Result = new BadRequestResult();
                            else
                                context.Result = new HttpBadRequestResult();

                            return;
                        }
                    }
                    if (entry.Value.ValidationState != ModelValidationState.Valid) continue;

                    modelInfo = GetModelInfo(context, entry.Key);
                    if (modelInfo == default || modelInfo.modelType == null) throw new ArgumentException($"Cannot find model {entry.Key} on action {context.ActionDescriptor.DisplayName}");
                    if (modelInfo.modelType != stringType) continue;
                }

                var valueAsString = modelInfo.model as string;
                var valueAsEnumerable = modelInfo.model as IEnumerable<string>;
                if (string.IsNullOrWhiteSpace(valueAsString) && (valueAsEnumerable == null || !valueAsEnumerable.Any())) continue;
                var result = valueAsString != null ? _xssValidator.Validate(valueAsString) : _xssValidator.Validate(valueAsEnumerable).FirstOrDefault(result => result != default);
                if (result == default) continue;

                //Log the Xss violation
                _xssValidator.LogViolation(context.HttpContext, modelInfo.modelName, result.badChars, result.position, entry.Value.AttemptedValue);

                if (!_testOptions.IsDevelopment())
                {
                    customErrorResult = GetCustomError(context, modelInfo.attributeTypes, modelInfo.modelName, modelInfo.actionArgument, true);
                    if (customErrorResult is Exception exception) throw exception;
                    if (customErrorResult is IActionResult actionResult)
                    {
                        if (isApi)
                            context.Result = new BadRequestResult();
                        else
                            context.Result = new HttpBadRequestResult();
                        return;
                    }
                }

                //Mark the record as empty
                var customError = customErrorResult as CustomErrorMessage;
                if (modelInfo.actionArgument || customError == null || string.IsNullOrWhiteSpace(customError.Title))
                {
                    if (isApi)
                        context.Result = new BadRequestResult();
                    else
                        context.Result = new HttpBadRequestResult();
                    return;
                }
                context.ModelState.AddModelError(1005, entry.Key, new { result.badChars }, title: customError.Title);
            }
            if (exceptions.Any())
            {
#if DEBUG || DEBUGLOCAL
                if (_testOptions.IsDevelopment())
                {
                    exceptions.ForEach(ex => Debug.WriteLine(ex.Message));
                }
#endif
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }

        private (object model, Type modelType, string modelName, IEnumerable<Type> attributeTypes, bool actionArgument) GetModelInfo(ActionExecutingContext context, string bindingName)
        {
            var propertyName = GetPropertyName(context.ActionDescriptor, bindingName.BeforeFirst("["));
            if (context.ActionArguments.ContainsKey(propertyName))
            {
                var model = context.ActionArguments[propertyName];

                var par = context.ActionDescriptor.Parameters.FirstOrDefault(p => propertyName.EqualsI(p.Name)) as ControllerParameterDescriptor;
                var attrs = par.ParameterInfo.CustomAttributes.Select(ca => ca.AttributeType).ToArray();
                var modelType = par.ParameterType;
                return (model, modelType, propertyName, attrs, true);
            }

            IModelMetadataProvider metadataProvider=null;

            if (context.Controller is BaseController baseController)
                metadataProvider = baseController.MetadataProvider;
            else if (context.Controller is Controller controller)
                metadataProvider = controller.MetadataProvider;

            foreach (var argument in context.ActionArguments)
            {
                var model = argument.Value;
                var modelType = argument.Value.GetType();
                var containerName = modelType.Name;
                var propertyInfo = modelType.GetProperty(propertyName);

                if (propertyInfo == null)
                {
                    var propertiesMetadata = metadataProvider.GetMetadataForProperties(modelType);
                    var propertyMetadata = propertiesMetadata.FirstOrDefault(metaData => propertyName.EqualsI(metaData.Name, metaData.BinderModelName));
                    if (propertyMetadata == null) continue;
                    propertyName = propertyMetadata.PropertyName;
                    propertyInfo = modelType.GetProperty(propertyName);
                }

                model = propertyInfo.GetValue(model);
                modelType = propertyInfo.PropertyType;

                var attrs = propertyInfo?.GetCustomAttributes(true).Select(ca => ca.GetType());
                return (model, modelType, $"{containerName}.{propertyName}", attrs, false);
            }
            return default;
        }

        private string GetPropertyName(ActionDescriptor actionDescriptor, string bindingName)
        {
            var par = actionDescriptor.Parameters.FirstOrDefault(p => bindingName.EqualsI(p.Name, p.BindingInfo?.BinderModelName));
            return par == null ? bindingName : par.Name;
        }

        private object GetCustomError(ActionExecutingContext context, IEnumerable<Type> attributeTypes, string modelName, bool actionArgument, bool useAny)
        {
            //If model doent have any ValidationAttributes then return 404 Bad Request
            var validationAttributeTypes = attributeTypes?.GetTextValidationAttributeTypes();
            if (!validationAttributeTypes.Any())
                return new ValidationException($"Cannot find ValidationAttributes for model: {modelName}");

            if (attributeTypes.Any(a => a == typeof(IgnoreTextAttribute) || typeof(SecuredAttribute).IsAssignableFrom(a))) return null;

            //If model doent have a custom error message for any of the ValidationAttributes then return 404 Bad Request
            var customError = GetCustomErrorMessage(validationAttributeTypes, modelName, useAny);
            if (customError == null)
            {
                if (actionArgument)
                {
                    if (_testOptions.IsDevelopment())
                        return null;//In development mode fall through th Xss validation
                    else
                        return new BadRequestResult(); //Otherwise throw a bad request
                }
                return new ValidationException($"Cannot find {nameof(CustomErrorMessage)} for model: {modelName}");
            }

            return customError;
        }

        private CustomErrorMessage GetCustomErrorMessage(IEnumerable<Type> validationAttributeTypes, string modelName, bool useAny)
        {
            foreach (var validationAttributeType in validationAttributeTypes)
            {
                var customError = CustomErrorMessages.GetValidationError($"{modelName}:{validationAttributeType.Name.TrimSuffix("Attribute")}");
                if (customError != null) return customError;
            }

            return useAny ? CustomErrorMessages.GetValidationError($"{modelName}:*") : null;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //Dont redirect for error controller
            if (context.Controller is ErrorController) return;

            //Dont redirect for api responses
            var isApi = context.Controller.GetAttribute<ApiControllerAttribute>() != null;
            if (isApi) return;

            //Not required
            int errorCode = -1;
            if (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode>=400)
            {
                errorCode = statusCodeResult.StatusCode;
                _logger.LogError($"Http {Enums.GetEnumOrValueDescription<HttpStatusCode,int>(errorCode)} Error");
            }
            else if (context.Result is HttpStatusViewResult httpStatusViewResult && httpStatusViewResult.StatusCode >= 400)
            {
                errorCode = httpStatusViewResult.StatusCode == null ? 400 : httpStatusViewResult.StatusCode.Value;
                _logger.LogError($"Http {Enums.GetEnumOrValueDescription<HttpStatusCode, int>(errorCode)} Error: {httpStatusViewResult.StatusDescription}");
            }
            else if (context.Result is HttpStatusCodeResult httpStatusCodeResult && httpStatusCodeResult.StatusCode >= 400)
            {
                errorCode = httpStatusCodeResult.StatusCode == null ? 400 : httpStatusCodeResult.StatusCode.Value;
                _logger.LogError($"Http {Enums.GetEnumOrValueDescription<HttpStatusCode, int>(errorCode)} Error: {httpStatusCodeResult.StatusDescription}");
            }

            if (errorCode>-1)context.Result = context.RouteData.GetRedirectToErrorPageResult(errorCode);

        }
    }
}
