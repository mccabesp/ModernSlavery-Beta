﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.SecuredModelBinder;
using ModernSlavery.WebUI.Shared.Classes.ViewModelBinder;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static void AddStringTrimmingProvider(this MvcOptions option)
        {
            var index = option.ModelBinderProviders.ToList().FindIndex(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (index < 0) index = 0;
            option.ModelBinderProviders.Insert(index, new TrimmingModelBinderProvider());
        }

        public static void AddViewModelBinderProvider(this MvcOptions option)
        {
            var index = option.ModelBinderProviders.ToList().FindIndex(x => x.GetType() == typeof(ComplexTypeModelBinderProvider));
            if (index < 0) index = 0;
            option.ModelBinderProviders.Insert(index, new ViewModelBinderProvider());
        }

        public static void AddSecureSimpleModelBinderProvider(this MvcOptions option)
        {
            var index = option.ModelBinderProviders.ToList().FindIndex(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (index < 0) throw new ArgumentException($"Cannot find {nameof(SimpleTypeModelBinderProvider)}");
            option.ModelBinderProviders.Insert(index, new SecureSimpleModelBinderProvider(option.ModelBinderProviders[index]));
        }

        #region AntiSpam

        public static IHtmlContent BotProtectionTimeStamp(this IHtmlHelper helper)
        {
            var builder = new TagBuilder("input");

            builder.MergeAttribute("id", "BotProtectionTimeStamp");
            builder.MergeAttribute("name", "BotProtectionTimeStamp");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("value", Encryption.Encrypt(VirtualDateTime.Now.ToSmallDateTime(), Encryption.Encodings.Base62));
            return builder.RenderSelfClosingTag();
        }

        #endregion

        #region User Entity

        #endregion

        #region Encypt Decrypt

        public static bool DecryptToParams(this string enc, out List<string> outParams)
        {
            outParams = null;
            if (string.IsNullOrWhiteSpace(enc)) return false;

            var decParams = Encryption.DecryptData(enc.DecodeUrlBase64());
            if (string.IsNullOrWhiteSpace(decParams)) return false;

            outParams = new List<string>(decParams.Split(':'));
            return true;
        }

        #endregion

        #region Navigation

        public static string GetControllerFriendlyName<TController>() where TController : Controller
        {
            return GetControllerFriendlyName(typeof(TController).Name);
        }

        private static string GetControllerFriendlyName(string controllerName)
        {
            return controllerName.Remove(controllerName.LastIndexOf(nameof(Controller)));
        }

        public static AreaAttribute GetControllerArea<TController>() where TController : Controller
        {
            return typeof(TController)
                .GetCustomAttributes(typeof(AreaAttribute), false)
                .FirstOrDefault() as AreaAttribute;
        }


        #endregion

        public static IActionResult GetRedirectToErrorPageResult(this RouteData routeData, int errorCode)
        {
            if (!routeData.IsAction("Default", "Error"))
                return new RedirectToActionResult("Default", "Error", new { errorCode });
            else
                return new StatusCodeResult(errorCode);
        }
    }
}