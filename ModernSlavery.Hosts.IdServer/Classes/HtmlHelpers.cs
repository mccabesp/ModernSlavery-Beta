using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.HtmlHelpers;

namespace ModernSlavery.IdServer.Classes
{
    public static class HtmlHelpers
    {

        public static HtmlString PageIdentifier(this IHtmlHelper htmlHelper)
        {
            var sharedOptions=htmlHelper.GetSharedOptions();
            return new HtmlString(
                $"Date:{VirtualDateTime.Now}, Version:{sharedOptions.Version}, File Date:{sharedOptions.AssemblyDate.ToLocalTime()}, Environment:{sharedOptions.Environment}, Machine:{Environment.MachineName}, Instance:{sharedOptions.WEBSITE_INSTANCE_ID}, {sharedOptions.AssemblyCopyright}");
        }

        public static string CurrentView(this IHtmlHelper html)
        {
            string path = ((RazorView) html.ViewContext.View).Path;
            path = path.RemoveStartI("~/Views/").TrimStartI(@"~/\").RemoveEndI(Path.GetExtension(path));
            return path;
        }

        #region Validation messages

        public static async Task<IHtmlContent> CustomValidationSummaryAsync(this IHtmlHelper helper,
            bool excludePropertyErrors = true,
            string validationSummaryMessage = "The following errors were detected",
            object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return await helper.PartialAsync("_ValidationSummary");
        }

        #endregion

    }
}
