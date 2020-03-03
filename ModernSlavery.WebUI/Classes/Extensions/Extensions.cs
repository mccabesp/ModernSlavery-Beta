using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ModernSlavery.WebUI.Classes
{
    public static partial class Extensions
    {

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

        #region Helpers


        #endregion

    }
}
