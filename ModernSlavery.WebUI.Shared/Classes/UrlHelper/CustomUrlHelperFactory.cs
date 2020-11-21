using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Shared.Classes.UrlHelper
{
    public class CustomUrlHelperFactory : IUrlHelperFactory
    {
        private readonly StaticRoutesOptions _urlRoutesOptions;
        public CustomUrlHelperFactory(StaticRoutesOptions urlRoutesOptions)
        {
            _urlRoutesOptions = urlRoutesOptions;
        }
        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            var originalUrlHelperFactory = new UrlHelperFactory();
            var originalUrlHelper = originalUrlHelperFactory.GetUrlHelper(context);
            return new CustomUrlHelper(context, originalUrlHelper, _urlRoutesOptions);
        }
    }
}
