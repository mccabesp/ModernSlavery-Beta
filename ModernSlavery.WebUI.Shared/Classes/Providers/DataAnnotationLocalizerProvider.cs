using System;
using System.Reflection;
using Microsoft.Extensions.Localization;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Shared.Classes.Providers
{

    public static class DataAnnotationLocalizerProvider
    {

        public static IStringLocalizer DefaultResourceHandler(Type type, IStringLocalizerFactory factory)
        {
            var defaultResource = type.GetCustomAttribute<DefaultResourceAttribute>();
            if (defaultResource == null)
            {
                return default;
            }

            return factory.Create(defaultResource.ResourceType);
        }

    }

}
