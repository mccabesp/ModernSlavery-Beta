using System;
using System.IO;
using System.Threading.Tasks;
using ModernSlavery.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using ModernSlavery.WebUI.Shared.Classes;

namespace ModernSlavery.IdentityServer4.Classes
{
    public static class HtmlHelpers
    {

        public static HtmlString PageIdentifier(this IHtmlHelper htmlHelper)
        {
            var globalOptions=htmlHelper.GetGlobalOptions();
            return new HtmlString(
                $"Date:{VirtualDateTime.Now}, Version:{globalOptions.Version}, File Date:{globalOptions.AssemblyDate.ToLocalTime()}, Environment:{globalOptions.Environment}, Machine:{Environment.MachineName}, Instance:{globalOptions.WEBSITE_INSTANCE_ID}, {globalOptions.AssemblyCopyright}");
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
