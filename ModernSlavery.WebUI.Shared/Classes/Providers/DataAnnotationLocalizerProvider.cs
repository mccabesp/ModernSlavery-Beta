using System;
using System.Reflection;
using Microsoft.Extensions.Localization;

namespace ModernSlavery.WebUI.Shared.Classes
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
