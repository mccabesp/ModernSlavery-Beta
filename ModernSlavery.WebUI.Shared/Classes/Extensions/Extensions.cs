using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static IUrlRouteHelper RouteHelper;

        public static void AddStringTrimmingProvider(this MvcOptions option)
        {
            var binderToFind =
                option.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (binderToFind == null) return;

            var index = option.ModelBinderProviders.IndexOf(binderToFind);
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
            if (string.IsNullOrWhiteSpace(enc)) return false;

            var id = Encryption.DecryptQuerystring(enc).ToInt64();
            if (id <= 0) return false;

            decId = id;
            return true;
        }

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
    }
}