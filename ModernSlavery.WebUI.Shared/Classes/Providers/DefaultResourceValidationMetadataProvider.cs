﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Classes.Providers
{
    public class DefaultResourceValidationMetadataProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var attribute in context.ValidationMetadata.ValidatorMetadata)
            {
                var validationAttribute = attribute as ValidationAttribute;
                if (validationAttribute == null) continue;

                // ignore custom error messages
                if (string.IsNullOrWhiteSpace(validationAttribute.ErrorMessage) == false) continue;

                // only continue if ErrorMessageResourceName is set
                if (string.IsNullOrWhiteSpace(validationAttribute.ErrorMessageResourceName)) continue;

                // only continue if DefaultResourceAttribute is set on the container class
                var defaultResource = context.Key.ContainerType.GetCustomAttribute<DefaultResourceAttribute>();
                if (defaultResource == null) continue;

                validationAttribute.ErrorMessageResourceType = defaultResource.ResourceType;
            }
        }
    }
}