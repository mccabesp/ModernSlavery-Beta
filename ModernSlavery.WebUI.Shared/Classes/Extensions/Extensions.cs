using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.Core.Extensions;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using IModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinderProvider;
using TagBuilder = Microsoft.AspNetCore.Mvc.Rendering.TagBuilder;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static IUrlRouteHelper RouteHelper;

        public static void AddStringTrimmingProvider(this MvcOptions option)
        {
            IModelBinderProvider binderToFind =
                option.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (binderToFind == null)
            {
                return;
            }

            int index = option.ModelBinderProviders.IndexOf(binderToFind);
            option.ModelBinderProviders.Insert(index, new TrimmingModelBinderProvider());
        }

        #region AntiSpam

        public static IHtmlContent SpamProtectionTimeStamp(this IHtmlHelper helper)
        {
            var builder = new TagBuilder("input");

            builder.MergeAttribute("id", "SpamProtectionTimeStamp");
            builder.MergeAttribute("name", "SpamProtectionTimeStamp");
            builder.MergeAttribute("type", "hidden");
            builder.MergeAttribute("value", Encryption.EncryptData(VirtualDateTime.Now.ToSmallDateTime()));
            return builder.RenderSelfClosingTag();
        }

        #endregion

        #region User Entity

    
        #endregion

        #region Encypt Decrypt

        public static bool DecryptToId(this string enc, out long decId)
        {
            decId = 0;
            if (string.IsNullOrWhiteSpace(enc))
            {
                return false;
            }

            long id = Encryption.DecryptQuerystring(enc).ToInt64();
            if (id <= 0)
            {
                return false;
            }

            decId = id;
            return true;
        }

        public static bool DecryptToParams(this string enc, out List<string> outParams)
        {
            outParams = null;
            if (string.IsNullOrWhiteSpace(enc))
            {
                return false;
            }

            string decParams = Encryption.DecryptData(enc.DecodeUrlBase64());
            if (string.IsNullOrWhiteSpace(decParams))
            {
                return false;
            }

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

    }
}
